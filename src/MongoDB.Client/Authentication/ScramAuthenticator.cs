using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Connection;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Settings;

namespace MongoDB.Client.Authentication
{
    internal class ScramAuthenticator
    {
        private const int ScramSha256MinimumIterationCount = 4096;
        private static readonly UTF8Encoding Strict = new UTF8Encoding(false, true);

        private readonly MongoClientSettings _settings;
        private readonly Dictionary<ScramCacheKey, ScramCache> _cache = new();
        private readonly object _cacheSync = new();

        public ScramAuthenticator(MongoClientSettings settings)
        {
            _settings = settings;
        }

        public SaslStart? AuthenticateIsMaster(BsonDocument isMasterDocument)
        {
            if (_settings.Login is null)
            {
                return null;
            }

            if (_settings.Password is null)
            {
                ThrowHelper.MongoAuthentificationException(
                    "Authentication requires a password when a login is provided. The current SCRAM implementation only supports password-based authentication.",
                    0);
            }

            return AddLoginInfoToCommand(isMasterDocument, _settings.Login, _settings.AdminDB, ResolveMechanism());
        }

        public async Task AuthenticateAsync(IMongoConnection connection, BsonDocument isMasterResult, SaslStart? saslStart, CancellationToken token)
        {
            if (_settings.Login is null || saslStart is null)
            {
                return;
            }

            if (_settings.Password is null)
            {
                ThrowHelper.MongoAuthentificationException(
                    "Authentication requires a password when a login is provided. The current SCRAM implementation only supports password-based authentication.",
                    0);
            }

            var commandDatabase = $"{_settings.AdminDB}.$cmd";
            var mechanism = ResolveMechanism();
            var initialResponse = isMasterResult.TryGet("speculativeAuthenticate", out var authDataElement)
                ? ParseCommandResponse(GetRequiredDocument(authDataElement, "speculativeAuthenticate"), "speculativeAuthenticate")
                : await SendCommandAsync(
                    connection,
                    commandDatabase,
                    CreateSaslStartCommand(saslStart, mechanism, _settings.AdminDB),
                    "saslStart",
                    null,
                    token).ConfigureAwait(false);

            var continuation = CreateClientFinalCommand(initialResponse.Payload, saslStart, initialResponse.ConversationId, mechanism);
            var response = await SendCommandAsync(
                connection,
                commandDatabase,
                continuation.Command,
                "saslContinue",
                initialResponse.ConversationId,
                token).ConfigureAwait(false);

            var serverSignatureVerified = response.Payload.Length != 0
                && LooksLikeServerFinalMessage(response.Payload)
                && TryValidateServerFinalMessage(response.Payload, continuation.ServerSignature);
            if (response.Done)
            {
                if (!serverSignatureVerified)
                {
                    ThrowHelper.MongoAuthentificationException("SCRAM conversation completed without a server signature.", 0);
                }

                return;
            }

            if (response.Payload.Length != 0 && !serverSignatureVerified)
            {
                ThrowHelper.MongoAuthentificationException("Unexpected SCRAM payload while authentication conversation is still in progress.", 0);
            }

            var finalResponse = await SendCommandAsync(
                connection,
                commandDatabase,
                CreateEmptySaslContinueCommand(response.ConversationId),
                "saslContinue",
                response.ConversationId,
                token).ConfigureAwait(false);

            if (!finalResponse.Done)
            {
                ThrowHelper.MongoAuthentificationException("SCRAM conversation did not complete after the final saslContinue.", 0);
            }

            if (finalResponse.Payload.Length != 0)
            {
                serverSignatureVerified = TryValidateServerFinalMessage(finalResponse.Payload, continuation.ServerSignature);
            }

            if (!serverSignatureVerified)
            {
                ThrowHelper.MongoAuthentificationException("SCRAM conversation completed without a server signature.", 0);
            }
        }

        private static BsonDocument CreateSaslStartCommand(SaslStart saslStart, string mechanism, string database)
        {
            var document = new BsonDocument();
            document.Add("saslStart", 1);
            document.Add("mechanism", mechanism);
            document.Add("payload", BsonBinaryData.Create(saslStart.Payload));
            document.Add("options", new BsonDocument("skipEmptyExchange", true));
            document.Add("db", database);
            return document;
        }

        private (BsonDocument Command, byte[] ServerSignature) CreateClientFinalCommand(byte[] replyBytes, SaslStart saslStart, int conversationId, string mechanism)
        {
            var document = new BsonDocument();
            var serverFirstMessage = Strict.GetString(replyBytes);
            var parsedReply = ParseServerFirstMessage(serverFirstMessage);
            var clientNonce = Strict.GetString(saslStart.Salt);
            if (!parsedReply.Nonce.StartsWith(clientNonce, StringComparison.Ordinal))
            {
                ThrowHelper.MongoAuthentificationException("Server sent an invalid nonce.", 0);
            }

            if (parsedReply.Nonce.Length == clientNonce.Length)
            {
                ThrowHelper.MongoAuthentificationException("Server sent an invalid nonce.", 0);
            }

            const string gs2Header = "n,,";
            var channelBinding = "c=" + Convert.ToBase64String(Strict.GetBytes(gs2Header));
            var nonce = "r=" + parsedReply.Nonce;
            var clientFinalMessageWithoutProof = channelBinding + "," + nonce;

            byte[] salt;
            try
            {
                salt = Convert.FromBase64String(parsedReply.Salt);
            }
            catch (FormatException ex)
            {
                throw new MongoAuthentificationException("SCRAM server-first-message contained an invalid salt.", ex);
            }

            var (clientKey, serverKey) = ComputeKeys(mechanism, parsedReply.IterationCount, salt);

            var storedKey = H256(clientKey);
            var authMessage = saslStart.BaseMessage + "," + serverFirstMessage + "," + clientFinalMessageWithoutProof;
            var clientSignature = Hmac256(Strict, storedKey, authMessage);
            var clientProof = XOR(clientKey, clientSignature);
            var serverSignature = Hmac256(Strict, serverKey, authMessage);
            var proof = "p=" + Convert.ToBase64String(clientProof);
            var clientFinalMessage = clientFinalMessageWithoutProof + "," + proof;
            var bytesToSend = Strict.GetBytes(clientFinalMessage);

            document.Add("saslContinue", 1);
            document.Add("conversationId", conversationId);
            document.Add("payload", BsonBinaryData.Create(bytesToSend));
            return (document, serverSignature);
        }

        private static BsonDocument CreateEmptySaslContinueCommand(int conversationId)
        {
            var document = new BsonDocument();
            document.Add("saslContinue", 1);
            document.Add("conversationId", conversationId);
            document.Add("payload", BsonBinaryData.Create(Array.Empty<byte>()));
            return document;
        }

        private async Task<ScramCommandResponse> SendCommandAsync(
            IMongoConnection connection,
            string database,
            BsonDocument command,
            string operationName,
            int? expectedConversationId,
            CancellationToken token)
        {
            var queryResult = await connection.SendQueryAsync<BsonDocument>(database, command, token).ConfigureAwait(false);
            if (queryResult.Count == 0)
            {
                ThrowHelper.MongoAuthentificationException($"SCRAM {operationName} did not return a response document.", 0);
            }

            return ParseCommandResponse(queryResult[0], operationName, expectedConversationId);
        }

        private static ScramCommandResponse ParseCommandResponse(BsonDocument document, string operationName, int? expectedConversationId = null)
        {
            if (document.TryGet("ok", out var okElement) && TryGetDouble(okElement, out var okValue) && okValue == 0)
            {
                var message = document.TryGet("errmsg", out var messageElement) && messageElement.AsString is { Length: > 0 } errorMessage
                    ? errorMessage
                    : $"SCRAM {operationName} failed.";
                var code = document.TryGet("code", out var codeElement) && TryGetInt32(codeElement, out var parsedCode)
                    ? parsedCode
                    : 0;
                ThrowHelper.MongoAuthentificationException(message, code);
            }

            var conversationId = GetRequiredInt32(document, "conversationId", operationName);
            if (expectedConversationId.HasValue && conversationId != expectedConversationId.Value)
            {
                ThrowHelper.MongoAuthentificationException("SCRAM conversationId changed unexpectedly.", 0);
            }

            var done = GetRequiredBoolean(document, "done", operationName);
            var payload = GetRequiredBinary(document, "payload", operationName);
            return new ScramCommandResponse(conversationId, done, payload);
        }

        private static bool TryValidateServerFinalMessage(byte[] payload, byte[] expectedServerSignature)
        {
            if (payload.Length == 0)
            {
                return false;
            }

            var finalMessage = Strict.GetString(payload);
            var fields = ParseScramFields(finalMessage, "server-final-message");
            if (fields.TryGetValue("e", out var serverError) && !string.IsNullOrEmpty(serverError))
            {
                ThrowHelper.MongoAuthentificationException($"SCRAM server rejected authentication: {serverError}", 0);
            }

            if (!fields.TryGetValue("v", out var signature) || string.IsNullOrEmpty(signature))
            {
                ThrowHelper.MongoAuthentificationException("SCRAM server-final-message did not contain a server signature.", 0);
            }

            byte[] receivedServerSignature;
            try
            {
                receivedServerSignature = Convert.FromBase64String(signature);
            }
            catch (FormatException ex)
            {
                throw new MongoAuthentificationException("SCRAM server-final-message contained an invalid server signature.", ex);
            }

            if (!expectedServerSignature.AsSpan().SequenceEqual(receivedServerSignature))
            {
                ThrowHelper.MongoAuthentificationException("Server signature was invalid.", 0);
            }

            return true;
        }

        private static bool LooksLikeServerFinalMessage(byte[] payload)
        {
            if (payload.Length < 2)
            {
                return false;
            }

            return (payload[0] == (byte)'v' || payload[0] == (byte)'e' || payload[0] == (byte)'m')
                && payload[1] == (byte)'=';
        }

        private (byte[] clientKey, byte[] serverKey) ComputeKeys(string mechanism, int i, byte[] salt)
        {
            var preparedPassword = SaslPrep.Prepare(_settings.Password!);
            var cacheKey = new ScramCacheKey(mechanism, preparedPassword, Convert.ToBase64String(salt), i);

            lock (_cacheSync)
            {
                if (_cache.TryGetValue(cacheKey, out var cache))
                {
                    return (cache.ClientKey, cache.ServerKey);
                }

                var passBytes = Hi(preparedPassword, salt, i);
                var clientKey = Hmac256(Strict, passBytes, "Client Key");
                var serverKey = Hmac256(Strict, passBytes, "Server Key");
                _cache[cacheKey] = new ScramCache(clientKey, serverKey);
                return (clientKey, serverKey);
            }
        }

        private static SaslStart CreateScramLoginBytes(string login)
        {
            var prepared = PrepareLogin(login);
            var randomBytes = GenerateRandomBytes(20);

            var bareMessage = "n=" + Strict.GetString(prepared) + "," + "r=" + Strict.GetString(randomBytes);
            var len = 5 + 3 + prepared.Length + 20;
            var bytes = new byte[len];
            Span<byte> span = bytes;
            var loginSlice = span.Slice(5, prepared.Length);
            var randomSlice = span.Slice(5 + prepared.Length + 3, 20);
            bytes[0] = 110;
            bytes[1] = 44;
            bytes[2] = 44;
            bytes[3] = 110;
            bytes[4] = 61;
            prepared.CopyTo(loginSlice);
            bytes[5 + prepared.Length] = 44;
            bytes[5 + prepared.Length + 1] = 114;
            bytes[5 + prepared.Length + 2] = 61;
            randomBytes.CopyTo(randomSlice);

            return new SaslStart(randomBytes, bareMessage, bytes);
        }

        private static byte[] GenerateRandomBytes(int len)
        {
            const string legalCharacters = "!\"#$%&'()*+-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
            var rnd = new Random();
            var array = new byte[len];
            for (int i = 0; i < len; i++)
            {
                var idx = rnd.Next(0, legalCharacters.Length);
                array[i] = (byte)legalCharacters[idx];
            }

            return array;
        }

        private static byte[] PrepareLogin(string login)
        {
            var rawLogin = Encoding.UTF8.GetBytes(login);
            var badBytesCount = 0;
            foreach (var currentByte in rawLogin)
            {
                if (currentByte == 0)
                {
                    ThrowHelper.MongoAuthentificationException(
                        "SCRAM username must not contain NUL.",
                        0);
                }

                if (currentByte == 61 || currentByte == 44)
                {
                    badBytesCount += 1;
                }
            }

            if (badBytesCount == 0)
            {
                return rawLogin;
            }

            var escapedLogin = new byte[rawLogin.Length + (badBytesCount * 2)];
            var escapedIndex = 0;
            foreach (var currentByte in rawLogin)
            {
                switch (currentByte)
                {
                    case 61:
                        escapedLogin[escapedIndex++] = 61;
                        escapedLogin[escapedIndex++] = 51;
                        escapedLogin[escapedIndex++] = 68;
                        break;
                    case 44:
                        escapedLogin[escapedIndex++] = 61;
                        escapedLogin[escapedIndex++] = 50;
                        escapedLogin[escapedIndex++] = 67;
                        break;
                    default:
                        escapedLogin[escapedIndex++] = currentByte;
                        break;
                }
            }

            return escapedLogin;
        }

        private static byte[] Hi(string password, Span<byte> salt, int iterations)
        {
            return Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 32);
        }

        private static byte[] Hmac256(UTF8Encoding encoding, byte[] data, string key)
        {
            using (var hmac = new HMACSHA256(data))
            {
                return hmac.ComputeHash(encoding.GetBytes(key));
            }
        }

        private static byte[] H256(byte[] data)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(data);
            }
        }

        private static byte[] XOR(byte[] a, byte[] b)
        {
            var result = new byte[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                result[i] = (byte)(a[i] ^ b[i]);
            }

            return result;
        }

        private static ScramServerFirstMessage ParseServerFirstMessage(string payload)
        {
            var fields = ParseScramFields(payload, "server-first-message");
            fields.TryGetValue("r", out var nonce);
            if (string.IsNullOrEmpty(nonce))
            {
                ThrowHelper.MongoAuthentificationException("SCRAM server-first-message did not contain a nonce.", 0);
            }

            fields.TryGetValue("s", out var salt);
            if (string.IsNullOrEmpty(salt))
            {
                ThrowHelper.MongoAuthentificationException("SCRAM server-first-message did not contain a salt.", 0);
            }

            var iterations = 0;
            if (!fields.TryGetValue("i", out var iterationText) || !int.TryParse(iterationText, out iterations) || iterations <= 0)
            {
                ThrowHelper.MongoAuthentificationException("SCRAM server-first-message did not contain a valid iteration count.", 0);
            }

            if (iterations < ScramSha256MinimumIterationCount)
            {
                ThrowHelper.MongoAuthentificationException(
                    $"SCRAM server-first-message iteration count must be at least {ScramSha256MinimumIterationCount} for SCRAM-SHA-256.",
                    0);
            }

            return new ScramServerFirstMessage(nonce!, salt!, iterations);
        }

        private static Dictionary<string, string> ParseScramFields(string payload, string messageName)
        {
            if (string.IsNullOrEmpty(payload))
            {
                ThrowHelper.MongoAuthentificationException($"SCRAM {messageName} payload was empty.", 0);
            }

            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var field in payload.Split(','))
            {
                var separatorIndex = field.IndexOf('=');
                if (separatorIndex <= 0 || separatorIndex == field.Length - 1)
                {
                    ThrowHelper.MongoAuthentificationException($"SCRAM {messageName} contained an invalid field '{field}'.", 0);
                }

                var attributeName = field.Substring(0, separatorIndex);
                if (string.Equals(attributeName, "m", StringComparison.Ordinal))
                {
                    ThrowHelper.MongoAuthentificationException($"SCRAM {messageName} contained the reserved extension attribute 'm'.", 0);
                }

                result[attributeName] = field.Substring(separatorIndex + 1);
            }

            return result;
        }

        private static BsonDocument GetRequiredDocument(BsonElement element, string fieldName)
        {
            var document = element.AsBsonDocument;
            if (document is null)
            {
                ThrowHelper.MongoAuthentificationException($"SCRAM response field '{fieldName}' was missing or had an invalid type.", 0);
            }

            return document!;
        }

        private static int GetRequiredInt32(BsonDocument document, string fieldName, string operationName)
        {
            var value = 0;
            if (!document.TryGet(fieldName, out var element) || !TryGetInt32(element, out value))
            {
                ThrowHelper.MongoAuthentificationException($"SCRAM {operationName} response did not include a valid '{fieldName}' field.", 0);
            }

            return value;
        }

        private static bool GetRequiredBoolean(BsonDocument document, string fieldName, string operationName)
        {
            if (!document.TryGet(fieldName, out var element))
            {
                ThrowHelper.MongoAuthentificationException($"SCRAM {operationName} response did not include a valid '{fieldName}' field.", 0);
            }

            var rawValue = element.Value;
            if (rawValue is not bool)
            {
                ThrowHelper.MongoAuthentificationException($"SCRAM {operationName} response did not include a valid '{fieldName}' field.", 0);
            }

            return (bool)rawValue!;
        }

        private static byte[] GetRequiredBinary(BsonDocument document, string fieldName, string operationName)
        {
            byte[]? value = null;
            if (!document.TryGet(fieldName, out var element) || (value = element.AsByteArray) is null)
            {
                ThrowHelper.MongoAuthentificationException($"SCRAM {operationName} response did not include a valid '{fieldName}' field.", 0);
            }

            return value!;
        }

        private static bool TryGetInt32(BsonElement element, out int value)
        {
            if (element.Value is int intValue)
            {
                value = intValue;
                return true;
            }

            value = default;
            return false;
        }

        private static bool TryGetDouble(BsonElement element, out double value)
        {
            switch (element.Value)
            {
                case double doubleValue:
                    value = doubleValue;
                    return true;
                case int intValue:
                    value = intValue;
                    return true;
                default:
                    value = default;
                    return false;
            }
        }

        private string ResolveMechanism()
        {
            var mechanism = _settings.AuthMechanism;
            if (string.IsNullOrWhiteSpace(mechanism))
            {
                return "SCRAM-SHA-256";
            }

            if (string.Equals(mechanism, "SCRAM-SHA-256", StringComparison.Ordinal))
            {
                return mechanism;
            }

            ThrowHelper.MongoAuthentificationException(
                $"Authentication mechanism '{mechanism}' is not supported by the current SCRAM implementation.",
                0);
            return string.Empty;
        }

        private static SaslStart AddLoginInfoToCommand(BsonDocument command, string login, string db, string mechanism)
        {
            command.Add("saslSupportedMechs", $"{db}.{login}");
            BsonDocument speculativeAuthenticate = new BsonDocument();
            speculativeAuthenticate.Add("saslStart", 1);
            speculativeAuthenticate.Add("mechanism", mechanism);
            var saslStart = CreateScramLoginBytes(login);
            speculativeAuthenticate.Add("payload", BsonBinaryData.Create(saslStart.Payload));
            speculativeAuthenticate.Add("options", new BsonDocument("skipEmptyExchange", true));
            speculativeAuthenticate.Add("db", db);
            command.Add("speculativeAuthenticate", speculativeAuthenticate);
            return saslStart;
        }

        private class ScramCache
        {
            public ScramCache(byte[] clientKey, byte[] serverKey)
            {
                ClientKey = clientKey;
                ServerKey = serverKey;
            }

            public byte[] ClientKey { get; }
            public byte[] ServerKey { get; }
        }

        private sealed record ScramCacheKey(string Mechanism, string PreparedPassword, string Salt, int IterationCount);
        private sealed record ScramCommandResponse(int ConversationId, bool Done, byte[] Payload);
        private sealed record ScramServerFirstMessage(string Nonce, string Salt, int IterationCount);
    }
}

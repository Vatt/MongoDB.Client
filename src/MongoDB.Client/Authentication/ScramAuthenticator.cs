using System;
using System.Buffers.Binary;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Connection;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Settings;

namespace MongoDB.Client.Authentication
{
    internal class ScramAuthenticator
    {
        private static readonly UTF8Encoding Strict = new UTF8Encoding(false, true);

        private readonly MongoClientSettings _settings;
        private ScramCache? _cache;

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

            return AddLoginInfoToCommand(isMasterDocument, _settings.Login, _settings.AdminDB);
        }

        public async Task AuthenticateAsync(IMongoConnection connection, BsonDocument isMasterResult, SaslStart? saslStart, CancellationToken token)
        {
            if (isMasterResult.TryGet("speculativeAuthenticate", out var authDataElement) && saslStart is not null)
            {
                var authData = authDataElement.AsBsonDocument!;
                var payload = authData["payload"].AsByteArray!;

                var conversationId = authData["conversationId"].AsInt;
                var (saslDoc, serverSignature) = CreateSaslStart(payload, saslStart!, conversationId);
                var queryResult = await connection.SendQueryAsync<BsonDocument>("admin.$cmd", saslDoc, token).ConfigureAwait(false);
                var result = queryResult[0];
                var isOk = result["ok"].AsDouble;
                if (isOk == 0)
                {
                    ThrowHelper.MongoAuthentificationException(result["errmsg"].ToString(), result["code"].AsInt);
                }

                payload = result["payload"].AsByteArray!;
                var v = ParseV(Strict.GetString(payload));
                var receivedServerSignature = Convert.FromBase64String(v);

                if (!ConstantTimeEquals(serverSignature, receivedServerSignature))
                {
                    ThrowHelper.MongoAuthentificationException("Server signature was invalid.", 0);
                }
            }
        }

        private bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            var diff = a.Length ^ b.Length;
            for (var i = 0; i < a.Length && i < b.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }

            return diff == 0;
        }

        private string ParseV(string payload)
        {
            return payload.Substring(2);
        }

        private (BsonDocument, byte[]) CreateSaslStart(byte[] replyBytes, SaslStart saslStart, int conversationId)
        {
            var document = new BsonDocument();
            ParseReplyScramBytes(replyBytes, out var rb, out _, out _);
            var prefixCheck = rb.Slice(0, saslStart.Salt.Length);
            if (prefixCheck.SequenceEqual(saslStart.Salt) == false)
            {
                ThrowHelper.MongoAuthentificationException("Server sent an invalid nonce.", 0);
            }
            var serverFirstMessage = Strict.GetString(replyBytes);
            ParseReplyScramBytes(serverFirstMessage, out var r, out var s, out var i);

            const string gs2Header = "n,,";
            var channelBinding = "c=" + Convert.ToBase64String(Strict.GetBytes(gs2Header));
            var nonce = "r=" + r;
            var clientFinalMessageWithoutProof = channelBinding + "," + nonce;

            Span<byte> salt = Convert.FromBase64String(s);

            var (clientKey, serverKey) = ComputeKeys(i, salt);

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

        private (byte[] clientKey, byte[] serverKey) ComputeKeys(int i, Span<byte> salt)
        {
            byte[] clientKey;
            byte[] serverKey;
            var cache = Volatile.Read(ref _cache);
            if (cache is null)
            {
                var passBytes = Hi(_settings.Password!, salt, i);
                clientKey = Hmac256(Strict, passBytes, "Client Key");
                serverKey = Hmac256(Strict, passBytes, "Server Key");
                cache = new ScramCache(clientKey, serverKey);
                Volatile.Write(ref _cache, cache);
            }
            else
            {
                clientKey = cache.ClientKey;
                serverKey = cache.ServerKey;
            }

            return (clientKey, serverKey);
        }

        private static SaslStart CreateScramLoginBytes(byte[] login)
        {
            var prepared = PrepareLogin(login);
            var randonBytes = GenerateRandonBytes(20);

            var bareMessage = "n=" + Strict.GetString(prepared) + "," + "r=" + Strict.GetString(randonBytes);
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
            randonBytes.CopyTo(randomSlice);

            return new SaslStart(randonBytes, bareMessage, bytes);
        }

        private static byte[] GenerateRandonBytes(int len)
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

        private static byte[] PrepareLogin(byte[] rawLogin)
        {
            var badBytesCount = 0;
            var span = rawLogin;
            for (int i = 0; i < rawLogin.Length - 1; i++)
            {
                if (span[i] == 61 || span[i] == 44)
                {
                    badBytesCount += 1;
                }
            }

            if (badBytesCount == 0)
            {
                return rawLogin;
            }

            var newLen = rawLogin.Length + (badBytesCount * 2);
            var newBytes = new byte[newLen];
            var newIdx = 0;
            for (int i = 0; i < rawLogin.Length - 1; i++)
            {
                var oldByte = span[i];
                switch (oldByte)
                {
                    case 61:
                        newBytes[++newIdx] = 61;
                        newBytes[++newIdx] = 51;
                        newBytes[++newIdx] = 68;
                        break;
                    case 44:
                        newBytes[++newIdx] = 61;
                        newBytes[++newIdx] = 50;
                        newBytes[++newIdx] = 67;
                        break;
                    default:
                        newBytes[++newIdx] = span[i];
                        break;
                }
            }

            return newBytes;
        }
        

        private static byte[] Hi(byte[] passBytes, Span<byte> salt, int iterations)
        {
            using var hmac = new HMACSHA256(passBytes);
            var hashed = ComputeHash(hmac, salt, iterations, out var block);
            var asd = string.Join(string.Empty, hashed.Select(b => b.ToString("X2")));
            return hashed;
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

        private static byte[] ComputeHash(HMAC algorithm, Span<byte> salt, int iterations, out int block)
        {
            //HMAC SHA256 hash size - 256
            const int blockSize = 256 >> 3;
            block = 1;
            var bytes = new byte[salt.Length + sizeof(uint)];
            Span<byte> span = bytes;
            salt.CopyTo(bytes);
            BinaryPrimitives.WriteInt32BigEndian(span.Slice(28), block);
            bytes = algorithm.ComputeHash(bytes);
            var result = bytes;
            for (int i = 1; i < iterations; i++)
            {
                bytes = algorithm.ComputeHash(bytes);
                for (int j = 0; j < blockSize; j++)
                {
                    result[j] ^= bytes[j];
                }
            }
            block++;
            return result;
        }
        private static void ParseReplyScramBytes(byte[] bytes, out Span<byte> r, out Span<byte> s, out int i)
        {
            r = default;
            s = default;
            i = default;
            var span = bytes.AsSpan();
            int rEnd = 0;
            int sStart = 0;
            int sEnd = 0;
            int iStart = 0;
            for (int index = 0; i < bytes.Length; index++)
            {
                if (span[index] == 44)
                {
                    if (span[index + 1] == 115 && span[index + 2] == 61)
                    {
                        rEnd = index;
                        sStart = index + 3;
                    }
                    if (span[index + 1] == 105 && span[index + 2] == 61)
                    {
                        sEnd = index;
                        iStart = index + 3;
                        break;
                    }
                }
            }
            r = span.Slice(2, rEnd - 2);
            s = span.Slice(sStart, sEnd - sStart);
            var iSpan = span.Slice(iStart, bytes.Length - iStart);
            i = int.Parse(Encoding.UTF8.GetString(iSpan));

        }

        private static void ParseReplyScramBytes(ReadOnlySpan<char> bytes, out string r, out string s, out int i)
        {
            i = default;
            var span = bytes;
            int rEnd = 0;
            int sStart = 0;
            int sEnd = 0;
            int iStart = 0;
            for (int index = 0; i < bytes.Length; index++)
            {
                if (span[index] == 44)
                {
                    if (span[index + 1] == 115 && span[index + 2] == 61)
                    {
                        rEnd = index;
                        sStart = index + 3;
                    }
                    if (span[index + 1] == 105 && span[index + 2] == 61)
                    {
                        sEnd = index;
                        iStart = index + 3;
                        break;
                    }
                }
            }
            r = new string(span.Slice(2, rEnd - 2));
            s = new string(span.Slice(sStart, sEnd - sStart));
            var iSpan = span.Slice(iStart, bytes.Length - iStart);
            i = int.Parse(iSpan);
        }

        private static SaslStart AddLoginInfoToCommand(BsonDocument command, byte[] login, string db)
        {
            var loginStr = Encoding.UTF8.GetString(login);
            command.Add("saslSupportedMechs", $"{db}.{loginStr}");
            BsonDocument speculativeAuthenticate = new BsonDocument();
            speculativeAuthenticate.Add("saslStart", 1);
            speculativeAuthenticate.Add("mechanism", "SCRAM-SHA-256");
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
    }
}

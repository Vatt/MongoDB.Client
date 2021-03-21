using System;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Settings;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Buffers.Binary;
using MongoDB.Client.Exceptions;

namespace MongoDB.Client.Connection
{
    internal static class InitHelper
    {
        private static byte[] _rPrefix;
        public static BsonDocument CreateInitialCommand(MongoClientSettings settings)
        {
            var command = CreateCommand();
            AddClientDocumentToCommand(command, settings.ApplicationName ?? string.Empty);
            AddCompressorsToCommand(command, Compressors);
            if (settings.Login is not null && settings.Password is not null)
            {
                AddLoginInfoToCommand(command, settings.Login, settings.Password, settings.AdminDB);
            }
            return command;
        }
        private static byte[] CreateScramLoginBytes(byte[] login)
        {
            var prepared = PrepareLogin(login);
            var randonBytes = GenerateRandonBytes(20);
            _rPrefix = randonBytes;
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
            return bytes;
        }

        private static byte[] GenerateRandonBytes(int len)
        {
            var rnd = new Random();
            var array = new byte[len];
            for (int i = 0; i < len; i++)
            {
                array[i] = (byte)rnd.Next(33, 126);
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
        public static BsonDocument CreateSaslStart(MongoClientSettings settings, byte[] replyBytes)
        {
            var document = new BsonDocument();
            ParseReplyScramBytes(replyBytes, out var r, out var s, out var i);
            var prefixCheck = r.Slice(0, _rPrefix.Length);
            if(prefixCheck.SequenceEqual(_rPrefix) == false)
            {
                ThrowHelper.MongoAuthentificationException("Server sent an invalid nonce.", 0);
            }
            Span<byte> clientFinalMessageWithoutProof = new byte[9 + r.Length];
            clientFinalMessageWithoutProof[0] = 99;
            clientFinalMessageWithoutProof[1] = 61;
            clientFinalMessageWithoutProof[2] = 98;
            clientFinalMessageWithoutProof[3] = 105;
            clientFinalMessageWithoutProof[4] = 119;
            clientFinalMessageWithoutProof[5] = 115;
            clientFinalMessageWithoutProof[6] = 44;
            clientFinalMessageWithoutProof[7] = 114;
            clientFinalMessageWithoutProof[8] = 61;
            r.CopyTo(clientFinalMessageWithoutProof.Slice(9));

            Span<byte> salt = Convert.FromBase64String(Encoding.UTF8.GetString(s));

            byte[] clientKey;
            byte[] serverKey;

            var passBytes = Hi(settings.Password, salt, i);
            document.Add("saslContinue", 1);
            document.Add("conversationId", 1);
            document.Add("payload", BsonBinaryData.Create(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes("n,,n=gamover,r=.`@cYWf%6FKEx?WCtDE9"))));
            //document.Add("payload", BsonBinaryData.Create(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(settings.Login))));
            //document.Add("payload", BsonBinaryData.Create(Encoding.UTF8.GetBytes("c=biws,r=.`@cYWf%6FKEx?WCtDE96MxRvkhr3OD7pNbXztr9wSPdUNY4J1sh,p=4Xgx4lMurUvq/iMPdX0/V41bdqzHwFSFUn59OwBdkFc=")));
            return document;
        }
        private static byte[] Hi(Span<byte> passBytes, Span<byte> salt, int iterations)
        {
            var hashed = ComputeHash(salt, iterations, out var block);

            return default;
        }
        private static byte[] ComputeHash(Span<byte> salt, int iterations, out int block)
        {
            //HMAC SHA256 hash size - 256
            const int blockSize = 256 >> 3;
            block = 1;
            var bytes = new byte[salt.Length + sizeof(uint)];
            Span<byte> span = bytes;
            salt.CopyTo(bytes);
            BinaryPrimitives.WriteInt32BigEndian(span.Slice(28), block);
            var algorithm = HMAC.Create("HMACSHA256");
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
            for(int index = 0; i < bytes.Length; index++)
            {
                if (span[index] == 44)
                {
                    if(span[index + 1] == 115 && span[index + 2] == 61)
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
        private static void AddLoginInfoToCommand(BsonDocument command, byte[] login, byte[] password, string db)
        {
            var loginStr = Encoding.UTF8.GetString(login);
            command.Add("saslSupportedMechs", $"{db}.{loginStr}");
            BsonDocument speculativeAuthenticate = new BsonDocument();
            speculativeAuthenticate.Add("saslStart", 1);
            speculativeAuthenticate.Add("mechanism", "SCRAM-SHA-256");
            speculativeAuthenticate.Add("payload", BsonBinaryData.Create(CreateScramLoginBytes(login)));
            //speculativeAuthenticate.Add("payload", BsonBinaryData.Create(Encoding.UTF8.GetBytes("n,,n=gamover,r=.`@cYWf%6FKEx?WCtDE9")));
            speculativeAuthenticate.Add("options", new BsonDocument("skipEmptyExchange", true));
            speculativeAuthenticate.Add("db", db);
            command.Add("speculativeAuthenticate", speculativeAuthenticate);
        }
        private static void AddClientDocumentToCommand(BsonDocument command, string appname)
        {
            var clientDocument = CreateClientDocument(appname);
            command.Add("client", clientDocument);
        }

        private static void AddCompressorsToCommand(BsonDocument command, BsonDocument compressors)
        {
            command.Add("compression", compressors);
        }

        private static BsonDocument CreateCommand()
        {
            BsonDocument? topologyVersion = null;
            //long? maxAwaitTimeMS = null;
            return new BsonDocument
            {
                { "isMaster", 1 },
                { "topologyVersion", topologyVersion, topologyVersion != null },
               // { "maxAwaitTimeMS", maxAwaitTimeMS, maxAwaitTimeMS.HasValue }
            };
        }

        private static readonly BsonArray Compressors = new BsonArray();

        private static BsonDocument CreateClientDocument(string applicationName)
        {
            var driverDocument = CreateDriverDocument();
            var osDocument = CreateOSDocument();
            var platformString = GetPlatformString();

            return CreateClientDocument(applicationName, driverDocument, osDocument, platformString);
        }



        private static BsonDocument CreateClientDocument(string applicationName, BsonDocument driverDocument, BsonDocument osDocument, string platformString)
        {
            var clientDocument = new BsonDocument
            {
                { "application", applicationName == null ? null : new BsonDocument("name", applicationName) },
                { "driver", driverDocument },
                { "os", osDocument },
                { "platform", platformString }
            };

            return clientDocument;
        }

        private static BsonDocument CreateDriverDocument()
        {
            return new BsonDocument
            {
                { "name", "dotnet-mongo-driver" },
                { "version", "0.1.0.0" }
            };
        }


        private static BsonDocument CreateOSDocument()
        {
            string osType;
            string osName;
            string? architecture;
            string? osVersion;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                osType = "Windows";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                osType = "Linux";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                osType = "macOS";
            }
            else
            {
                osType = "unknown";
            }

            osName = RuntimeInformation.OSDescription.Trim();

            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.Arm: architecture = "arm"; break;
                case Architecture.Arm64: architecture = "arm64"; break;
                case Architecture.X64: architecture = "x86_64"; break;
                case Architecture.X86: architecture = "x86_32"; break;
                default: architecture = null; break;
            }

            var match = Regex.Match(osName, @" (?<version>\d+\.\d[^ ]*)");
            if (match.Success)
            {
                osVersion = match.Groups["version"].Value;
            }
            else
            {
                osVersion = null;
            }

            return CreateOSDocument(osType, osName, architecture, osVersion);
        }

        private static BsonDocument CreateOSDocument(string osType, string osName, string? architecture, string? osVersion)
        {
            return new BsonDocument
            {
                { "type", osType },
                { "name", osName },
                { "architecture", architecture},
                { "version", osVersion }
            };
        }

        private static string GetPlatformString()
        {
            return RuntimeInformation.FrameworkDescription;
        }
    }
}

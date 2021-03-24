namespace MongoDB.Client.Authentication
{
    internal class SaslStart
    {
        public byte[] Salt { get; }
        public string BaseMessage { get; }
        public byte[] Payload { get; }

        public SaslStart(byte[] salt, string baseMessage, byte[] payload)
        {
            Salt = salt;
            BaseMessage = baseMessage;
            Payload = payload;
        }
    }
}

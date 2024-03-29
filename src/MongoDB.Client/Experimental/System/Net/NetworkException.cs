// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Serialization;

namespace System.Net
{
    /// <summary>Provides socket exceptions to the application.</summary>
    [Serializable]
    public class NetworkException : IOException
    {
        /// <summary>Creates a new instance of the <see cref='System.Net.NetworkException'/> class with the specified error code.</summary>
        public NetworkException(NetworkError error, Exception? innerException = null)
            : this(GetExceptionMessage(error), error, innerException) { }

        /// <summary>Creates a new instance of the <see cref='System.Net.NetworkException'/> class with the specified error code and message.</summary>
        public NetworkException(string message, NetworkError error, Exception? innerException = null)
            : base(message, innerException)
        {
            NetworkError = error;
        }

        /// <summary>Creates a new instance of the <see cref='System.Net.NetworkException'/> from serialized data.</summary>
        protected NetworkException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
            NetworkError = (NetworkError)serializationInfo.GetInt32("NetworkError");
        }

        /// <summary>Populates the serialization data for this object.</summary>
        public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            base.GetObjectData(serializationInfo, streamingContext);
            serializationInfo.AddValue("NetworkError", (int)NetworkError);
        }

        /// <summary>Returns the specific kind of error.</summary>
        public NetworkError NetworkError { get; }

        // TODO: Better exception messages
        private static string GetExceptionMessage(NetworkError error) => $"A network error occurred: {error}";
    }
}

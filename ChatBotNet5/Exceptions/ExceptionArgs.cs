using System;

namespace ChatBot_Net5.Exceptions
{
    [Serializable]
    public class NoUserDataException : Exception
    {
        public NoUserDataException(string message) : base(message)
        {

        }

        public NoUserDataException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public NoUserDataException()
        {
        }

        protected NoUserDataException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }

}

using System.Diagnostics.CodeAnalysis;

namespace Mimoto.Exceptions
{
    [ExcludeFromCodeCoverage]
    [System.Serializable]
    public class UnknownExternalUserIdException : System.Exception
    {
        public UnknownExternalUserIdException() { }
        public UnknownExternalUserIdException(string message) : base(message) { }
        public UnknownExternalUserIdException(string message, System.Exception inner) : base(message, inner) { }
        protected UnknownExternalUserIdException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
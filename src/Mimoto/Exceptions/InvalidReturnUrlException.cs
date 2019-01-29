using System.Diagnostics.CodeAnalysis;

namespace Mimoto.Exceptions
{
    [ExcludeFromCodeCoverage]
    [System.Serializable]
    public class InvalidReturnUrlException : System.Exception
    {
        public InvalidReturnUrlException() : base("invalid return URL") { }
        public InvalidReturnUrlException(string message) : base(message) { }
        public InvalidReturnUrlException(string message, System.Exception inner) : base(message, inner) { }
        protected InvalidReturnUrlException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
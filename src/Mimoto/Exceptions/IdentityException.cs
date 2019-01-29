using System.Diagnostics.CodeAnalysis;

namespace Mimoto.Exceptions
{
    [ExcludeFromCodeCoverage]
    [System.Serializable]
    public class IdentityException : System.Exception
    {
        public IdentityException() { }
        public IdentityException(string message) : base(message) { }
        public IdentityException(string message, System.Exception inner) : base(message, inner) { }
        protected IdentityException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
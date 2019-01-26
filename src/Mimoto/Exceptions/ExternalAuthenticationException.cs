namespace Mimoto.Exceptions
{
    [System.Serializable]
    public class ExternalAuthenticationException : System.Exception
    {
        public ExternalAuthenticationException() : base("External authentication error") { }
        public ExternalAuthenticationException(string message) : base(message) { }
        public ExternalAuthenticationException(string message, System.Exception inner) : base(message, inner) { }
        protected ExternalAuthenticationException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
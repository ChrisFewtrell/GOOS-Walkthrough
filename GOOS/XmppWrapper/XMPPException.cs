using System;
using System.Runtime.Serialization;

namespace GOOSTests
{
    [Serializable]
    public class XMPPException : Exception, ISerializable
    {
        public XMPPException()
        {}

        public XMPPException(string message) : base(message)
        {}

        public XMPPException(string message, Exception innerException) : base(message, innerException)
        {}

        protected XMPPException(SerializationInfo info, StreamingContext context) : base(info, context)
        {}
    }
}
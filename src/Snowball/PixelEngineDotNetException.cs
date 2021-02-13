using System;
using System.Runtime.Serialization;

namespace Snowball
{
    [Serializable]
    internal class SnowballException : Exception
    {
        public SnowballException()
        {
        }

        public SnowballException(string message)
            : base(message)
        {
        }

        public SnowballException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected SnowballException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

using System;
using System.Runtime.Serialization;

namespace PixelCannon
{
    [Serializable]
    internal class PixelCannonException : Exception
    {
        public PixelCannonException()
        {
        }

        public PixelCannonException(string message)
            : base(message)
        {
        }

        public PixelCannonException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected PixelCannonException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

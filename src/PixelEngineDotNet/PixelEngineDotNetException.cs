using System;
using System.Runtime.Serialization;

namespace PixelEngineDotNet
{
    [Serializable]
    internal class PixelEngineDotNetException : Exception
    {
        public PixelEngineDotNetException()
        {
        }

        public PixelEngineDotNetException(string message)
            : base(message)
        {
        }

        public PixelEngineDotNetException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected PixelEngineDotNetException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

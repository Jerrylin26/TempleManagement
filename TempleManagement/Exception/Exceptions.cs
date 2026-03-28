using System;

namespace TempleManagement.Exceptions
{
    public class NeedNameChineseException : Exception
    {
        public string ErrorCode { get; }

        public NeedNameChineseException(string message, string errorCode = null)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public NeedNameChineseException(string message, string errorCode, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }

    public class NeedPriceException : Exception
    {
        public string ErrorCode { get; }

        public NeedPriceException(string message, string errorCode = null)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public NeedPriceException(string message, string errorCode, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}

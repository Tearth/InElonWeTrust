using System;

namespace InElonWeTrust.Core.Services.Cache.Exceptions
{
    public class NoDataProviderException : Exception
    {
        public NoDataProviderException()
        {
        }

        public NoDataProviderException(string message) : base(message)
        {
        }

        public NoDataProviderException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

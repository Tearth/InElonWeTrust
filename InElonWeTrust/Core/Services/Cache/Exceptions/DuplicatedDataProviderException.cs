using System;

namespace InElonWeTrust.Core.Services.Cache.Exceptions
{
    public class DuplicatedDataProviderException : Exception
    {
        public DuplicatedDataProviderException()
        {
        }

        public DuplicatedDataProviderException(string message) : base(message)
        {
        }

        public DuplicatedDataProviderException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

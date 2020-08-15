using System;

namespace NubeSync.Client
{
    public class StoreOperationFailedException : Exception
    {
        public StoreOperationFailedException()
        {
        }

        public StoreOperationFailedException(string message) : base(message)
        {
        }

        public StoreOperationFailedException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
using System;

namespace NubeSync.Client
{
    public class PushOperationFailedException : Exception
    {
        public PushOperationFailedException()
        {
        }

        public PushOperationFailedException(string message) : base(message)
        {
        }

        public PushOperationFailedException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
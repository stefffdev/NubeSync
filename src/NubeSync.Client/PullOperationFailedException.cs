using System;

namespace NubeSync.Client
{
    public class PullOperationFailedException : Exception
    {
        public PullOperationFailedException()
        {
        }

        public PullOperationFailedException(string message) : base(message)
        {
        }

        public PullOperationFailedException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
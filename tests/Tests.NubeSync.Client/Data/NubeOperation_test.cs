using System;
using NubeSync.Client.Data;
using Xunit;

namespace Tests.NubeSync.Client.Data.NubeOperation_test
{
    public class Always
    {
        [Fact]
        public void CreatedAt_is_set()
        {
            var operation = new NubeOperation();

            Assert.True(operation.CreatedAt > DateTimeOffset.Now.AddSeconds(-1));
        }

        [Fact]
        public void Id_is_generated()
        {
            var operation = new NubeOperation();

            Assert.NotEmpty(operation.Id);
        }
    }
}
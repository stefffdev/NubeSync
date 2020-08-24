using System;
using NSubstitute;
using NubeSync.Client;
using NubeSync.Core;

namespace Tests.NubeSync.Client
{
    public static class TestFactory
    {
        public static INubeAuthentication CreateAuthentication()
        {
            return Substitute.For<INubeAuthentication>();
        }

        public static IChangeTracker CreateChangeTracker()
        {
            return Substitute.For<IChangeTracker>();
        }

        public static IDataStore CreateDataStore()
        {
            return Substitute.For<IDataStore>();
        }

        public static TestItem CreateTestItem(string id, string name, DateTimeOffset timestamp)
        {
            return new TestItem()
            {
                CreatedAt = timestamp,
                UpdatedAt = timestamp,
                Id = id,
                Name = name
            };
        }
    }
}
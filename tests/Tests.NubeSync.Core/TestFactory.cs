using System;

namespace Tests.NubeSync.Core
{
    public static class TestFactory
    {
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
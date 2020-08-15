using System;
using System.Globalization;
using Xunit;

namespace Tests.NubeSync.Core.NubeTable_test
{
    public class Get_properties
    {
        protected readonly TestItem Item;

        public Get_properties()
        {
            Item = new TestItem()
            {
                Id = "MyId",
                Name = "MyName",
                CreatedAt = DateTimeOffset.Now,
                UpdatedAt = DateTimeOffset.Now
            };
        }

        [Fact]
        public void Does_not_return_the_id()
        {
            var result = Item.GetProperties();

            Assert.False(result.ContainsKey("Id"));
        }

        [Fact]
        public void Returns_all_properties()
        {
            var result = Item.GetProperties();

            Assert.Equal(3, result.Count);
            Assert.Equal(Item.Name, result["Name"]);
            Assert.Equal(Convert.ToString(Item.CreatedAt, CultureInfo.InvariantCulture), result["CreatedAt"]);
            Assert.Equal(Convert.ToString(Item.UpdatedAt, CultureInfo.InvariantCulture), result["UpdatedAt"]);
        }

        [Fact]
        public void Returns_null_when_property_is_not_set()
        {
            var item = new TestItem()
            {
                Id = "MyId",
                CreatedAt = DateTimeOffset.Now,
                UpdatedAt = DateTimeOffset.Now
            };

            var result = item.GetProperties();

            Assert.Null(result["Name"]);
        }
    }
}
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
        public void Does_not_return_deleted_at()
        {
            var result = Item.GetProperties();

            Assert.False(result.ContainsKey("DeletedAt"));
        }

        [Fact]
        public void Does_not_return_server_updated_at()
        {
            var result = Item.GetProperties();

            Assert.False(result.ContainsKey("ServerUpdatedAt"));
        }

        [Fact]
        public void Does_not_return_the_clustered_index()
        {
            var result = Item.GetProperties();

            Assert.False(result.ContainsKey("ClusteredIndex"));
        }

        [Fact]
        public void Does_not_return_the_id()
        {
            var result = Item.GetProperties();

            Assert.False(result.ContainsKey("Id"));
        }

        [Fact]
        public void Does_not_return_the_user_id()
        {
            var result = Item.GetProperties();

            Assert.False(result.ContainsKey("UserId"));
        }

        [Fact]
        public void Does_only_return_valid_types()
        {
            var dateTime = DateTime.Now;
            var dateTimeOffset = DateTimeOffset.Now;
            var item = new TestItem3
            {
                Array = new string[] { "1", "2", "3" },
                Bool = true,
                Byte = 0xFF,
                Char = 'c',
                ComplexType = new TestItem2(),
                DateTime = dateTime,
                DateTimeOffset = dateTimeOffset,
                Decimal = 6.66M,
                Double = 3.33,
                Enum = TestEnum.High,
                Float = 2.22f,
                Guid = Guid.NewGuid(),
                Int = 6,
                Long = 7,
                SByte = 8,
                Short = 9,
                SimpleType = "myValue",
                TimeSpan = TimeSpan.FromSeconds(6.66),
                UInt = 10,
                ULong = 11,
                UShort = 12
            };

            var result = item.GetProperties();

            Assert.Equal(21, result.Count);
        }

        [Fact]
        public void Returns_all_properties()
        {
            var result = Item.GetProperties();

            Assert.Equal(3, result.Count);
            Assert.Equal(Item.Name, result["Name"]);
            Assert.Equal(Item.CreatedAt.ToString("o", CultureInfo.InvariantCulture), result["CreatedAt"]);
            Assert.Equal(Item.UpdatedAt.ToString("o", CultureInfo.InvariantCulture), result["UpdatedAt"]);
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
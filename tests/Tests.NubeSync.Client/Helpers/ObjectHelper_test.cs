using System;
using NubeSync.Client.Helpers;
using NubeSync.Core;
using Xunit;

namespace Tests.NubeSync.Client.Helpers.ObjectHelper_test
{
    public class Clone
    {
        [Fact]
        public void Creates_new_clone()
        {
            var myObject = new NubeOperation();

            var newObject = ObjectHelper.Clone(myObject);

            Assert.Equal(myObject.Id, newObject.Id);
            Assert.NotEqual(myObject, newObject);
        }
    }

    public class Copy_properties
    {
        [Fact]
        public void Copies_all_properties()
        {
            var source = _CreateOperation();
            var destination = new NubeOperation();

            source.CopyProperties(destination);

            Assert.Equal(source.ItemId, destination.ItemId);
            Assert.Equal(source.CreatedAt, destination.CreatedAt);
            Assert.Equal(source.Property, destination.Property);
            Assert.Equal(source.OldValue, destination.OldValue);
            Assert.Equal(source.Value, destination.Value);
            Assert.Equal(source.TableName, destination.TableName);
            Assert.Equal(source.Type, destination.Type);
        }

        [Fact]
        public void Does_not_copy_the_id()
        {
            var source = _CreateOperation();
            var destination = new NubeOperation();

            source.CopyProperties(destination);

            Assert.NotEqual(destination.Id, source.Id);
        }

        [Fact]
        public void Throws_when_destination_is_null()
        {
            var myObject = new NubeOperation();

            var exception = Assert.Throws<ArgumentNullException>(() => ObjectHelper.CopyProperties(myObject, null));
            Assert.Equal("Value cannot be null. (Parameter 'Source and/or Destination Objects are null')", exception.Message);
        }

        [Fact]
        public void Throws_when_source_is_null()
        {
            var myObject = new NubeOperation();

            var exception = Assert.Throws<ArgumentNullException>(() => ObjectHelper.CopyProperties(null, myObject));
            Assert.Equal("Value cannot be null. (Parameter 'Source and/or Destination Objects are null')", exception.Message);
        }

        [Fact]
        public void Throws_when_types_are_not_matching()
        {
            var source = new object();
            var destination = new NubeOperation();

            var exception = Assert.Throws<InvalidOperationException>(() => ObjectHelper.CopyProperties(source, destination));
            Assert.Equal("Cannot copy properties to different object types", exception.Message);
        }

        private NubeOperation _CreateOperation()
        {
            return new NubeOperation()
            {
                ItemId = "MyItemId",
                Property = "MyProperty",
                OldValue = "MyOldValue",
                Value = "MyValue",
                TableName = "MyTableName",
                Type = OperationType.Modified
            };
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NubeSync.Core;
using Xunit;

namespace Tests.NubeSync.Client.SQLiteStore.NubeSQLiteDataStore_Operations_test
{
    public class Always : NubeSQLiteDataStoreTestBase
    {
        [Fact]
        public async Task Add_operations_adds()
        {
            await DataStore.InitializeAsync();

            var result = await DataStore.AddOperationsAsync(Operations);

            Assert.True(result);
            var operations = await DataStore.GetOperationsAsync();
            Assert.Equal(Operations.Length, operations.Count());
        }

        [Fact]
        public async Task Add_operations_does_not_throw_when_list_is_empty()
        {
            await DataStore.InitializeAsync();

            var result = await DataStore.AddOperationsAsync(new List<NubeOperation>().ToArray());

            Assert.True(result);
        }

        [Fact]
        public async Task Add_operations_does_not_throw_when_list_is_null()
        {
            await DataStore.InitializeAsync();

            var result = await DataStore.AddOperationsAsync(null);

            Assert.True(result);
        }

        [Fact]
        public async Task Delete_operations_deletes()
        {
            await DataStore.InitializeAsync();
            await DataStore.AddOperationsAsync(Operations);

            await DataStore.DeleteOperationsAsync(Operations);

            var operations = await DataStore.GetOperationsAsync();
            Assert.Empty(operations);
        }

        [Fact]
        public async Task Delete_operations_does_not_throw_when_list_is_empty()
        {
            await DataStore.InitializeAsync();

            var result = await DataStore.DeleteOperationsAsync(new List<NubeOperation>().ToArray());

            Assert.True(result);
        }

        [Fact]
        public async Task Delete_operations_does_not_throw_when_list_is_null()
        {
            await DataStore.InitializeAsync();

            var result = await DataStore.DeleteOperationsAsync(null);

            Assert.True(result);
        }

        [Fact]
        public async Task Get_operations_can_be_paginated()
        {
            var expectedCount = 3;
            await DataStore.InitializeAsync();
            await DataStore.AddOperationsAsync(Operations);

            var operations = await DataStore.GetOperationsAsync(expectedCount);

            Assert.True(expectedCount < Operations.Count());
            Assert.Equal(expectedCount, operations.Count());
        }

        [Fact]
        public async Task Get_operations_returns_all_operations()
        {
            await DataStore.InitializeAsync();
            await DataStore.AddOperationsAsync(Operations);

            var operations = await DataStore.GetOperationsAsync();

            Assert.Equal(Operations.Length, operations.Count());
        }

        [Fact]
        public async Task Get_operations_does_not_split_add_operations_1()
        {
            var operations = new NubeOperation[]
            {
                new NubeOperation() { ItemId = "otherId", Type = OperationType.Modified },
                new NubeOperation() { ItemId = "123", Type = OperationType.Deleted },
                new NubeOperation() { ItemId = "012", Type = OperationType.Added },
                new NubeOperation() { ItemId = "012", Type = OperationType.Modified },
                new NubeOperation() { ItemId = "012", Type = OperationType.Modified },
            };
            await DataStore.InitializeAsync();
            await DataStore.AddOperationsAsync(operations);

            var syncOperations = await DataStore.GetOperationsAsync(3);
            var addOperations = syncOperations.Where(o => o.ItemId == "012");
            Assert.Equal(2, syncOperations.Count());
            Assert.Equal(0, addOperations.Count());

            await DataStore.DeleteOperationsAsync(syncOperations.ToArray());
            syncOperations = await DataStore.GetOperationsAsync(3);
            addOperations = syncOperations.Where(o => o.ItemId == "012");
            Assert.Equal(3, syncOperations.Count());
            Assert.Equal(3, addOperations.Count());
        }

        [Fact]
        public async Task Get_operations_does_not_split_add_operations_2()
        {
            var operations = new NubeOperation[]
            {
                new NubeOperation() { ItemId = "otherId", Type = OperationType.Modified },
                new NubeOperation() { ItemId = "otherId", Type = OperationType.Modified },
                new NubeOperation() { ItemId = "otherId", Type = OperationType.Modified },
                new NubeOperation() { ItemId = "123", Type = OperationType.Modified },
                new NubeOperation() { ItemId = "123", Type = OperationType.Modified },
                new NubeOperation() { ItemId = "012", Type = OperationType.Added },
                new NubeOperation() { ItemId = "012", Type = OperationType.Modified },
                new NubeOperation() { ItemId = "012", Type = OperationType.Modified },
            };
            await DataStore.InitializeAsync();
            await DataStore.AddOperationsAsync(operations);

            var syncOperations = await DataStore.GetOperationsAsync(3);
            var addOperations = syncOperations.Where(o => o.ItemId == "012");
            Assert.Equal(3, syncOperations.Count());
            Assert.Equal(0, addOperations.Count());

            await DataStore.DeleteOperationsAsync(syncOperations.ToArray());
            syncOperations = await DataStore.GetOperationsAsync(3);
            addOperations = syncOperations.Where(o => o.ItemId == "012");
            Assert.Equal(2, syncOperations.Count());
            Assert.Equal(0, addOperations.Count());

            await DataStore.DeleteOperationsAsync(syncOperations.ToArray());
            syncOperations = await DataStore.GetOperationsAsync(3);
            addOperations = syncOperations.Where(o => o.ItemId == "012");
            Assert.Equal(3, syncOperations.Count());
            Assert.Equal(3, addOperations.Count());
        }

        [Fact]
        public async Task Get_operations_does_not_split_add_operations_3()
        {
            var operations = new NubeOperation[]
            {
                new NubeOperation() { ItemId = "otherId", Type = OperationType.Modified },
                new NubeOperation() { ItemId = "otherId", Type = OperationType.Modified },
                new NubeOperation() { ItemId = "012", Type = OperationType.Added },
                new NubeOperation() { ItemId = "012", Type = OperationType.Modified },
                new NubeOperation() { ItemId = "012", Type = OperationType.Modified },
                new NubeOperation() { ItemId = "012", Type = OperationType.Modified },
                new NubeOperation() { ItemId = "012", Type = OperationType.Modified },
            };
            await DataStore.InitializeAsync();
            await DataStore.AddOperationsAsync(operations);

            var syncOperations = await DataStore.GetOperationsAsync(3);
            var addOperations = syncOperations.Where(o => o.ItemId == "012");
            Assert.Equal(2, syncOperations.Count());
            Assert.Equal(0, addOperations.Count());

            await DataStore.DeleteOperationsAsync(syncOperations.ToArray());
            syncOperations = await DataStore.GetOperationsAsync(3);
            addOperations = syncOperations.Where(o => o.ItemId == "012");
            Assert.Equal(5, syncOperations.Count());
            Assert.Equal(5, addOperations.Count());
        }

        [Fact]
        public async Task Get_operations_does_not_split_add_operations_4()
        {
            var operations = new NubeOperation[]
            {
                new NubeOperation() { ItemId = "012", Type = OperationType.Added },
                new NubeOperation() { ItemId = "012", Type = OperationType.Modified },
                new NubeOperation() { ItemId = "012", Type = OperationType.Modified },
                new NubeOperation() { ItemId = "012", Type = OperationType.Modified },
                new NubeOperation() { ItemId = "012", Type = OperationType.Modified },
                new NubeOperation() { ItemId = "otherId", Type = OperationType.Modified },
                new NubeOperation() { ItemId = "otherId", Type = OperationType.Modified },
            };
            await DataStore.InitializeAsync();
            await DataStore.AddOperationsAsync(operations);

            var syncOperations = await DataStore.GetOperationsAsync(3);
            var addOperations = syncOperations.Where(o => o.ItemId == "012");
            Assert.Equal(5, syncOperations.Count());
            Assert.Equal(5, addOperations.Count());

            await DataStore.DeleteOperationsAsync(syncOperations.ToArray());
            syncOperations = await DataStore.GetOperationsAsync(3);
            addOperations = syncOperations.Where(o => o.ItemId == "012");
            Assert.Equal(2, syncOperations.Count());
            Assert.Equal(0, addOperations.Count());
        }
    }
}
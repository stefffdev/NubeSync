using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NubeSync.Core;
using Xunit;

namespace Tests.NubeSync.Client.SQLiteStoreEFCore.NubeSQLiteDataStore_Repository_test
{
    public class Always : NubeSQLiteDataStoreEFCoreTestBase
    {
        private readonly List<TestItem> _items;

        public Always()
        {
            _items = new List<TestItem>
            {
                new TestItem { Id = "1", Name = "Name1" },
                new TestItem { Id = "2", Name = "Name12" },
                new TestItem { Id = "3", Name = "Name3" },
                new TestItem { Id = "4", Name = "Name4" },
                new TestItem { Id = "5", Name = "Name5" },
            };
        }

        [Fact]
        public async Task All_returns_all_elements()
        {
            await _AddItemsAsync();

            var items = await DataStore.AllAsync<TestItem>();

            Assert.Equal(_items.Count, items.Count());
        }

        [Fact]
        public async Task Delete_deletes_the_item()
        {
            await _AddItemsAsync();

            var result = await DataStore.DeleteAsync(_items[0]);

            var items = await DataStore.AllAsync<TestItem>();
            Assert.True(result);
            Assert.Equal(_items.Count - 1, items.Count());
        }

        [Fact]
        public async Task Find_by_finds_the_matching_items()
        {
            await _AddItemsAsync();

            var items = await DataStore.FindByAsync<TestItem>(i => i.Name.StartsWith("Name1"));

            Assert.Equal(2, items.Count());
        }

        [Fact]
        public async Task Find_by_id_returns_the_right_item()
        {
            await _AddItemsAsync();

            var item = await DataStore.FindByIdAsync<TestItem>("1");

            Assert.Equal("Name1", item.Name);
        }

        [Fact]
        public async Task Insert_adds_the_item()
        {
            await _AddItemsAsync();

            var result = await DataStore.InsertAsync(new TestItem { Id = "new" });

            Assert.True(result);
            var items = await DataStore.AllAsync<TestItem>();
            Assert.Equal(_items.Count + 1, items.Count());
        }

        [Fact]
        public async Task Tables_exists_checks_if_table_exists()
        {
            await _AddItemsAsync();

            Assert.False(await DataStore.TableExistsAsync<TestItem2>());
            Assert.True(await DataStore.TableExistsAsync<TestItem>());
        }

        [Fact]
        public async Task Update_updates_the_item()
        {
            await _AddItemsAsync();
            var changedItem = _items[1];
            changedItem.Name = "New";

            var result = await DataStore.UpdateAsync(changedItem);

            Assert.True(result);
            var item = await DataStore.FindByIdAsync<TestItem>(changedItem.Id);
            Assert.Equal("New", item.Name);
        }

        private async Task _AddItemsAsync()
        {
            foreach (var item in _items)
            {
                await DataStore.InsertAsync(item);
            }
        }
    }

    public class TestItem2 : NubeTable
    {
    }
}
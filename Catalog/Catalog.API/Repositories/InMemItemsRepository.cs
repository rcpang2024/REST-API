using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Catalog.API.Entities;

namespace Catalog.API.Repositories {

// don't really need this class anymore since IItemsRepository has been adjusted
    public class InMemItemsRepository : IItemsRepository {
        private readonly List<Item> items = new List<Item>() {
            new Item { Id = Guid.NewGuid(), Name = "Potion", Price = 9, CreatedDate = DateTimeOffset.UtcNow },
            new Item { Id = Guid.NewGuid(), Name = "Steel Sword", Price = 15, CreatedDate = DateTimeOffset.UtcNow },
            new Item { Id = Guid.NewGuid(), Name = "Iron Shield", Price = 12, CreatedDate = DateTimeOffset.UtcNow }
        };

        public async Task<IEnumerable<Item>> GetItemsAsync() {
            return await Task.FromResult(items); // creates a task that has already completed
        }

        public async Task<Item?> getItemAsync(Guid id) {
            var item = items.Where(item => item.Id == id).SingleOrDefault(); // capture the item we found
            return await Task.FromResult(item);
        }

        public async Task CreateItemAsync(Item item) {
            items.Add(item);
            await Task.CompletedTask; // create some task that has been completed and return it w/o anything in it
        }

        public async Task UpdateItemAsync(Item item) {
            var index = items.FindIndex(existingItem => existingItem.Id == item.Id); // finds the index of the item
            items[index] = item; // sets the updated item at its original place in the list
            await Task.CompletedTask;
        }

        public async Task DeleteItemAsync(Guid id) {
            var index = items.FindIndex(existingItem => existingItem.Id == id);
            items.RemoveAt(index);
            await Task.CompletedTask;
        }
    }
} 
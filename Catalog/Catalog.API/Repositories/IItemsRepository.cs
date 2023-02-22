using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.API.Entities;

// task means you're not going to get an item right away anymore, but instead a task that represents an asynch op. that eventually returns the item
namespace Catalog.API.Repositories {
    public interface IItemsRepository { // all methods should return task to signfy they're asynch now
        Task<IEnumerable<Item>> GetItemsAsync(); 

        Task<Item?> getItemAsync(Guid id);

        Task CreateItemAsync(Item item);

        Task UpdateItemAsync(Item item);

        Task DeleteItemAsync(Guid id);
    }
}
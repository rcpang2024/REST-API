using Microsoft.AspNetCore.Mvc;
using Catalog.API.Repositories;
using Catalog.API.Entities;
using Catalog.API.DTOs;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Catalog.API.Controllers {
    
    // GET items
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase { // ItemsController inherits from ControllerBase
        private readonly IItemsRepository? repository; // explicit dependency (not ideal)
        private readonly ILogger<ItemsController> logger; // tells us which pod is which

        public ItemsController(IItemsRepository repository, ILogger<ItemsController> logger) {
            this.repository = repository; // constructor for repository
            this.logger = logger;
        }

        // reacts when someone tries to GET /items
        [HttpGet]
        public async Task<IEnumerable<ItemDTO>> GetItemsAsync(string? name = null) { // sets the route
            var items = (await repository!.GetItemsAsync())
                        .Select(item => item.AsDTO()); // wrap await getItemsAsync to tell it to wait until that's completed before moving on
            if (!string.IsNullOrWhiteSpace(name)) {
                items = items.Where(item => item.Name.Contains(name, StringComparison.OrdinalIgnoreCase)); // as long as name has been provided, it'll do a filter to check for it
            }
            logger.LogInformation($"{DateTime.UtcNow.ToString("hh:mm:ss")}: Retrieved {items.Count()} items");
            return items;
        }

        // GET /items/{id} gets a specific item
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDTO>> GetItemAsync(Guid id) { // has to be ActionResult<> to be able to return more than one type
            var item = await repository!.getItemAsync(id);
            if (item is null) {
                return NotFound();
            }
            return item!.AsDTO();
        }

        // POST /items
        [HttpPost]
        public async Task<ActionResult<ItemDTO>> CreateItemAsync(CreateItemDTO itemDTO) {
            Item item = new Item() {
                Id = Guid.NewGuid(),
                Name = itemDTO.Name,
                Description = itemDTO.Description,
                Price = itemDTO.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await repository!.CreateItemAsync(item); // await here because this is where we create the item
            return CreatedAtAction(nameof(GetItemAsync), new { id = item.Id}, item.AsDTO());
        }

        // PUT /items/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateItemAsync(Guid id, UpdateItemDTO itemDTO) {  // don't return anything because its an output so no type
            var existingItem = await repository!.getItemAsync(id); // finds the item
            if (existingItem is null) {
                return NotFound();
            }
            existingItem.Name = itemDTO.Name;
            existingItem.Price = itemDTO.Price;

            await repository.UpdateItemAsync(existingItem);
            return NoContent();
        }

        // DELETE /items/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteItemAsync(Guid id) {
            var existingItem = await repository!.getItemAsync(id); // finds the item
            if (existingItem is null) {
                return NotFound();
            }
            await repository.DeleteItemAsync(id);
            return NoContent();
        }
    }
}
using Catalog.API.DTOs;
using Catalog.API.Entities;

namespace Catalog.API { // extension method that extends the definition of one type by adding a method that can be extecuted on that type
    public static class Extensions { // for extension methods, you have to use static
        public static ItemDTO AsDTO(this Item item) { // receives an item that returns its version
            return new ItemDTO(item.Id, item.Name, item.Description, item.Price, item.CreatedDate);
            }
        }
    } 
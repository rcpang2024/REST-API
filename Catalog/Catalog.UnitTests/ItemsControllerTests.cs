using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using FluentAssertions;
using Catalog.API.Entities;
using Catalog.API.Controllers;
using Catalog.API.Repositories;
using Catalog.API.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.UnitTests;

public class ItemControllerTests
{
    private readonly Mock<IItemsRepository> repositoryStub = new();
    private readonly Mock<ILogger<ItemsController>> loggerStub = new();
    private readonly Random rand = new();

    [Fact]
    public async Task GetItemAsync_WithUnexistingItem_ReturnsNotFound()
    {
        // Arrange
        repositoryStub.Setup(repo => repo.getItemAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Item?) null);

        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object); // need object property because that's the real object that's going to be passed in
        // Act
        var result = await controller.GetItemAsync(Guid.NewGuid());
        // Assert
        result.Result.Should().BeOfType<NotFoundResult>(); // result = action result, Result = the actual result we got
    }

    [Fact]
    public async Task GetItemAsync_WithExistingItem_ReturnsExpectedItem() {
        // Arrange
        var expectedItem = CreateRandomItem();
        repositoryStub.Setup(repo => repo.getItemAsync(It.IsAny<Guid>()))
            .ReturnsAsync(expectedItem);

        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);
        // Act
        var result = await controller.GetItemAsync(Guid.NewGuid());
        // Assert
        result.Value.Should().BeEquivalentTo(expectedItem);
    }

    [Fact]
    public async Task GetItemsAsync_WithExistingItem_ReturnsAllItems() {
        // Arrange
        var expectedItems = new[]{CreateRandomItem(), CreateRandomItem(), CreateRandomItem()};
        repositoryStub.Setup(repo => repo.GetItemsAsync())
            .ReturnsAsync(expectedItems);
        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);
        // Act
        var actualItems = await controller.GetItemsAsync();
        // Assert
        actualItems.Should().BeEquivalentTo(expectedItems);
    }

    [Fact]
    public async Task GetItemsAsync_WithMatchingItem_ReturnsMatchingItems() {
        // Arrange
        var allItems = new[]{
            new Item(){Name = "Potion"},
            new Item(){Name = "Cure"},
            new Item(){Name = "Upgraded Potion"},
        };

        var nameToMatch = "Potion";
        repositoryStub.Setup(repo => repo.GetItemsAsync())
            .ReturnsAsync(allItems);
        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);
        // Act
        IEnumerable<ItemDTO> foundItems = await controller.GetItemsAsync(nameToMatch);
        // Assert
        foundItems.Should().OnlyContain(
            item => item.Name == allItems[0].Name || item.Name == allItems[2].Name
        );
    }

    [Fact]
    public async Task CreateItemAsync_WithItemToCreate_ReturnsCreatedItem() {
        // Arrange
        var itemToCreate = new CreateItemDTO(
            Guid.NewGuid().ToString(), 
            Guid.NewGuid().ToString(), 
            rand.Next(1000));

        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);
        // Act
        var result = await controller.CreateItemAsync(itemToCreate);
        // Assert
        var createdItem = (result.Result as CreatedAtActionResult).Value as ItemDTO;
        itemToCreate.Should().BeEquivalentTo(
            createdItem,
            options => options.ComparingByMembers<ItemDTO>().ExcludingMissingMembers()
        );
        createdItem!.Id.Should().NotBeEmpty();
        // createdItem.CreatedDate.Should().BeCloseTo(DateTimeOffset.UtcNow, 1000);
    }

    [Fact]
    public async Task UpdateItemAsync_WithExistingItem_ReturnsNoContent() {
        // Arrange
        var existingItem = CreateRandomItem();
        repositoryStub.Setup(repo => repo.getItemAsync(It.IsAny<Guid>()))
            .ReturnsAsync(existingItem);
        
        var itemId = existingItem.Id;
        var itemToUpdate = new UpdateItemDTO(
            Guid.NewGuid().ToString(), 
            Guid.NewGuid().ToString(), 
            existingItem.Price + 5);
        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);
        // Act
        var result = await controller.UpdateItemAsync(itemId, itemToUpdate);
        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteItemAsync_WithExistingItem_ReturnsNoContent() {
        // Arrange
        var existingItem = CreateRandomItem();
        repositoryStub.Setup(repo => repo.getItemAsync(It.IsAny<Guid>()))
            .ReturnsAsync(existingItem);
        

        var controller = new ItemsController(repositoryStub.Object, loggerStub.Object);
        // Act
        var result = await controller.DeleteItemAsync(existingItem.Id);
        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    private Item CreateRandomItem() {
        return new() {
            Id = Guid.NewGuid(),
            Name = Guid.NewGuid().ToString(),
            Price = rand.Next(1000),
            CreatedDate = DateTimeOffset.UtcNow
        };
    }
}
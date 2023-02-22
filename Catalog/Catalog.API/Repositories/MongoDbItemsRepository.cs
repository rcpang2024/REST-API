using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Catalog.API.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Catalog.API.Repositories {
    public class MongoDbItemsRepository : IItemsRepository //dependency injection
    {
        private const string databaseName = "catalog";
        private const string collectionName = "items";
        private readonly IMongoCollection<Item> itemsCollection;
        private readonly FilterDefinitionBuilder<Item> filterBuilder = Builders<Item>.Filter; // allows us to filter items

        public MongoDbItemsRepository(IMongoClient mongoClient) { // stores a collection, will be created the first time we run an API
            IMongoDatabase dataBase = mongoClient.GetDatabase(databaseName);
            itemsCollection = dataBase.GetCollection<Item>(collectionName);
        }
        public async Task CreateItemAsync(Item item) {
            await itemsCollection.InsertOneAsync(item); // async method removes the blocking call which previously meant nothing can happen until the call comes back to the method
        }

        public async Task DeleteItemAsync(Guid id) {
            var filter = filterBuilder.Eq(item => item.Id, id);
            await itemsCollection.DeleteOneAsync(filter);
        }

        public async Task<Item?> getItemAsync(Guid id) {
            var filter = filterBuilder.Eq(item => item.Id, id);
            return await itemsCollection.Find(filter).SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<Item>> GetItemsAsync() {
            return await itemsCollection.Find(new BsonDocument()).ToListAsync();
        }

        public async Task UpdateItemAsync(Item item) {
            var filter = filterBuilder.Eq(existingItem => existingItem.Id, item.Id);
            await itemsCollection.ReplaceOneAsync(filter, item);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FadingFlame.Repositories
{
    public class MongoDbRepositoryBase
    {
        private readonly MongoClient _mongoClient;
        public const string DatabaseName = "FadingFlame";

        public MongoDbRepositoryBase(MongoClient mongoClient)
        {
            _mongoClient = mongoClient;
        }

        protected IMongoDatabase CreateClient()
        {
            var database = _mongoClient.GetDatabase(DatabaseName);
            return database;
        }

        protected Task<T> LoadFirst<T>(Expression<Func<T, bool>> expression)
        {
            var mongoCollection = CreateCollection<T>();
            return mongoCollection.Find(expression).FirstOrDefaultAsync();
        }

        protected Task<T> LoadFirst<T>(ObjectId id) where T : IIdentifiable
        {
            return LoadFirst<T>(x => x.Id == id);
        }

        protected Task Insert<T>(T element)
        {
            var mongoCollection = CreateCollection<T>();
            return mongoCollection.InsertOneAsync(element);
        }

        protected Task Insert<T>(List<T> element)
        {
            var mongoCollection = CreateCollection<T>();
            return mongoCollection.InsertManyAsync(element);
        }

        protected async Task<List<T>> LoadAll<T>(Expression<Func<T, bool>> expression = null, int? limit = null)
        {
            if (expression == null) expression = l => true;
            var mongoCollection = CreateCollection<T>();
            var elements = await mongoCollection.Find(expression).Limit(limit).ToListAsync();
            return elements;
        }

        protected IMongoCollection<T> CreateCollection<T>(string collectionName = null)
        {
            var mongoDatabase = CreateClient();
            var mongoCollection = mongoDatabase.GetCollection<T>((collectionName ?? typeof(T).Name));
            return mongoCollection;
        }

        protected async Task Upsert<T>(T insertObject, Expression<Func<T, bool>> identityQuerry)
        {
            var mongoDatabase = CreateClient();
            var mongoCollection = mongoDatabase.GetCollection<T>(typeof(T).Name);
            await mongoCollection.FindOneAndReplaceAsync(
                identityQuerry,
                insertObject,
                new FindOneAndReplaceOptions<T> {IsUpsert = true});
        }

        protected async Task<bool> UpdateVersionsave<T>(T insertObject)
            where T : class, IVersionable, IIdentifiable
        {
            var mongoDatabase = CreateClient();
            var mongoCollection = mongoDatabase.GetCollection<T>(typeof(T).Name);
            insertObject.Version++;
            var replaced = await mongoCollection.ReplaceOneAsync(
                item => (item.Version == null || item.Version == insertObject.Version - 1) && item.Id == insertObject.Id,
                insertObject,
                new ReplaceOptions {IsUpsert = false});
            return replaced.ModifiedCount == 1;
        }

        protected Task Upsert<T>(T insertObject)  where T : IIdentifiable
        {
            return Upsert(insertObject, x => x.Id == insertObject.Id);
        }

        protected Task UpsertMany<T>(List<T> insertObject) where T : IIdentifiable
        {
            if (!insertObject.Any()) return Task.CompletedTask;

            var collection = CreateCollection<T>();
            var bulkOps = insertObject
                .Select(record => new ReplaceOneModel<T>(Builders<T>.Filter
                .Where(x => x.Id == record.Id), record) {IsUpsert = true})
                .Cast<WriteModel<T>>().ToList();
            return collection.BulkWriteAsync(bulkOps);
        }

        protected async Task Delete<T>(Expression<Func<T, bool>> deleteQuery)
        {
            var mongoDatabase = CreateClient();
            var mongoCollection = mongoDatabase.GetCollection<T>(typeof(T).Name);
            await mongoCollection.DeleteOneAsync<T>(deleteQuery);
        }
        
        protected Task DeleteMultiple<T>(List<ObjectId> deleteIds)  where T : IIdentifiable
        {
            return DeleteMultiple<T>(t => deleteIds.Contains(t.Id));
        }
        
        protected async Task DeleteMultiple<T>(Expression<Func<T, bool>> deleteQuery)
        {
            var mongoDatabase = CreateClient();
            var mongoCollection = mongoDatabase.GetCollection<T>(typeof(T).Name);
            await mongoCollection.DeleteManyAsync<T>(deleteQuery);
        }

        protected Task Delete<T>(ObjectId id) where T : IIdentifiable
        {
            return Delete<T>(x => x.Id == id);
        }
    }

    public interface IVersionable
    {
        public int Version { get; set; }
    }

    public interface IIdentifiable
    {
        public ObjectId Id { get; }
    }
}
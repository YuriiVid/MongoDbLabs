using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDbApp.Models;

namespace MongoDbApp.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _usersCollection;

    public UserRepository(IOptions<MongoDbSettings> mongoDbSettings)
    {
        var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
        _usersCollection = mongoDatabase.GetCollection<User>(mongoDbSettings.Value.CollectionName);
    }

    public async Task<List<User>> GetAllAsync() =>
        await _usersCollection.Find(_ => true).ToListAsync();

    public async Task<User?> GetByIdAsync(string id) =>
        await _usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(User user) => await _usersCollection.InsertOneAsync(user);

    public async Task UpdateAsync(string id, User updatedUser) =>
        await _usersCollection.ReplaceOneAsync(x => x.Id == id, updatedUser);

    public async Task UpdateNameAsync(string id, string newName)
    {
        var filter = Builders<User>.Filter.Eq(x => x.Id, id);
        var update = Builders<User>.Update.Set(x => x.Name, newName);
        await _usersCollection.UpdateOneAsync(filter, update);
    }

    public async Task DeleteAsync(string id) =>
        await _usersCollection.DeleteOneAsync(x => x.Id == id);
}

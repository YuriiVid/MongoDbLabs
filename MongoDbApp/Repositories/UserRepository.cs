using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDbApp.Models;

namespace MongoDbApp.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _usersCollection;
    private readonly IDistributedCache _cache;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(
        IOptions<MongoDbSettings> settings,
        IDistributedCache cache,
        ILogger<UserRepository> logger
    )
    {
        _cache = cache;
        _logger = logger;
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _usersCollection = database.GetCollection<User>(settings.Value.CollectionName);
    }

    private static string GetCacheKey(string id) => $"User_{id}";

    public async Task<List<User>> GetAllAsync() =>
        await _usersCollection.Find(_ => true).ToListAsync();

    public async Task<User?> GetByIdAsync(string id)
    {
        var cacheKey = GetCacheKey(id);

        try
        {
            var cachedUser = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedUser))
            {
                _logger.LogInformation("Retrieved user {Id} from cache", id);
                return JsonSerializer.Deserialize<User>(cachedUser);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get user {Id} from cache", id);
        }

        var user = await _usersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
        if (user is null)
            return null;

        try
        {
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(
                TimeSpan.FromMinutes(5)
            );
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(user), options);
            _logger.LogInformation("Cached user {Id} with cache key {CacheKey}", id, cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache user {Id}", id);
        }

        return user;
    }

    public async Task CreateAsync(User user) => await _usersCollection.InsertOneAsync(user);

    public async Task UpdateAsync(string id, User updatedUser)
    {
        await _usersCollection.ReplaceOneAsync(x => x.Id == id, updatedUser);
        await InvalidateCacheAsync(id);
    }

    public async Task UpdateNameAsync(string id, string newName)
    {
        var filter = Builders<User>.Filter.Eq(x => x.Id, id);
        var update = Builders<User>.Update.Set(x => x.Name, newName);

        await _usersCollection.UpdateOneAsync(filter, update);
        await InvalidateCacheAsync(id);
    }

    public async Task DeleteAsync(string id)
    {
        await _usersCollection.DeleteOneAsync(x => x.Id == id);
        await InvalidateCacheAsync(id);
    }

    private async Task InvalidateCacheAsync(string id)
    {
        var cacheKey = GetCacheKey(id);
        try
        {
            await _cache.RemoveAsync(cacheKey);
            _logger.LogInformation("Invalidated cache for user {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate cache for user {Id}", id);
        }
    }
}

using MongoDbApp.Models;

namespace MongoDbApp.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(string id);
    Task CreateAsync(User user);
    Task UpdateAsync(string id, User user);
    Task UpdateNameAsync(string id, string newName);
    Task DeleteAsync(string id);
}

using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDbApp.Models;
using MongoDbApp.Repositories;
using NSubstitute;
using NUnit.Framework;

namespace MongoDbApp.IntegrationTests;

[TestFixture]
public class UserRepositoryTests
{
    private UserRepository _userRepository = null!;
    private IMongoCollection<User> _usersCollection = null!;
    private IDistributedCache _cache = null!;
    private ILogger<UserRepository> _logger = null!;

    private const string TestDatabaseName = "UserDatabaseTests";
    private const string ConnectionString = "mongodb://localhost:27017";
    private const string CollectionName = "Users";

    [OneTimeSetUp]
    public void Init()
    {
        var settings = Options.Create(
            new MongoDbSettings
            {
                ConnectionString = ConnectionString,
                DatabaseName = TestDatabaseName,
                CollectionName = CollectionName,
            }
        );

        _cache = Substitute.For<IDistributedCache>();
        _logger = Substitute.For<ILogger<UserRepository>>();
        _userRepository = new UserRepository(settings, _cache, _logger);

        var client = new MongoClient(ConnectionString);
        var database = client.GetDatabase(TestDatabaseName);
        _usersCollection = database.GetCollection<User>(CollectionName);
    }

    [SetUp]
    public async Task Setup()
    {
        await _usersCollection.DeleteManyAsync(_ => true);
    }

    [Test]
    public async Task CreateAsync_ShouldInsertUserIntoDatabase()
    {
        var newUser = new User
        {
            Name = "Integration Test User",
            Email = "test@integration.com",
            Role = "Tester",
        };

        await _userRepository.CreateAsync(newUser);

        var result = await _usersCollection
            .Find(u => u.Email == "test@integration.com")
            .FirstOrDefaultAsync();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo(newUser.Name));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnCorrectUser()
    {
        var user = new User
        {
            Name = "Find Me",
            Email = "find@me.com",
            Role = "User",
        };
        await _usersCollection.InsertOneAsync(user);

        var result = await _userRepository.GetByIdAsync(user.Id!);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Find Me"));
    }

    [Test]
    public async Task UpdateAsync_ShouldModifyExistingUser()
    {
        var user = new User
        {
            Name = "Old Name",
            Email = "old@mail.com",
            Role = "User",
        };
        await _usersCollection.InsertOneAsync(user);
        var updatedUser = new User
        {
            Id = user.Id,
            Name = "New Name",
            Email = "new@mail.com",
            Role = "Admin",
        };

        await _userRepository.UpdateAsync(user.Id!, updatedUser);

        var result = await _usersCollection.Find(u => u.Id == user.Id).FirstOrDefaultAsync();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("New Name"));
        Assert.That(result.Role, Is.EqualTo("Admin"));
    }

    [Test]
    public async Task UpdateNameAsync_ShouldOnlyUpdateNameField()
    {
        var user = new User
        {
            Name = "Original Name",
            Email = "keep@mail.com",
            Role = "User",
        };
        await _usersCollection.InsertOneAsync(user);
        var newName = "Updated Name";

        await _userRepository.UpdateNameAsync(user.Id!, newName);

        var result = await _usersCollection.Find(u => u.Id == user.Id).FirstOrDefaultAsync();
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo(newName));
        Assert.That(result.Email, Is.EqualTo("keep@mail.com"));
    }

    [Test]
    public async Task DeleteAsync_ShouldRemoveUserFromDatabase()
    {
        var user = new User
        {
            Name = "Delete Me",
            Email = "del@mail.com",
            Role = "User",
        };
        await _usersCollection.InsertOneAsync(user);

        await _userRepository.DeleteAsync(user.Id!);

        var result = await _usersCollection.Find(u => u.Id == user.Id).FirstOrDefaultAsync();
        Assert.That(result, Is.Null);
    }
}

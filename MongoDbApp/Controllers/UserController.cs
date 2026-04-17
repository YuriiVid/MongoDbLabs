using Microsoft.AspNetCore.Mvc;
using MongoDbApp.Models;
using MongoDbApp.Repositories;
using MongoDbApp.ViewModels;

namespace MongoDbApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _repository;

    public UserController(IUserRepository repository) => _repository = repository;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserListViewModel>>> Get()
    {
        var users = await _repository.GetAllAsync();
        var result = users.Select(u => new UserListViewModel(u.Id!, u.Name, u.Role));
        return Ok(result);
    }

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<UserDetailsViewModel>> Get(string id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null) return NotFound();

        return new UserDetailsViewModel(user.Id!, user.Name, user.Email, user.Role);
    }

    [HttpPost]
    public async Task<IActionResult> Post(UserCreateViewModel model)
    {
        var user = new User
        {
            Name = model.Name,
            Email = model.Email,
            Role = model.Role
        };

        await _repository.CreateAsync(user);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, new UserDetailsViewModel(user.Id!, user.Name, user.Email, user.Role));
    }

    [HttpPatch("{id:length(24)}/name")]
    public async Task<IActionResult> UpdateName(string id, UserUpdateNameViewModel model)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null) return NotFound();

        await _repository.UpdateNameAsync(id, model.Name);
        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null) return NotFound();

        await _repository.DeleteAsync(id);
        return NoContent();
    }
}

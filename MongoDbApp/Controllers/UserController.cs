using Microsoft.AspNetCore.Mvc;
using MongoDbApp.Models;
using MongoDbApp.Repositories;

namespace MongoDbApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserRepository repository) : ControllerBase
{
    private readonly IUserRepository _repository = repository;

    [HttpGet]
    public async Task<List<User>> Get() => await _repository.GetAllAsync();

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<User>> Get(string id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null)
            return NotFound();
        return user;
    }

    [HttpPost]
    public async Task<IActionResult> Post(User newUser)
    {
        await _repository.CreateAsync(newUser);
        return CreatedAtAction(nameof(Get), new { id = newUser.Id }, newUser);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, User updatedUser)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null)
            return NotFound();
        updatedUser.Id = user.Id;
        await _repository.UpdateAsync(id, updatedUser);
        return NoContent();
    }

    [HttpPatch("{id:length(24)}/name")]
    public async Task<IActionResult> UpdateName(string id, [FromBody] string newName)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null)
            return NotFound();
        await _repository.UpdateNameAsync(id, newName);
        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null)
            return NotFound();
        await _repository.DeleteAsync(id);
        return NoContent();
    }
}

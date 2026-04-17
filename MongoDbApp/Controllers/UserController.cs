using AutoMapper;
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
    private readonly IMapper _mapper;

    public UserController(IUserRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserListViewModel>>> Get()
    {
        var users = await _repository.GetAllAsync();
        var result = _mapper.Map<IEnumerable<UserListViewModel>>(users);
        return Ok(result);
    }

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<UserDetailsViewModel>> Get(string id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null)
            return NotFound();

        return _mapper.Map<UserDetailsViewModel>(user);
    }

    [HttpPost]
    public async Task<IActionResult> Post(UserCreateViewModel model)
    {
        var user = _mapper.Map<User>(model);

        await _repository.CreateAsync(user);
        return CreatedAtAction(
            nameof(Get),
            new { id = user.Id },
            _mapper.Map<UserDetailsViewModel>(user)
        );
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Put(string id, UserUpdateViewModel model)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null)
            return NotFound();

        var updatedUser = _mapper.Map<User>(model);
        updatedUser.Id = id;
        updatedUser.InternalSecret = user.InternalSecret;

        await _repository.UpdateAsync(id, updatedUser);
        return NoContent();
    }

    [HttpPatch("{id:length(24)}/name")]
    public async Task<IActionResult> UpdateName(string id, UserUpdateNameViewModel model)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user is null)
            return NotFound();

        await _repository.UpdateNameAsync(id, model.Name);
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

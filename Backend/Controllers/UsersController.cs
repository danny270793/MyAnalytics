using Backend.Data;
using Backend.Extensions;
using Backend.Models;
using Backend.Requests;
using Backend.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<PagedResult<UserResponse>>> GetUsers(
        int page = 1,
        int pageSize = 10
    )
    {
        return Ok(await _context.Users
            .AsNoTracking()
            .OrderBy(user => user.CreatedAt)
            .Select(user => new UserResponse { Id = user.Id, Username = user.Username })
            .ToPagedResultAsync(page, pageSize)
        );
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> GetUser(int id)
    {
        var user = await _context.Users.Select(user => new UserResponse { Id = user.Id, Username = user.Username }).FirstOrDefaultAsync(user => user.Id == id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request)
    {
        var user = new User { Username = request.Username, Password = request.Password };
        EntityEntry<User> addedUser = await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        var userResponse = new UserResponse { Id = addedUser.Entity.Id, Username = addedUser.Entity.Username };
        return CreatedAtAction(nameof(GetUser), new { id = addedUser.Entity.Id }, userResponse);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        User? existingUser = await _context.Users.FindWithFiltersAsync(id);
        if (existingUser == null)
        {
            return NotFound();
        }

        existingUser.Username = request.Username;
        existingUser.Password = request.Password;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteUser(int id)
    {
        User? user = await _context.Users.FindWithFiltersAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

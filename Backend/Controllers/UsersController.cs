using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Responses;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers()
    {
        var users = await _context.Users.Select(user => new UserResponse { Id = user.Id, Username = user.Username }).ToListAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
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
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new UserResponse { Id = user.Id, Username = user.Username });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<User>> UpdateUser(int id, [FromBody] User user)
    {
        if (id != user.Id)
        {
            return BadRequest();
        }

        var existingUser = await _context.Users.FindAsync(id);
        if (existingUser == null)
        {
            return NotFound();
        }

        existingUser.Username = user.Username;
        existingUser.Password = user.Password;
        existingUser.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<User>> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

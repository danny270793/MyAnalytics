using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var users = await _context.Users.ToListAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser([FromBody] User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
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

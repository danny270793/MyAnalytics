using Backend.Data;
using Backend.Requests;
using Backend.Responses;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> LoginAsync([FromBody] LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }
        if (user.Password != request.Password)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        var token = new Token {
            UserId = user.Id,
            User = user,
            AccessToken = Guid.NewGuid().ToString(),
            RefreshToken = Guid.NewGuid().ToString(),
            ExpiresIn = 15 * 60 * 1000, // 15 minutes in milliseconds
            TokenType = "Bearer"
        };
        await _context.Tokens.AddAsync(token);
        await _context.SaveChangesAsync();

        return Ok(new LoginResponse {
            User = new UserResponse { Id = user.Id, Username = user.Username },
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
            ExpiresIn = token.ExpiresIn,
            TokenType = token.TokenType
        });
    }
}

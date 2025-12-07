using Backend.Data;
using Backend.Models;
using Backend.Requests;
using Backend.Responses;
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

        var token = new Token
        {
            UserId = user.Id,
            User = user,
            AccessToken = Guid.NewGuid().ToString(),
            RefreshToken = Guid.NewGuid().ToString(),
            ExpiresIn = 15 * 60 * 1000, // 15 minutes in milliseconds
            TokenType = "Bearer"
        };
        await _context.Tokens.AddAsync(token);
        await _context.SaveChangesAsync();

        return Ok(new LoginResponse
        {
            User = new UserResponse { Id = user.Id, Username = user.Username },
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
            ExpiresIn = token.ExpiresIn,
            TokenType = token.TokenType
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        var authHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authHeader))
        {
            return BadRequest(new { message = "Missing authorization header" });
        }
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Invalid authorization header format" });
        }
        var accessToken = authHeader.Substring("Bearer ".Length).Trim();
        var token = await _context.Tokens.FirstOrDefaultAsync(t => t.AccessToken == accessToken);
        if (token == null)
        {
            return BadRequest(new { message = "Invalid access token" });
        }
        _context.Tokens.Remove(token);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> RefreshAsync()
    {
        var authHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authHeader))
        {
            return BadRequest(new { message = "Missing authorization header" });
        }
        if (!authHeader.StartsWith("Refresh ", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Invalid authorization header format" });
        }
        var refreshToken = authHeader.Substring("Refresh ".Length).Trim();
        var token = await _context.Tokens.FirstOrDefaultAsync(t => t.RefreshToken == refreshToken);
        if (token == null)
        {
            return BadRequest(new { message = "Invalid refresh token" });
        }
        var newToken = new Token
        {
            UserId = token.UserId,
            User = token.User,
            AccessToken = Guid.NewGuid().ToString(),
            RefreshToken = Guid.NewGuid().ToString(),
            ExpiresIn = 15 * 60 * 1000, // 15 minutes in milliseconds
            TokenType = "Bearer"
        };
        await _context.Tokens.AddAsync(newToken);
        _context.Tokens.Remove(token);
        await _context.SaveChangesAsync();
        return Ok(new LoginResponse
        {
            User = new UserResponse { Id = token.User.Id, Username = token.User.Username },
            AccessToken = newToken.AccessToken,
            RefreshToken = newToken.RefreshToken,
            ExpiresIn = newToken.ExpiresIn,
            TokenType = newToken.TokenType
        });
    }
}

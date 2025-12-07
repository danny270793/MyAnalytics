using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using Backend.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Backend.Authentication;

public class TokenAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    AppDbContext context
    ) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "TokenAuthenticationHandler";

    private readonly AppDbContext _context = context;

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return AuthenticateResult.Fail("Missing authorization header");
        }

        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail("Invalid authorization header format");
        }

        var bearerToken = authHeader.Substring("Bearer ".Length).Trim();
        if (string.IsNullOrEmpty(bearerToken))
        {
            return AuthenticateResult.Fail("Missing access token");
        }

        var token = await _context.Tokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.AccessToken == bearerToken);
        if (token == null)
        {
            return AuthenticateResult.Fail("Invalid access token");
        }

        var tokenExpiresAt = token.CreatedAt.AddMilliseconds(token.ExpiresIn);
        if (DateTime.UtcNow > tokenExpiresAt)
        {
            return AuthenticateResult.Fail("Token has expired");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, token.User.Id.ToString()),
            new Claim(ClaimTypes.Name, token.User.Username),
            new Claim("UserId", token.User.Id.ToString()),
            new Claim("TokenId", token.Id.ToString()),
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }
}

using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Backend.Authentication;
using Backend.Models;
using Backend.Tests.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Backend.Tests.Authentication;

public class TokenAuthenticationHandlerTests
{
    [Fact]
    public async Task HandleAuthenticateAsync_WithValidToken_ReturnsSuccess()
    {
        var dbContext = Database.GetInMemoryDbContext();

        var user = new User { Username = "testuser", Password = "testpassword" };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var token = new Token
        {
            UserId = user.Id,
            User = user,
            AccessToken = "valid-access-token",
            RefreshToken = "valid-refresh-token",
            ExpiresIn = 3600000, // 1 hour in milliseconds
            TokenType = "Bearer"
        };
        dbContext.Tokens.Add(token);
        await dbContext.SaveChangesAsync();

        var options = new Mock_OptionsMonitor();
        var loggerFactory = new LoggerFactory();
        var encoder = UrlEncoder.Default;

        var handler = new TokenAuthenticationHandler(options, loggerFactory, encoder, dbContext);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = $"Bearer {token.AccessToken}";

        await handler.InitializeAsync(
            new AuthenticationScheme(TokenAuthenticationHandler.SchemeName, null, typeof(TokenAuthenticationHandler)),
            httpContext
        );

        var result = await handler.AuthenticateAsync();

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Principal);
        Assert.Equal(user.Id.ToString(), result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal(user.Username, result.Principal.FindFirst(ClaimTypes.Name)?.Value);
        Assert.Equal(user.Id.ToString(), result.Principal.FindFirst("UserId")?.Value);
        Assert.Equal(token.Id.ToString(), result.Principal.FindFirst("TokenId")?.Value);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WithNoToken_ReturnsFailure()
    {
        var dbContext = Database.GetInMemoryDbContext();

        var options = new Mock_OptionsMonitor();
        var loggerFactory = new LoggerFactory();
        var encoder = UrlEncoder.Default;

        var handler = new TokenAuthenticationHandler(options, loggerFactory, encoder, dbContext);

        var httpContext = new DefaultHttpContext();

        await handler.InitializeAsync(
            new AuthenticationScheme(TokenAuthenticationHandler.SchemeName, null, typeof(TokenAuthenticationHandler)),
            httpContext
        );

        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Failure);
        Assert.Equal("Missing authorization header", result.Failure?.Message);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WithInvalidTokenType_ReturnsFailure()
    {
        var dbContext = Database.GetInMemoryDbContext();

        var options = new Mock_OptionsMonitor();
        var loggerFactory = new LoggerFactory();
        var encoder = UrlEncoder.Default;

        var handler = new TokenAuthenticationHandler(options, loggerFactory, encoder, dbContext);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = $"Basic invalid-token";

        await handler.InitializeAsync(
            new AuthenticationScheme(TokenAuthenticationHandler.SchemeName, null, typeof(TokenAuthenticationHandler)),
            httpContext
        );

        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Failure);
        Assert.Equal("Invalid authorization header format", result.Failure?.Message);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WithEmptyToken_ReturnsFailure()
    {
        var dbContext = Database.GetInMemoryDbContext();

        var options = new Mock_OptionsMonitor();
        var loggerFactory = new LoggerFactory();
        var encoder = UrlEncoder.Default;

        var handler = new TokenAuthenticationHandler(options, loggerFactory, encoder, dbContext);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = $"Bearer ";

        await handler.InitializeAsync(
            new AuthenticationScheme(TokenAuthenticationHandler.SchemeName, null, typeof(TokenAuthenticationHandler)),
            httpContext
        );

        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Failure);
        Assert.Equal("Missing access token", result.Failure?.Message);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WithInvalidToken_ReturnsFailure()
    {
        var dbContext = Database.GetInMemoryDbContext();

        var options = new Mock_OptionsMonitor();
        var loggerFactory = new LoggerFactory();
        var encoder = UrlEncoder.Default;

        var handler = new TokenAuthenticationHandler(options, loggerFactory, encoder, dbContext);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = $"Bearer invalid-token";

        await handler.InitializeAsync(
            new AuthenticationScheme(TokenAuthenticationHandler.SchemeName, null, typeof(TokenAuthenticationHandler)),
            httpContext
        );

        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Failure);
        Assert.Equal("Invalid access token", result.Failure?.Message);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_ExpiredToken_ReturnsFailure()
    {
        var dbContext = Database.GetInMemoryDbContext();

        var user = new User { Username = "testuser", Password = "testpassword" };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var token = new Token
        {
            UserId = user.Id,
            User = user,
            AccessToken = "valid-access-token",
            RefreshToken = "valid-refresh-token",
            ExpiresIn = 1, // 1 milliseconds
            TokenType = "Bearer"
        };
        dbContext.Tokens.Add(token);
        await dbContext.SaveChangesAsync();

        var options = new Mock_OptionsMonitor();
        var loggerFactory = new LoggerFactory();
        var encoder = UrlEncoder.Default;

        var handler = new TokenAuthenticationHandler(options, loggerFactory, encoder, dbContext);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = $"Bearer {token.AccessToken}";

        await handler.InitializeAsync(
            new AuthenticationScheme(TokenAuthenticationHandler.SchemeName, null, typeof(TokenAuthenticationHandler)),
            httpContext
        );

        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Failure);
        Assert.Equal("Token has expired", result.Failure?.Message);
    }
}

// Helper class to mock IOptionsMonitor<AuthenticationSchemeOptions>
public class Mock_OptionsMonitor : IOptionsMonitor<AuthenticationSchemeOptions>
{
    public AuthenticationSchemeOptions CurrentValue => new AuthenticationSchemeOptions();

    public AuthenticationSchemeOptions Get(string? name) => new AuthenticationSchemeOptions();

    public IDisposable? OnChange(Action<AuthenticationSchemeOptions, string?> listener) => null;
}
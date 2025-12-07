using System.Threading.Tasks;
using Backend.Controllers;
using Backend.Models;
using Backend.Requests;
using Backend.Responses;
using Backend.Tests.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Tests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        var dbContext = Database.GetInMemoryDbContext();
        var user = new User { Username = "testuser", Password = "testpassword" };
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
        var controller = new AuthController(dbContext);

        var request = new LoginRequest { Username = user.Username, Password = user.Password };
        var result = await controller.LoginAsync(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var loginResponse = Assert.IsType<LoginResponse>(okResult.Value);
        Assert.Equal(user.Id, loginResponse.User.Id);
        Assert.Equal(user.Username, loginResponse.User.Username);
        Assert.NotNull(loginResponse.AccessToken);
        Assert.NotNull(loginResponse.RefreshToken);
        Assert.Equal("Bearer", loginResponse.TokenType);
    }
    
    [Fact]
    public async Task Login_WithInvalidUsername_ReturnsUnauthorized()
    {
        var dbContext = Database.GetInMemoryDbContext();
        var user = new User { Username = "testuser", Password = "testpassword" };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        var controller = new AuthController(dbContext);

        var request = new LoginRequest { Username = "wronguser", Password = "testpassword" };
        var result = await controller.LoginAsync(request);
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);

        var messageProperty = unauthorizedResult.Value?.GetType().GetProperty("message");
        Assert.NotNull(messageProperty);
        var message = messageProperty.GetValue(unauthorizedResult.Value)?.ToString();
        Assert.Equal("Invalid username or password", message);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var dbContext = Database.GetInMemoryDbContext();
        var user = new User { Username = "testuser", Password = "testpassword" };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        var controller = new AuthController(dbContext);

        var request = new LoginRequest { Username = "testuser", Password = "wrongpassword" };
        var result = await controller.LoginAsync(request);
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);

        var messageProperty = unauthorizedResult.Value?.GetType().GetProperty("message");
        Assert.NotNull(messageProperty);
        var message = messageProperty.GetValue(unauthorizedResult.Value)?.ToString();
        Assert.Equal("Invalid username or password", message);
    }

    [Fact]
    public async Task Login_CreatesTokenInDatabase()
    {
        var dbContext = Database.GetInMemoryDbContext();
        var user = new User { Username = "testuser", Password = "testpassword" };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        var controller = new AuthController(dbContext);

        var request = new LoginRequest { Username = user.Username, Password = user.Password };
        var result = await controller.LoginAsync(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var loginResponse = Assert.IsType<LoginResponse>(okResult.Value);

        var token = await dbContext.Tokens.FirstOrDefaultAsync(t => t.UserId == user.Id);
        Assert.NotNull(token);
        Assert.Equal(loginResponse.AccessToken, token.AccessToken);
        Assert.Equal(loginResponse.RefreshToken, token.RefreshToken);
    }
}


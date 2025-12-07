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
}


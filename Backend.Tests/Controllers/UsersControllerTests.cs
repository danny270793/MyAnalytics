using System.Threading.Tasks;
using Backend.Controllers;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Tests;

public class UsersControllerTests
{
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetUsers_ReturnsOkResult()
    {
        var dbContext = GetInMemoryDbContext();
        var alreadySavedUser = new User { Id = 1, Username = "test", Password = "test" };
        dbContext.Users.Add(alreadySavedUser);
        await dbContext.SaveChangesAsync();
        var controller = new UsersController(dbContext);

        var result = await controller.GetUsers();
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var users = Assert.IsAssignableFrom<IEnumerable<User>>(okResult.Value);
        Assert.Single(users);
    }

    [Fact]
    public async Task GetUsers_ReturnsEmptyList()
    {
        var dbContext = GetInMemoryDbContext();
        var controller = new UsersController(dbContext);

        var result = await controller.GetUsers();
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var users = Assert.IsAssignableFrom<IEnumerable<User>>(okResult.Value);
        Assert.Empty(users);
    }

    [Fact]
    public async Task GetUser_ReturnsOkResult()
    {
        var dbContext = GetInMemoryDbContext();
        var alreadySavedUser = new User { Id = 1, Username = "test", Password = "test" };
        dbContext.Users.Add(alreadySavedUser);
        await dbContext.SaveChangesAsync();
        var controller = new UsersController(dbContext);

        var result = await controller.GetUser(1);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var user = Assert.IsAssignableFrom<User>(okResult.Value);

        Assert.Equal(alreadySavedUser.Id, user.Id);
        Assert.Equal(alreadySavedUser.Username, user.Username);
        Assert.Equal(alreadySavedUser.Password, user.Password);
    }

    [Fact]
    public async Task GetUser_ReturnsNotFoundResult()
    {
        var dbContext = GetInMemoryDbContext();
        var controller = new UsersController(dbContext);

        var result = await controller.GetUser(1);
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateUser_ReturnsCreatedAtActionResult()
    {
        var dbContext = GetInMemoryDbContext();
        var controller = new UsersController(dbContext);

        var newUser = new User { Username = "test", Password = "test" };
        var result = await controller.CreateUser(newUser);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(controller.GetUser), createdAtActionResult.ActionName);
        Assert.Equal(1, createdAtActionResult.RouteValues["id"]);
        var user = Assert.IsAssignableFrom<User>(createdAtActionResult.Value);
        Assert.Equal(newUser.Id, user.Id);
        Assert.Equal(newUser.Username, user.Username);
        Assert.Equal(newUser.Password, user.Password);
    }
}

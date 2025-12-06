using System.Threading.Tasks;
using Backend.Controllers;
using Backend.Data;
using Backend.Models;
using Backend.Responses;
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
        var users = Assert.IsAssignableFrom<IEnumerable<UserResponse>>(okResult.Value);
        Assert.Single(users);
    }

    [Fact]
    public async Task GetUsers_ReturnsEmptyList()
    {
        var dbContext = GetInMemoryDbContext();
        var controller = new UsersController(dbContext);

        var result = await controller.GetUsers();
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var users = Assert.IsAssignableFrom<IEnumerable<UserResponse>>(okResult.Value);
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
        var user = Assert.IsAssignableFrom<UserResponse>(okResult.Value);

        Assert.Equal(alreadySavedUser.Id, user.Id);
        Assert.Equal(alreadySavedUser.Username, user.Username);
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
        Assert.Equal(newUser.Id, createdAtActionResult.RouteValues?["id"]);
        var user = Assert.IsAssignableFrom<UserResponse>(createdAtActionResult.Value);
        Assert.Equal(newUser.Id, user.Id);
        Assert.Equal(newUser.Username, user.Username);
    }

    [Fact]
    public async Task UpdateUser_ReturnsNoContentResult()
    {
        var dbContext = GetInMemoryDbContext();
        var alreadySavedUser = new User { Id = 1, Username = "test", Password = "test" };
        dbContext.Users.Add(alreadySavedUser);
        await dbContext.SaveChangesAsync();
        var controller = new UsersController(dbContext);

        var updatedUser = new User { Id = alreadySavedUser.Id, Username = "test2", Password = "test2" };
        var result = await controller.UpdateUser(1, updatedUser);
        Assert.IsType<NoContentResult>(result);
        var alreadySavedUserUpdated = await dbContext.Users.FindAsync(alreadySavedUser.Id);
        Assert.Equal(updatedUser.Username, alreadySavedUserUpdated?.Username);
        Assert.Equal(updatedUser.Password, alreadySavedUserUpdated?.Password);
    }

    [Fact]
    public async Task UpdateUser_ReturnsNotFoundResult()
    {
        var dbContext = GetInMemoryDbContext();
        var controller = new UsersController(dbContext);

        var result = await controller.UpdateUser(1, new User { Id = 1, Username = "test", Password = "test" });
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateUser_ReturnsBadRequestResult()
    {
        var dbContext = GetInMemoryDbContext();
        var controller = new UsersController(dbContext);

        var result = await controller.UpdateUser(1, new User { Id = 2, Username = "test", Password = "test" });
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task DeleteUser_ReturnsNoContentResult()
    {
        var dbContext = GetInMemoryDbContext();
        var alreadySavedUser = new User { Id = 1, Username = "test", Password = "test" };
        dbContext.Users.Add(alreadySavedUser);
        await dbContext.SaveChangesAsync();
        var controller = new UsersController(dbContext);

        var result = await controller.DeleteUser(1);
        Assert.IsType<NoContentResult>(result);
        var alreadySavedUserDeleted = await dbContext.Users.FindAsync(alreadySavedUser.Id);
        Assert.Null(alreadySavedUserDeleted);
    }

    [Fact]
    public async Task DeleteUser_ReturnsNotFoundResult()
    {
        var dbContext = GetInMemoryDbContext();
        var controller = new UsersController(dbContext);

        var result = await controller.DeleteUser(1);
        Assert.IsType<NotFoundResult>(result);
    }
}

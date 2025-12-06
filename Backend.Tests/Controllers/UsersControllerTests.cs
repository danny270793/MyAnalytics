using System.Threading.Tasks;
using Backend.Controllers;
using Backend.Data;
using Backend.Models;
using Backend.Responses;
using Backend.Requests;
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
        var paginator = Assert.IsAssignableFrom<PagedResult<UserResponse>>(okResult.Value);
        Assert.Single(paginator.Items);
    }

    [Fact]
    public async Task GetUsers_ReturnsEmptyList()
    {
        var dbContext = GetInMemoryDbContext();
        var controller = new UsersController(dbContext);

        var result = await controller.GetUsers();
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginator = Assert.IsAssignableFrom<PagedResult<UserResponse>>(okResult.Value);
        Assert.Empty(paginator.Items);
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

        var request = new CreateUserRequest { Username = "test", Password = "test" };
        var result = await controller.CreateUser(request);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(controller.GetUser), createdAtActionResult.ActionName);
        Assert.Equal(1, createdAtActionResult.RouteValues?["id"]);
        var user = Assert.IsAssignableFrom<UserResponse>(createdAtActionResult.Value);
        Assert.Equal(1, user.Id);
        Assert.Equal(request.Username, user.Username);
    }

    [Fact]
    public async Task UpdateUser_ReturnsNoContentResult()
    {
        var dbContext = GetInMemoryDbContext();
        var alreadySavedUser = new User { Id = 1, Username = "test", Password = "test" };
        dbContext.Users.Add(alreadySavedUser);
        await dbContext.SaveChangesAsync();
        var controller = new UsersController(dbContext);

        var request = new UpdateUserRequest { Username = "test2", Password = "test2" };
        var result = await controller.UpdateUser(1, request);
        Assert.IsType<NoContentResult>(result);
        var alreadySavedUserUpdated = await dbContext.Users.FindAsync(alreadySavedUser.Id);
        Assert.Equal(request.Username, alreadySavedUserUpdated?.Username);
        Assert.Equal(request.Password, alreadySavedUserUpdated?.Password);
    }

    [Fact]
    public async Task UpdateUser_ReturnsNotFoundResult()
    {
        var dbContext = GetInMemoryDbContext();
        var controller = new UsersController(dbContext);

        var request = new UpdateUserRequest { Username = "test", Password = "test" };
        var result = await controller.UpdateUser(1, request);
        Assert.IsType<NotFoundResult>(result);
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

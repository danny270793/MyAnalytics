using System.Threading.Tasks;
using Backend.Controllers;
using Backend.Data;
using Backend.Extensions;
using Backend.Models;
using Backend.Requests;
using Backend.Responses;
using Backend.Tests.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Tests.Controllers;

public class UsersControllerTests
{
    [Fact]
    public async Task GetUsers_ReturnsOkResult()
    {
        var dbContext = Database.GetInMemoryDbContext();
        var alreadySavedUser = new User { Id = 1, Username = "test", Password = "test" };
        dbContext.Users.Add(alreadySavedUser);
        await dbContext.SaveChangesAsync();
        var controller = new UsersController(dbContext);

        var result = await controller.GetUsersAsync();
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginator = Assert.IsAssignableFrom<PagedResponse<UserResponse>>(okResult.Value);
        Assert.Single(paginator.Items);
    }

    [Fact]
    public async Task GetUsers_ReturnsEmptyList()
    {
        var dbContext = Database.GetInMemoryDbContext();
        var controller = new UsersController(dbContext);

        var result = await controller.GetUsersAsync();
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginator = Assert.IsAssignableFrom<PagedResponse<UserResponse>>(okResult.Value);
        Assert.Empty(paginator.Items);
    }

    [Fact]
    public async Task GetUsers_ReturnsPagedResult_WhenNoUsersAreSaved()
    {
        var dbContext = Database.GetInMemoryDbContext();
        var controller = new UsersController(dbContext);

        var totalUsers = 0;
        var page = 1;
        var pageSize = 10;
        var result = await controller.GetUsersAsync(page: page, pageSize: pageSize);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginator = Assert.IsAssignableFrom<PagedResponse<UserResponse>>(okResult.Value);
        Assert.Equal(page, paginator.Page);
        Assert.Equal(pageSize, paginator.PageSize);
        Assert.Equal(totalUsers, paginator.TotalItems);
        var pages = (int) Math.Ceiling((double) totalUsers / pageSize);
        Assert.Equal(pages, paginator.TotalPages);
        int itemsReturned = Math.Max(0, Math.Min(pageSize, totalUsers - (page - 1) * pageSize));
        Assert.Equal(itemsReturned, paginator.Items.Count());
    }

    [Fact]
    public async Task GetUsers_ReturnsPagedResult_WhenLessThanPageSizeUsersAreSaved()
    {
        var dbContext = Database.GetInMemoryDbContext();
        var totalUsers = 5;
        for (int i = 0; i < totalUsers; i++)
        {
            var user = new User { Id = i + 1, Username = $"test{i}", Password = $"test{i}" };
            dbContext.Users.Add(user);
        }
        await dbContext.SaveChangesAsync();
        var controller = new UsersController(dbContext);

        var page = 1;
        var pageSize = 10;
        var result = await controller.GetUsersAsync(page: page, pageSize: pageSize);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginator = Assert.IsAssignableFrom<PagedResponse<UserResponse>>(okResult.Value);
        Assert.Equal(page, paginator.Page);
        Assert.Equal(pageSize, paginator.PageSize);
        Assert.Equal(totalUsers, paginator.TotalItems);
        var pages = (int) Math.Ceiling((double) totalUsers / pageSize);
        Assert.Equal(pages, paginator.TotalPages);
        int itemsReturned = Math.Max(0, Math.Min(pageSize, totalUsers - (page - 1) * pageSize));
        Assert.Equal(itemsReturned, paginator.Items.Count());
    }

    [Fact]
    public async Task GetUsers_ReturnsPagedResult_Page1WhenMoreThanPageSizeUsersAreSaved()
    {
        var dbContext = Database.GetInMemoryDbContext();
        var totalUsers = 15;
        for (int i = 0; i < totalUsers; i++)
        {
            var user = new User { Id = i + 1, Username = $"test{i}", Password = $"test{i}" };
            dbContext.Users.Add(user);
        }
        await dbContext.SaveChangesAsync();
        var controller = new UsersController(dbContext);

        var page = 1;
        var pageSize = 10;
        var result = await controller.GetUsersAsync(page: page, pageSize: pageSize);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginator = Assert.IsAssignableFrom<PagedResponse<UserResponse>>(okResult.Value);
        Assert.Equal(page, paginator.Page);
        Assert.Equal(pageSize, paginator.PageSize);
        Assert.Equal(totalUsers, paginator.TotalItems);
        var pages = (int) Math.Ceiling((double) totalUsers / pageSize);
        Assert.Equal(pages, paginator.TotalPages);
        int itemsReturned = Math.Max(0, Math.Min(pageSize, totalUsers - (page - 1) * pageSize));
        Assert.Equal(itemsReturned, paginator.Items.Count());
    }

    [Fact]
    public async Task GetUsers_ReturnsPagedResult_Page2WhenMoreThanPageSizeUsersAreSaved()
    {
        var dbContext = Database.GetInMemoryDbContext();
        var totalUsers = 15;
        for (int i = 0; i < totalUsers; i++)
        {
            var user = new User { Id = i + 1, Username = $"test{i}", Password = $"test{i}" };
            dbContext.Users.Add(user);
        }
        await dbContext.SaveChangesAsync();
        var controller = new UsersController(dbContext);

        var page = 2;
        var pageSize = 10;
        var result = await controller.GetUsersAsync(page: page, pageSize: pageSize);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paginator = Assert.IsAssignableFrom<PagedResponse<UserResponse>>(okResult.Value);
        Assert.Equal(page, paginator.Page);
        Assert.Equal(pageSize, paginator.PageSize);
        Assert.Equal(totalUsers, paginator.TotalItems);
        var pages = (int) Math.Ceiling((double) totalUsers / pageSize);
        Assert.Equal(pages, paginator.TotalPages);
        int itemsReturned = Math.Max(0, Math.Min(pageSize, totalUsers - (page - 1) * pageSize));
        Assert.Equal(itemsReturned, paginator.Items.Count());
    }

    [Fact]
    public async Task GetUser_ReturnsOkResult()
    {
        var dbContext = Database.GetInMemoryDbContext();
        var alreadySavedUser = new User { Id = 1, Username = "test", Password = "test" };
        dbContext.Users.Add(alreadySavedUser);
        await dbContext.SaveChangesAsync();
        var controller = new UsersController(dbContext);

        var result = await controller.GetUserAsync(1);
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var user = Assert.IsAssignableFrom<UserResponse>(okResult.Value);

        Assert.Equal(alreadySavedUser.Id, user.Id);
        Assert.Equal(alreadySavedUser.Username, user.Username);
    }

    [Fact]
    public async Task GetUser_ReturnsNotFoundResult()
    {
        var dbContext = Database.GetInMemoryDbContext();
        var controller = new UsersController(dbContext);

        var result = await controller.GetUserAsync(1);
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateUser_ReturnsCreatedAtActionResult()
    {
        var dbContext = Database.GetInMemoryDbContext();
        var controller = new UsersController(dbContext);

        var request = new CreateUserRequest { Username = "test", Password = "test" };
        var result = await controller.CreateUserAsync(request);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(controller.GetUserAsync), createdAtActionResult.ActionName);
        Assert.Equal(1, createdAtActionResult.RouteValues?["id"]);
        var user = Assert.IsAssignableFrom<UserResponse>(createdAtActionResult.Value);
        Assert.Equal(1, user.Id);
        Assert.Equal(request.Username, user.Username);
    }

    [Fact]
    public async Task UpdateUser_ReturnsNoContentResult()
    {
        var dbContext = Database.GetInMemoryDbContext();
        var alreadySavedUser = new User { Id = 1, Username = "test", Password = "test" };
        dbContext.Users.Add(alreadySavedUser);
        await dbContext.SaveChangesAsync();
        var controller = new UsersController(dbContext);

        var request = new UpdateUserRequest { Username = "test2", Password = "test2" };
        var result = await controller.UpdateUserAsync(1, request);
        Assert.IsType<NoContentResult>(result);
        var alreadySavedUserUpdated = await dbContext.Users.FindAsync(alreadySavedUser.Id);
        Assert.Equal(request.Username, alreadySavedUserUpdated?.Username);
        Assert.Equal(request.Password, alreadySavedUserUpdated?.Password);
    }

    [Fact]
    public async Task UpdateUser_ReturnsNotFoundResult()
    {
        var dbContext = Database.GetInMemoryDbContext();
        var controller = new UsersController(dbContext);

        var request = new UpdateUserRequest { Username = "test", Password = "test" };
        var result = await controller.UpdateUserAsync(1, request);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteUser_ReturnsNoContentResult()
    {
        var dbContext = Database.GetInMemoryDbContext();
        var alreadySavedUser = new User { Id = 1, Username = "test", Password = "test" };
        dbContext.Users.Add(alreadySavedUser);
        await dbContext.SaveChangesAsync();
        var controller = new UsersController(dbContext);

        var result = await controller.DeleteUserAsync(1);
        Assert.IsType<NoContentResult>(result);
        var alreadySavedUserDeleted = await dbContext.Users.FindWithFiltersAsync(alreadySavedUser.Id);
        Assert.Null(alreadySavedUserDeleted);
    }

    [Fact]
    public async Task DeleteUser_ReturnsNotFoundResult()
    {
        var dbContext = Database.GetInMemoryDbContext();
        var controller = new UsersController(dbContext);

        var result = await controller.DeleteUserAsync(1);
        Assert.IsType<NotFoundResult>(result);
    }
}

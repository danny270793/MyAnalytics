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
}

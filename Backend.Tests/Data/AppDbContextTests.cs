using Backend.Data;
using Backend.Models;
using Backend.Tests.Utils;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Threading.Tasks;

namespace Backend.Tests.Data;

public class AppDbContextTests
{
    [Fact]
    public async Task CreateUser()
    {
        var dbContext = Database.GetInMemoryDbContext();
        var originalUser = new User { Id = 1, Username = "test", Password = "test" };
        await dbContext.Users.AddAsync(originalUser);
        await dbContext.SaveChangesAsync();

        var addedUser = await dbContext.Users.FindAsync(originalUser.Id);
        Assert.NotNull(addedUser);
        Assert.Null(addedUser.DeletedAt);
        Assert.Equal(addedUser.CreatedAt, addedUser.UpdatedAt);
        Assert.Equal(addedUser.Username, originalUser.Username);
        Assert.Equal(addedUser.Password, originalUser.Password);
    }

    [Fact]
    public async Task UpdateUser()
    {
        var dbContext = Database.GetInMemoryDbContext();
        var originalUser = new User { Id = 1, Username = "test", Password = "test" };
        await dbContext.Users.AddAsync(originalUser);
        await dbContext.SaveChangesAsync();

        var addedUser = await dbContext.Users.FindAsync(originalUser.Id);
        Assert.NotNull(addedUser);
        addedUser.Username = "test2";
        addedUser.Password = "test2";
        await dbContext.SaveChangesAsync();

        var updatedUser = await dbContext.Users.FindAsync(originalUser.Id);
        Assert.NotNull(updatedUser);
        Assert.Equal(originalUser.Username, updatedUser.Username);
        Assert.Equal(originalUser.Password, updatedUser.Password);
        Assert.Null(updatedUser.DeletedAt);
        Assert.NotEqual(updatedUser.CreatedAt, updatedUser.UpdatedAt);
        Assert.True(updatedUser.UpdatedAt > updatedUser.CreatedAt);
    }

    [Fact]
    public async Task DeleteUser()
    {
        var dbContext = Database.GetInMemoryDbContext();
        var originalUser = new User { Id = 1, Username = "test", Password = "test" };
        await dbContext.Users.AddAsync(originalUser);
        await dbContext.SaveChangesAsync();

        var userToDelete = await dbContext.Users.FindAsync(originalUser.Id);
        Assert.NotNull(userToDelete);
        dbContext.Users.Remove(userToDelete);
        await dbContext.SaveChangesAsync();

        var deletedUser = await dbContext.Users.FindAsync(originalUser.Id);
        Assert.NotNull(deletedUser);
        Assert.NotNull(deletedUser.DeletedAt);
        Assert.True(deletedUser.DeletedAt > deletedUser.CreatedAt);
        Assert.True(deletedUser.DeletedAt > deletedUser.UpdatedAt);
        Assert.Equal(deletedUser.CreatedAt, deletedUser.CreatedAt);
        Assert.Equal(deletedUser.UpdatedAt, deletedUser.UpdatedAt);
    }
}
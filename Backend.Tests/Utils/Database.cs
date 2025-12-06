using Backend.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace Backend.Tests.Utils;

public class Database
{
    public static AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}

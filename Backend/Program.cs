using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/users", async (AppDbContext _context) =>
{
    var users = await _context.Users.ToListAsync();
    return Results.Ok(users);
});

app.MapPost("/users", async (AppDbContext _context) =>
{
    var user = new User { Username = "John Doe", Password = "123456" };
    await _context.Users.AddAsync(user);
    await _context.SaveChangesAsync();
    return Results.Ok(user);
});

app.Run();

using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Extensions;

public static class DbSetExtensions
{
    public static async Task<T?> FindWithFiltersAsync<T>(
        this DbSet<T> dbSet,
        object key) where T : BaseEntity
    {
        return await dbSet.FirstOrDefaultAsync(e => e.Id == (int)key);
    }
}

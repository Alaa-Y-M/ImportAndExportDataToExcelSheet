using EFCore.BulkExtensions;
using Task.UI.Common.Interfaces;
using Task.UI.Data;

namespace Task.UI.Services;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    private readonly ApplicationDbContext context;

    public BaseRepository(ApplicationDbContext _context)
    {
        context = _context;
    }
    public async Task<IEnumerable<T>> AddOrUpdateAllAsync(IEnumerable<T> entities)
    {
        await context.BulkInsertOrUpdateAsync<T>(entities.ToList());
        await context.BulkSaveChangesAsync();
        return entities;
    }
    public IEnumerable<T> GetAll()
    {
        return context.Set<T>().ToList();
    }
}
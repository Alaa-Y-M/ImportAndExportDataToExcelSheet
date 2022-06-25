using System.IO.Compression;
using System.Linq.Expressions;
//using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
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
    public async Task<IEnumerable<T>> AddAllAsync(IEnumerable<T> entities)
    {
        //await context.BulkInsertAsync<T>(entities.ToList());
        foreach (var entity in entities)
            await context.Set<T>().AddAsync(entity);
        //await context.SaveChangesAsync();
        return entities;
    }

    public T AddOne(T entity)
    {
        context.Set<T>().Add(entity);
        return entity;
    }

    public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria)
    {
        var query = await context.Set<T>().Where(criteria).ToListAsync();
        return query;
    }

    public async Task<T> FindAsync(Expression<Func<T, bool>> criteria)
    {
        var query = await context.Set<T>().FirstOrDefaultAsync(criteria);
        return query!;
    }

    public IEnumerable<T> GetAll()
    {
        return context.Set<T>().ToList();
    }

    public async Task<T> GetByIdAsync(int Id)
    {
        return (await context.Set<T>().FindAsync(Id))!;
    }
}
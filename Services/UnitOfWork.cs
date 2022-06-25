using Task.UI.Common.Interfaces;
using Task.UI.Data;

namespace Task.UI.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext context;

    public IBaseRepository<CiscoPSSProducts> CiscoPSSProducts { get; private set; }
    public IBaseRepository<CiscoPSSServices> CiscoPSSServices { get; private set; }
    public IBaseRepository<Citrix3PPSS> Citrix3PPSS { get; private set; }
    public UnitOfWork(ApplicationDbContext _context)
    {
        context = _context;
        CiscoPSSProducts = new BaseRepository<CiscoPSSProducts>(context);
        CiscoPSSServices = new BaseRepository<CiscoPSSServices>(context);
        Citrix3PPSS = new BaseRepository<Citrix3PPSS>(context);
    }
    public async Task<int> CompleteAsync()
    {
        return await context.SaveChangesAsync();
    }

    public void Dispose()
    {
        context.Dispose();
    }
}

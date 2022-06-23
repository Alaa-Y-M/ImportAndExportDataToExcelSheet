using Task.UI.Data;

namespace Task.UI.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IBaseRepository<CiscoPSSProducts> CiscoPSSProducts { get; }
    IBaseRepository<CiscoPSSServices> CiscoPSSServices { get; }
    IBaseRepository<Citrix3PPSS> Citrix3PPSS { get; }
    Task<int> Complete();
}
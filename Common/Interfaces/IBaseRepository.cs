using System.Linq.Expressions;
namespace Task.UI.Common.Interfaces;

public interface IBaseRepository<T> where T : class
{
    IEnumerable<T> GetAll();
    Task<IEnumerable<T>> AddOrUpdateAllAsync(IEnumerable<T> entities);
}
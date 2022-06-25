using System.Linq.Expressions;
namespace Task.UI.Common.Interfaces;

public interface IBaseRepository<T> where T : class
{
    Task<T> GetByIdAsync(int Id);
    IEnumerable<T> GetAll();
    Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria);
    Task<IEnumerable<T>> AddAllAsync(IEnumerable<T> entities);
    T AddOne(T entity);
    Task<T> FindAsync(Expression<Func<T, bool>> criteria);
}
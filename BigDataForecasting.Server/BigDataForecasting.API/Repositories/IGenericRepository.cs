using System.Linq.Expressions;

namespace BigDataForecasting.API.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        //Read methods
        IQueryable<T> GetAll(bool tracking = false);
        IQueryable<T> Where(Expression<Func<T, bool>> predicate, bool tracking = false);
        Task<T> GetByIdAsync(int id);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        //Write methods
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}

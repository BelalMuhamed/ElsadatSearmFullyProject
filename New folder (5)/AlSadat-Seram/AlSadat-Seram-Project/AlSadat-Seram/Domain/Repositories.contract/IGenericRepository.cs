using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Repositories.contract
{
    public interface IGenericRepository<T, Tkey> where T : class where Tkey : IEquatable<Tkey>
    {
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task AddRangeAsync (IEnumerable<T> entities);
        Task AddRangeAsyncWithoutSave(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task AddWithoutSaveAsync(T entity);

        Task DeleteAsync(T entity);
        IQueryable<T> GetQueryable();
        Task<T?> FindAsync(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        void UpdateWithoutSaveAsync(T entity);
        void DeleteWithoutSaveAsync(T entity);

        //----------------------------------------------------------------
        Task DeleteRangeAsync(IEnumerable<T> entities);


        void DeleteRangeWithoutSaveAsync(IEnumerable<T> entities);

        //----------------------------------------------------------------
        void Detach(T entity);
    }
}

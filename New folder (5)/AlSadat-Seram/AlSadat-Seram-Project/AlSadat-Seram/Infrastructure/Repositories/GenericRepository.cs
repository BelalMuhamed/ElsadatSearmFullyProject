using Domain.Repositories.contract;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class GenericRepository<T, Tkey> : IGenericRepository<T, Tkey> where T : class where Tkey : IEquatable<Tkey>
    {
        protected readonly AppDbContext _context;
        public GenericRepository(AppDbContext context) =>  _context = context;
        //----------------------------------------------------------------
        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _context.Set<T>().AsNoTracking().ToListAsync();
        }
        //----------------------------------------------------------------
        public IQueryable<T> GetAllAsQueryable()
        {
            return _context.Set<T>().AsNoTracking().AsQueryable();
        }
        public void Detach(T entity)
        {
            _context.Entry(entity).State = EntityState.Detached;
        }
        //----------------------------------------------------------------
        public async Task<T?> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }
        //----------------------------------------------------------------
        public async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().AsNoTracking().Where(predicate).ToListAsync();
        }
        //----------------------------------------------------------------
        public async Task<T> AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task AddWithoutSaveAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }
        //----------------------------------------------------------------
        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _context.Set<T>().AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }
        //----------------------------------------------------------------
        public async Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
           await _context.SaveChangesAsync();
        }
        public void UpdateWithoutSaveAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }
        //----------------------------------------------------------------
        public async Task DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }
        public void DeleteWithoutSaveAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
        }
        //----------------------------------------------------------------
        public IQueryable<T> GetQueryable()
        {
            return _context.Set<T>().AsQueryable();
        }
        //----------------------------------------------------------------
        public async Task<T?> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate);
        }
        //----------------------------------------------------------------
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().AnyAsync(predicate);
        }
        //----------------------------------------------------------------
        //----------------------------------------------------------------
        public async Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        public void DeleteRangeWithoutSaveAsync(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
        }

        public async Task AddRangeAsyncWithoutSave(IEnumerable<T> entities)
        {
            await _context.Set<T>().AddRangeAsync(entities);

        }
        //----------------------------------------------------------------

    }
}

using Domain.Repositories.contract;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.UnitOfWork.Contract
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IGenericRepository<T, Tkey> GetRepository<T, Tkey>()
                where T : class where Tkey : IEquatable<Tkey>;    
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task<IDbContextTransaction> BeginTransactionAsyncM();
        Task RollbackAsync();
        Task CommitAsync();
        Task LogError(Exception ex);
        public bool IsTransactionActive();

    }
}

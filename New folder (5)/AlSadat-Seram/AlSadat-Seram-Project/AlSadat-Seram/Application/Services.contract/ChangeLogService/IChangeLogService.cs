using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract.ChangeLogService
{
    public interface IChangeLogService
    {
        void SetCreateChangeLogInfo<T>(T entity) where T : IAuditableEntity;
        void SetUpdateChangeLogInfo<T>(T entity) where T : IAuditableEntity;
        void SetDeleteChangeLogInfo<T>(T entity) where T : IAuditableEntity;
    }
}

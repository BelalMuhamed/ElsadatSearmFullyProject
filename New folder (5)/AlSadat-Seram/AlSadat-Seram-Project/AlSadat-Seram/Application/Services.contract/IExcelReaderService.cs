using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.DTOs.ExcelReaderDtos;

namespace Application.Services.contract
{
    public interface IExcelReaderService
    {
        ExcelReadResult<T> Read<T>(Stream fileStream) where T : new();
        Task<ExcelImportResult<TEntity>> ImportAsync<TExcel, TEntity>(
        Stream fileStream,
        Func<TExcel, ImportRowContext, Task<RowImportResult<TEntity>>> mapFunc,
        CancellationToken ct)
        where TExcel : new();
    }
}

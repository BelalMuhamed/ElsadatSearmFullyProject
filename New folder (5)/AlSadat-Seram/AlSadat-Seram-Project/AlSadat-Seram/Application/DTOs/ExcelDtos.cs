using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ExcelImportResult<T>
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }

        public List<T> Imported { get; set; } = new();
        public List<ExcelImportRowError> Errors { get; set; } = new();
    }
    public class ExcelImportRowError
    {
        public int RowNumber { get; set; }
        public string? Column { get; set; }
        public string Message { get; set; } = null!;
    }
    public class ImportRowContext
    {
        public int RowNumber { get; set; }
    }
    public class RowImportResult<T>
    {
        public bool IsSuccess { get; set; }
        public T? Entity { get; set; }
        public string? Column { get; set; }
        public string? ErrorMessage { get; set; }

        public static RowImportResult<T> Success(T entity)
            => new() { IsSuccess = true, Entity = entity };

        public static RowImportResult<T> Fail(string msg, string? column = null)
            => new()
            {
                IsSuccess = false,
                ErrorMessage = msg,
                Column = column
            };
    }
}

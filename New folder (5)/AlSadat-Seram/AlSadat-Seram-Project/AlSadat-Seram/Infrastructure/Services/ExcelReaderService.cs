using Application.DTOs;
using Application.Services.contract;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Application.DTOs.ExcelReaderDtos;

namespace Infrastructure.Services
{
    public class ExcelReaderService: IExcelReaderService
    {
    
        public ExcelReadResult<T> Read<T>(Stream fileStream) where T : new()
        {
            var result = new ExcelReadResult<T>();

            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheet(1);

            var headerRow = worksheet.Row(1);

            var headers = headerRow.Cells()
                .Select((c, i) => new
                {
                    Name = c.GetString().Trim(),
                    Index = i + 1
                })
                .ToDictionary(x => x.Name, x => x.Index);

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

            int rowIndex = 2;

            foreach (var row in rows)
            {
                var obj = new T();
                bool hasError = false;

                foreach (var prop in properties)
                {
                    if (!headers.TryGetValue(prop.Name, out int columnIndex))
                        continue;

                    var cell = row.Cell(columnIndex);

                    if (cell.IsEmpty())
                        continue;

                    var stringValue = cell.GetString()?.Trim();

                    if (string.IsNullOrWhiteSpace(stringValue))
                        continue;

                    var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    try
                    {
                        object convertedValue = null;

                        if (targetType == typeof(string))
                        {
                            convertedValue = stringValue;
                        }
                        else if (targetType == typeof(int))
                        {
                            if (!int.TryParse(stringValue, out var intVal))
                                throw new Exception();
                            convertedValue = intVal;
                        }
                        else if (targetType == typeof(decimal))
                        {
                            if (!decimal.TryParse(stringValue, out var decVal))
                                throw new Exception();
                            convertedValue = decVal;
                        }
                        else if (targetType == typeof(double))
                        {
                            if (!double.TryParse(stringValue, out var dblVal))
                                throw new Exception();
                            convertedValue = dblVal;
                        }
                        else if (targetType == typeof(DateTime))
                        {
                            if (!DateTime.TryParse(stringValue, out var dateVal))
                                throw new Exception();
                            convertedValue = dateVal;
                        }
                        else if (targetType == typeof(bool))
                        {
                            if (!bool.TryParse(stringValue, out var boolVal))
                                throw new Exception();
                            convertedValue = boolVal;
                        }
                        else
                        {
                            convertedValue = Convert.ChangeType(stringValue, targetType);
                        }

                        prop.SetValue(obj, convertedValue);
                    }
                    catch
                    {
                        hasError = true;

                        result.errors.Add(new ExcelError
                        {
                            Row = rowIndex,
                            Column = prop.Name,
                            Message = $"Invalid value '{stringValue}' for type {prop.PropertyType.Name}"
                        });
                    }
                }

                if (!hasError)
                    result.data.Add(obj);

                rowIndex++;
            }

            return result;
        }
        public async Task<ExcelImportResult<TEntity>> ImportAsync<TExcel, TEntity>(
    Stream fileStream,
    Func<TExcel, ImportRowContext, Task<RowImportResult<TEntity>>> mapFunc,
    CancellationToken ct)
    where TExcel : new()
        {
            var parsed = Read<TExcel>(fileStream);

            var result = new ExcelImportResult<TEntity>();

            foreach (var err in parsed.errors)
            {
                result.Errors.Add(new ExcelImportRowError
                {
                    RowNumber = err.Row,
                    Column = err.Column,
                    Message = err.Message
                });
            }

            result.TotalRows = parsed.data.Count + parsed.errors.Count;

            int rowNumber = 1;

            foreach (var row in parsed.data)
            {
                rowNumber++;
                ct.ThrowIfCancellationRequested();

                var mapped = await mapFunc(row, new ImportRowContext
                {
                    RowNumber = rowNumber
                });

                if (mapped.IsSuccess && mapped.Entity != null)
                {
                    result.Imported.Add(mapped.Entity);
                    result.SuccessCount++;
                }
                else
                {
                    result.Errors.Add(new ExcelImportRowError
                    {
                        RowNumber = rowNumber,
                        Column = mapped.Column,
                        Message = mapped.ErrorMessage!
                    });

                    result.FailedCount++;
                }
            }

            result.FailedCount = result.Errors.Count;

            return result;
        }
    }
}

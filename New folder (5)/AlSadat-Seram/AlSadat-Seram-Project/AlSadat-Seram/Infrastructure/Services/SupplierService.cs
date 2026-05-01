using AlSadatSeram.Services.contract;
using Application.DTOs.FinanceDtos;
using Application.Services.contract;
using Application.Services.contract.Finance;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Domain.Common;
using Domain.Entities;
using Domain.UnitOfWork.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;
using static Application.DTOs.SupplierDtos;

namespace Infrastructure.Services
{
    /// <summary>
    /// Supplier application service.
    /// Onion rule: depends on Domain + Application only; infra concerns (EF/ClosedXML) are used here by design.
    /// </summary>
    public sealed class SupplierService : ISupplierContract
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExcelReaderService _excelReader;

        // Safe defaults — protects against B-9 (div-by-zero + unbounded lists)
        private const int DefaultPageSize = 20;
        private const int MaxPageSize = 50;

        // Formula-injection guard: any cell starting with one of these gets prefixed with '
        private static readonly char[] ExcelInjectionTriggers = { '=', '+', '-', '@' };
        private ServiceManager serviceManager;

      

        public SupplierService(IUnitOfWork unitOfWork, IExcelReaderService excelReader, ServiceManager serviceManager)
        {
            _unitOfWork = unitOfWork;
            _excelReader = excelReader;
            this.serviceManager = serviceManager;
        }

        // =======================================================================
        // 1) GET ALL (paginated + filterable)
        // =======================================================================
        public async Task<Result<ApiResponse<List<SupplierDto>>>> GetAllSuppliers(SupplierFilteration req)
        {
            try
            {
                var query = _unitOfWork.GetRepository<Supplier, int>()
                    .GetQueryable()
                    .Include(s => s.city)
                    .AsQueryable();

                // ---- Filters ----
                if (req.isDeleted.HasValue)
                    query = query.Where(s => s.IsDeleted == req.isDeleted.Value);

                if (!string.IsNullOrWhiteSpace(req.name))
                {
                    var name = req.name.Trim();
                    query = query.Where(s => s.Name.Contains(name));
                }

                if (!string.IsNullOrWhiteSpace(req.phoneNumbers))
                {
                    var phone = req.phoneNumbers.Trim();
                    query = query.Where(s => s.phoneNumbers.Contains(phone));
                }

                var totalCount = await query.CountAsync();

                // ---- Pagination rule (per your spec):
                //   If BOTH page AND pageSize supplied → paginate.
                //   Otherwise → return everything.
                List<SupplierDto> data;
                int page, pageSize, totalPages;

                var isPaginated = req.page.HasValue && req.pageSize.HasValue;

                if (isPaginated)
                {
                    page = req.page!.Value <= 0 ? 1 : req.page.Value;
                    pageSize = req.pageSize!.Value <= 0 ? DefaultPageSize : req.pageSize.Value;
                    if (pageSize > MaxPageSize) pageSize = MaxPageSize;

                    data = await query
                        .OrderBy(s => s.Id)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Select(s => new SupplierDto
                        {
                            id = s.Id,
                            name = s.Name,
                            phoneNumbers = s.phoneNumbers,
                            address = s.address,
                            cityId = s.cityId,
                            // Null-safe: suppliers without a city show a null cityName.
                            cityName = s.city != null ? s.city.Name : null,
                            isDeleted = s.IsDeleted
                        })
                        .ToListAsync();

                    totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 1;
                }
                else
                {
                    data = await query
                        .OrderBy(s => s.Id)
                        .Select(s => new SupplierDto
                        {
                            id = s.Id,
                            name = s.Name,
                            phoneNumbers = s.phoneNumbers,
                            address = s.address,
                            cityId = s.cityId,
                            // Null-safe: suppliers without a city show a null cityName.
                            cityName = s.city != null ? s.city.Name : null,
                            isDeleted = s.IsDeleted
                        })
                        .ToListAsync();

                    page = 1;
                    pageSize = totalCount;
                    totalPages = 1;
                }

                var envelope = new ApiResponse<List<SupplierDto>>
                {
                    totalCount = totalCount,
                    page = page,
                    pageSize = pageSize,
                    totalPages = totalPages,
                    data = data
                };

                return Result<ApiResponse<List<SupplierDto>>>.Success(envelope);
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<ApiResponse<List<SupplierDto>>>.Failure(
                    "حدث خطأ أثناء تحميل الموردين", HttpStatusCode.InternalServerError);
            }
        }

        // =======================================================================
        // 2) GET BY ID
        // =======================================================================
        public async Task<Result<SupplierDto>> GetById(int id)
        {
            try
            {
                var supplier = await _unitOfWork.GetRepository<Supplier, int>()
                    .GetQueryable()
                    .Include(s => s.city)
                    .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

                if (supplier == null)
                    return Result<SupplierDto>.Failure("المورد غير موجود", HttpStatusCode.NotFound);

                var dto = new SupplierDto
                {
                    id = supplier.Id,
                    name = supplier.Name,
                    phoneNumbers = supplier.phoneNumbers,
                    address = supplier.address,
                    cityId = supplier.cityId,
                    cityName = supplier.city?.Name,
                    isDeleted = supplier.IsDeleted
                };

                return Result<SupplierDto>.Success(dto);
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<SupplierDto>.Failure(
                    "حدث خطأ أثناء تحميل المورد", HttpStatusCode.InternalServerError);
            }
        }

        // =======================================================================
        // 3) LOOKUPS — for select boxes (active only, optional name filter)
        // =======================================================================
        public async Task<Result<List<SupplierLookupDto>>> GetSupplierLookups(SupplierLookupFilter filter)
        {
            try
            {
                var query = _unitOfWork.GetRepository<Supplier, int>()
                    .GetQueryable()
                    .Where(s => !s.IsDeleted);

                if (!string.IsNullOrWhiteSpace(filter.name))
                {
                    var name = filter.name.Trim();
                    query = query.Where(s => s.Name.Contains(name));
                }

                var lookups = await query
                    .OrderBy(s => s.Name)
                    .Select(s => new SupplierLookupDto { id = s.Id, name = s.Name })
                    .ToListAsync();

                return Result<List<SupplierLookupDto>>.Success(lookups);
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<List<SupplierLookupDto>>.Failure(
                    "حدث خطأ أثناء تحميل قائمة الموردين", HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Creates a new supplier and its corresponding leaf account in the chart of
        /// accounts. The two operations are atomic: if either fails, both roll back.
        /// Duplicate detection matches the Excel-import semantics — a supplier is a
        /// duplicate only when BOTH name AND phone match an existing ACTIVE supplier.
        /// </summary>
        public async Task<Result<string>> AddNewSupplier(SupplierDto dto)
        {
            // Defensive validation: ASP.NET model binding already runs DataAnnotations,
            // but we re-validate so the service is safe to call from background jobs etc.
            if (!TryValidate(dto, out var validationMessage))
                return Result<string>.Failure(validationMessage!, HttpStatusCode.BadRequest);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var supplierRepo = _unitOfWork.GetRepository<Supplier, int>();

                var trimmedName = dto.name.Trim();
                var trimmedPhone = dto.phoneNumbers.Trim();

                // Duplicate guard: same Name AND same Phone among ACTIVE suppliers only.
                // A name alone or a phone alone is NOT a duplicate — two real-world
                // suppliers can legitimately share either.
                var duplicateExists = await supplierRepo
                    .GetQueryable()
                    .AnyAsync(s =>
                        !s.IsDeleted &&
                        s.Name == trimmedName &&
                        s.phoneNumbers == trimmedPhone);

                if (duplicateExists)
                {
                    await _unitOfWork.RollbackAsync();
                    return Result<string>.Failure(
                        "يوجد مورد آخر بنفس الاسم ورقم الهاتف", HttpStatusCode.Conflict);
                }

                var supplier = new Supplier
                {
                    Name = trimmedName,
                    phoneNumbers = trimmedPhone,
                    address = string.IsNullOrWhiteSpace(dto.address) ? null : dto.address.Trim(),
                    cityId = dto.cityId,
                    IsDeleted = false
                };

                await supplierRepo.AddWithoutSaveAsync(supplier);
                await _unitOfWork.SaveChangesAsync();

                // Mirror in chart of accounts: leaf account under the suppliers parent.
                // Type is inherited from the parent server-side; code is auto-generated.
                var supplierAccountDto = new CreateAccountDto
                {
                    userId = supplier.Id.ToString(),
                    accountName = supplier.Name,
                    parentAccountId = SuppliersParentAccountId,
                    isLeaf = true,
                    isActive = true
                };

                var accountResult = await serviceManager.treeService.AddNewAccount(supplierAccountDto);
                if (!accountResult.IsSuccess)
                {
                    await _unitOfWork.RollbackAsync();
                    return accountResult;
                }

                await _unitOfWork.CommitAsync();
                return Result<string>.Success("تم إضافة المورد بنجاح");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                await _unitOfWork.LogError(ex);
                return Result<string>.Failure(
                    "حدث خطأ أثناء حفظ المورد", HttpStatusCode.InternalServerError);
            }
        }

        // -----------------------------------------------------------------------------
        // Suppliers' parent account ID in the chart of accounts. Pre-existing magic
        // number that pre-dates this PR — should be migrated to
        // SystemAccountCode.SuppliersParent in a follow-up. Kept as a named constant
        // here so both AddNewSupplier and ImportFromExcelAsync share a single source.
        // -----------------------------------------------------------------------------
        private const int SuppliersParentAccountId = 10;

        // =======================================================================
        // 5) EDIT — cannot edit soft-deleted; does NOT touch IsDeleted
        // =======================================================================
        public async Task<Result<string>> EditSupplier(SupplierDto dto)
        {
            if (dto.id is null or <= 0)
                return Result<string>.Failure("معرّف المورد مطلوب", HttpStatusCode.BadRequest);

            if (!TryValidate(dto, out var validationMessage))
                return Result<string>.Failure(validationMessage!, HttpStatusCode.BadRequest);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var supplierRepo = _unitOfWork.GetRepository<Supplier, int>();

                var supplier = await supplierRepo
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.Id == dto.id.Value);

                if (supplier == null)
                {
                    await _unitOfWork.RollbackAsync();
                    return Result<string>.Failure("المورد غير موجود", HttpStatusCode.NotFound);
                }

                // Block edits on soft-deleted suppliers (B-3)
                if (supplier.IsDeleted)
                {
                    await _unitOfWork.RollbackAsync();
                    return Result<string>.Failure(
                        "لا يمكن تعديل مورد محذوف — قم بإعادة تفعيله أولاً",
                        HttpStatusCode.BadRequest);
                }

                // Duplicate guard — same Name + PhoneNumbers on a DIFFERENT active supplier
                var conflict = await supplierRepo
                    .GetQueryable()
                    .AnyAsync(s =>
                        s.Id != dto.id.Value &&
                        !s.IsDeleted &&
                        s.Name == dto.name.Trim() &&
                        s.phoneNumbers == dto.phoneNumbers.Trim());

                if (conflict)
                {
                    await _unitOfWork.RollbackAsync();
                    return Result<string>.Failure(
                        "يوجد مورد آخر بنفس الاسم ورقم الهاتف", HttpStatusCode.Conflict);
                }

                // Update only editable fields — IsDeleted is intentionally NOT touched here.
                // Values are stored as-is: any Excel-formula-looking character at the start
                // of a value is a concern for EXPORT time, not persistence time.
                supplier.Name = dto.name.Trim();
                supplier.phoneNumbers = dto.phoneNumbers.Trim();
                supplier.address = string.IsNullOrWhiteSpace(dto.address) ? null : dto.address.Trim();
                supplier.cityId = dto.cityId;

                supplierRepo.UpdateWithoutSaveAsync(supplier);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return Result<string>.Success("تم تحديث بيانات المورد بنجاح");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                await _unitOfWork.LogError(ex);
                return Result<string>.Failure(
                    "حدث خطأ أثناء تحديث المورد", HttpStatusCode.InternalServerError);
            }
        }

        // =======================================================================
        // 6) TOGGLE STATUS — dedicated endpoint, flips IsDeleted only
        // =======================================================================
        public async Task<Result<string>> ToggleSupplierStatus(int id)
        {
            if (id <= 0)
                return Result<string>.Failure("معرّف المورد غير صالح", HttpStatusCode.BadRequest);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var supplierRepo = _unitOfWork.GetRepository<Supplier, int>();

                var supplier = await supplierRepo.GetByIdAsync(id);
                if (supplier == null)
                {
                    await _unitOfWork.RollbackAsync();
                    return Result<string>.Failure("المورد غير موجود", HttpStatusCode.NotFound);
                }

                supplier.IsDeleted = !supplier.IsDeleted;
                supplierRepo.UpdateWithoutSaveAsync(supplier);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                var message = supplier.IsDeleted
                    ? "تم إيقاف المورد"
                    : "تم تفعيل المورد";

                return Result<string>.Success(message);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                await _unitOfWork.LogError(ex);
                return Result<string>.Failure(
                    "حدث خطأ أثناء تحديث حالة المورد", HttpStatusCode.InternalServerError);
            }
        }

        // =======================================================================
        // 7) IMPORT FROM EXCEL — per-row atomic (supplier + tree account), partial
        //    success preserved, formula-injection guard at export time only.
        // =======================================================================
        public async Task<Result<SupplierImportResultDto>> ImportFromExcelAsync(
            Stream fileStream, CancellationToken ct)
        {
            var result = new SupplierImportResultDto();

            try
            {
                // ---- 1) Parse workbook via the existing generic reader ---------------
                var parsed = _excelReader.Read<ExcelSupplierDto>(fileStream);

                // Map reader-level cell errors into our row error shape
                foreach (var e in parsed.errors)
                {
                    result.errors.Add(new SupplierImportRowError
                    {
                        rowNumber = e.Row,
                        column = e.Column,
                        message = e.Message
                    });
                }

                result.totalRows = parsed.data.Count + parsed.errors.Count;

                if (parsed.data.Count == 0)
                {
                    result.failedCount = result.errors.Count;
                    return Result<SupplierImportResultDto>.Success(
                        result, "لا توجد بيانات صالحة للاستيراد");
                }

                ct.ThrowIfCancellationRequested();

                // ---- 2) Pre-load DB state ONCE (no N+1) -------------------------------
                var supplierRepo = _unitOfWork.GetRepository<Supplier, int>();

                var existing = await supplierRepo.GetQueryable()
                    .Where(s => !s.IsDeleted)
                    .Select(s => new { s.Name, s.phoneNumbers })
                    .ToListAsync(ct);

                var existingKeys = existing
                    .Select(x => ComposeKey(x.Name, x.phoneNumbers))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                // ---- 3) Validate each parsed row, build a list of validated candidates
                // We keep the Excel row number on each candidate so per-row errors
                // during persistence (step 4) can still point at the correct row.
                var validatedRows = new List<ValidatedSupplierRow>();
                var seenInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // Excel row counter starts at 2 (row 1 = header).
                int excelRow = 1;
                foreach (var row in parsed.data)
                {
                    excelRow++;
                    ct.ThrowIfCancellationRequested();

                    var rawName = row.Name?.Trim() ?? string.Empty;
                    var rawPhone = row.PhoneNumbers?.Trim() ?? string.Empty;

                    // Phone normalization (see method docs).
                    var normalizedPhone = NormalizeImportedPhone(rawPhone);

                    if (!IsValidE164(normalizedPhone))
                    {
                        result.errors.Add(new SupplierImportRowError
                        {
                            rowNumber = excelRow,
                            supplierName = rawName,
                            column = nameof(row.PhoneNumbers),
                            message = "رقم الهاتف غير صالح — يجب أن يحتوي على 7 إلى 15 رقمًا (يمكنك كتابته بصيغة +201012345678 أو 201012345678)"
                        });
                        continue;
                    }

                    var sanitizedDto = new ExcelSupplierDto
                    {
                        Name = rawName,
                        PhoneNumbers = normalizedPhone
                    };

                    if (!TryValidate(sanitizedDto, out var validationMsg, out var failedProperty))
                    {
                        result.errors.Add(new SupplierImportRowError
                        {
                            rowNumber = excelRow,
                            supplierName = rawName,
                            column = failedProperty ?? "row",
                            message = validationMsg!
                        });
                        continue;
                    }

                    var key = ComposeKey(rawName, normalizedPhone);

                    // Duplicate within the same file
                    if (!seenInFile.Add(key))
                    {
                        result.errors.Add(new SupplierImportRowError
                        {
                            rowNumber = excelRow,
                            supplierName = rawName,
                            column = nameof(row.Name),
                            message = "هذا المورد مكرر داخل الملف نفسه"
                        });
                        continue;
                    }

                    // Duplicate against the DB (active suppliers)
                    if (existingKeys.Contains(key))
                    {
                        result.errors.Add(new SupplierImportRowError
                        {
                            rowNumber = excelRow,
                            supplierName = rawName,
                            column = nameof(row.Name),
                            message = "يوجد مورد مسجل مسبقًا بنفس الاسم ورقم الهاتف"
                        });
                        continue;
                    }

                    validatedRows.Add(new ValidatedSupplierRow(
                        ExcelRowNumber: excelRow,
                        Name: rawName,
                        Phone: normalizedPhone));
                }

                // ---- 4) Persist each valid row in its own transaction -----------------
                // Per-row atomicity: each loop iteration commits or rolls back ITS OWN
                // supplier + tree account together. A bad row never poisons a good one.
                foreach (var candidate in validatedRows)
                {
                    ct.ThrowIfCancellationRequested();

                    var rowResult = await TryInsertSupplierWithAccountAsync(candidate, ct);

                    if (rowResult.IsSuccess)
                    {
                        result.successCount++;
                        result.imported.Add(rowResult.Value);
                    }
                    else
                    {
                        result.errors.Add(new SupplierImportRowError
                        {
                            rowNumber = candidate.ExcelRowNumber,
                            supplierName = candidate.Name,
                            column = rowResult.ErrorColumn ?? "row",
                            message = rowResult.ErrorMessage ?? "فشل الحفظ"
                        });
                    }
                }

                result.failedCount = result.errors.Count;

                var message = (result.successCount, result.failedCount) switch
                {
                    ( > 0, 0) => "تم استيراد جميع الموردين بنجاح",
                    ( > 0, > 0) => $"تم استيراد {result.successCount} مورد بنجاح، وفشل {result.failedCount}",
                    (0, > 0) => "فشل استيراد جميع الصفوف — راجع قائمة الأخطاء",
                    _ => "لا توجد بيانات للاستيراد"
                };

                return Result<SupplierImportResultDto>.Success(result, message);
            }
            catch (OperationCanceledException)
            {
                return Result<SupplierImportResultDto>.Failure(
                    "تم إلغاء عملية الاستيراد", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<SupplierImportResultDto>.Failure(
                    "حدث خطأ أثناء استيراد الملف — تأكد من صحة التنسيق",
                    HttpStatusCode.InternalServerError);
            }
        }

        // ---------------------------------------------------------------------------
        // Inserts ONE supplier together with its leaf account in the chart of
        // accounts. Both succeed or both roll back — there are no dangling suppliers.
        //
        // Returns:
        //   IsSuccess = true  → Value contains the persisted SupplierDto
        //   IsSuccess = false → ErrorColumn / ErrorMessage describe the failure
        // ---------------------------------------------------------------------------
        private async Task<RowInsertOutcome> TryInsertSupplierWithAccountAsync(
            ValidatedSupplierRow row, CancellationToken ct)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var supplierRepo = _unitOfWork.GetRepository<Supplier, int>();

                var supplier = new Supplier
                {
                    Name = row.Name,
                    phoneNumbers = row.Phone,
                    address = null,
                    cityId = null,
                    IsDeleted = false
                };

                await supplierRepo.AddWithoutSaveAsync(supplier);
                await _unitOfWork.SaveChangesAsync();   // populates supplier.Id for the account link

                // Same shape as AddNewSupplier — single source of truth for this mapping.
                var accountDto = new CreateAccountDto
                {
                    userId = supplier.Id.ToString(),
                    accountName = supplier.Name,
                    parentAccountId = SuppliersParentAccountId,
                    isLeaf = true,
                    isActive = true
                };

                var accountResult = await serviceManager.treeService.AddNewAccount(accountDto);
                if (!accountResult.IsSuccess)
                {
                    await _unitOfWork.RollbackAsync();
                    return RowInsertOutcome.Failure(
                        column: "TreeAccount",
                        message: $"تعذّر إنشاء الحساب المحاسبي للمورد: {accountResult.Message}");
                }

                await _unitOfWork.CommitAsync();

                return RowInsertOutcome.Success(new SupplierDto
                {
                    id = supplier.Id,
                    name = supplier.Name,
                    phoneNumbers = supplier.phoneNumbers,
                    address = supplier.address,
                    cityId = supplier.cityId,
                    cityName = null,
                    isDeleted = supplier.IsDeleted
                });
            }
            catch (Exception ex)
            {
                // Best-effort rollback — never let a row failure break the loop.
                try { await _unitOfWork.RollbackAsync(); } catch { /* swallow */ }
                await _unitOfWork.LogError(ex);

                return RowInsertOutcome.Failure(
                    column: "DB",
                    message: "فشل الحفظ في قاعدة البيانات");
            }
        }

        // ---------------------------------------------------------------------------
        // Internal value objects used only by ImportFromExcelAsync. Kept as nested
        // types because they have no meaning outside this file.
        // ---------------------------------------------------------------------------
        private sealed record ValidatedSupplierRow(
            int ExcelRowNumber,
            string Name,
            string Phone);

        private sealed class RowInsertOutcome
        {
            public bool IsSuccess { get; private init; }
            public SupplierDto? Value { get; private init; }
            public string? ErrorColumn { get; private init; }
            public string? ErrorMessage { get; private init; }

            public static RowInsertOutcome Success(SupplierDto value)
                => new() { IsSuccess = true, Value = value };

            public static RowInsertOutcome Failure(string column, string message)
                => new() { IsSuccess = false, ErrorColumn = column, ErrorMessage = message };
        }

        // =======================================================================
        // 8) GENERATE TEMPLATE — two-sheet .xlsx via ClosedXML (MIT, already installed)
        // =======================================================================
        public Task<Result<byte[]>> GenerateImportTemplateAsync(CancellationToken ct)
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                using var workbook = new XLWorkbook();

                // ---- Sheet 1: "Suppliers" (the one the importer reads) ----
                var ws = workbook.Worksheets.Add("Suppliers");

                ws.Cell(1, 1).Value = "Name";
                ws.Cell(1, 2).Value = "PhoneNumbers";

                // Header styling: required fields = gold fill + bold
                var header = ws.Range(1, 1, 1, 2);
                header.Style.Font.Bold = true;
                header.Style.Font.FontColor = XLColor.Black;
                header.Style.Fill.BackgroundColor = XLColor.FromHtml("#D4AF37"); // gold (matches UI)
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                // ------------------------------------------------------------------
                // CRITICAL: Force the PhoneNumbers column to TEXT format.
                //
                // Excel will silently convert "+201012345678" to the integer
                // 201012345678 unless the cell is explicitly text-typed.
                // Three layers of defence:
                //
                //   1. Column-level number format '@' = text.
                //   2. A pre-typed range covering rows 2..1000 so any row a user
                //      pastes into within that range inherits text formatting.
                //   3. Explicit per-cell DataType = Text on the example row.
                //
                // Even with all three, some Excel versions still override the
                // format when the user edits the cell. The import service
                // normalizes on read as the final safety net.
                // ------------------------------------------------------------------
                ws.Column(2).Style.NumberFormat.Format = "@";
                ws.Range(2, 2, 1000, 2).Style.NumberFormat.Format = "@";
                ws.Range(2, 2, 1000, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                // Example row (row 2) — user should delete before importing
                ws.Cell(2, 1).Value = "شركة النور للمستلزمات";

                var phoneExampleCell = ws.Cell(2, 2);
                phoneExampleCell.Style.NumberFormat.Format = "@";
                phoneExampleCell.SetValue("201012345678");

                // A comment on the header cell reminding the user about the '+'
                ws.Cell(1, 2).CreateComment()
                    .AddText("اكتب رقم الهاتف بصيغة +201012345678. إذا حذف Excel علامة + بعد الكتابة، لا مشكلة — الاستيراد يتعرف على الصيغتين.");

                ws.Range(2, 1, 2, 2).Style.Font.Italic = true;
                ws.Range(2, 1, 2, 2).Style.Font.FontColor = XLColor.Gray;

                ws.Columns().AdjustToContents();
                ws.Column(1).Width = Math.Max(ws.Column(1).Width, 30);
                ws.Column(2).Width = Math.Max(ws.Column(2).Width, 25);

                ws.SheetView.FreezeRows(1);

                // ---- Sheet 2: "Instructions" ----
                var help = workbook.Worksheets.Add("Instructions");
                help.Cell(1, 1).Value = "تعليمات استيراد الموردين";
                help.Cell(1, 1).Style.Font.Bold = true;
                help.Cell(1, 1).Style.Font.FontSize = 14;

                help.Cell(3, 1).Value = "الأعمدة المطلوبة:";
                help.Cell(3, 1).Style.Font.Bold = true;
                help.Cell(4, 1).Value = "• Name — اسم المورد (مطلوب، بحد أقصى 200 حرف)";
                help.Cell(5, 1).Value = "• PhoneNumbers — رقم الهاتف (مطلوب، 7 إلى 15 رقمًا)";

                help.Cell(7, 1).Value = "ملاحظات:";
                help.Cell(7, 1).Style.Font.Bold = true;
                help.Cell(8, 1).Value = "1. احذف الصف التجريبي (الثاني) في ورقة Suppliers قبل الاستيراد.";
                help.Cell(9, 1).Value = "2. يمكن كتابة رقم الهاتف بصيغة +201012345678 أو 201012345678 — النظام يقبل الصيغتين.";
                help.Cell(10, 1).Value = "3. إذا حذف Excel علامة + تلقائيًا من بداية الرقم، لا تقلق — الاستيراد سيتعرف على الرقم ويضيف الإشارة تلقائيًا.";
                help.Cell(11, 1).Value = "4. الصفوف الفارغة يتم تجاهلها تلقائيًا.";
                help.Cell(12, 1).Value = "5. الصفوف المكررة داخل الملف أو الموجودة مسبقًا يتم رفضها مع رسالة خطأ.";
                help.Cell(13, 1).Value = "6. بعد الاستيراد، ادخل لتعديل كل مورد وإضافة المدينة والعنوان.";

                help.Column(1).Width = 80;

                using var ms = new MemoryStream();
                workbook.SaveAs(ms);
                return Task.FromResult(Result<byte[]>.Success(ms.ToArray()));
            }
            catch (Exception ex)
            {
                _ = _unitOfWork.LogError(ex);
                return Task.FromResult(Result<byte[]>.Failure(
                    "تعذر إنشاء القالب", HttpStatusCode.InternalServerError));
            }
        }

        // =======================================================================
        // Helpers
        // =======================================================================

        /// <summary>
        /// Validates a DTO using its DataAnnotations. On failure returns the first error message
        /// and (optionally) the property that failed.
        /// </summary>
        private static bool TryValidate(object instance, out string? message)
            => TryValidate(instance, out message, out _);

        private static bool TryValidate(object instance, out string? message, out string? failedProperty)
        {
            var context = new ValidationContext(instance);
            var errors = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(instance, context, errors, validateAllProperties: true);

            if (isValid)
            {
                message = null;
                failedProperty = null;
                return true;
            }

            var first = errors[0];
            message = first.ErrorMessage ?? "البيانات غير صالحة";
            failedProperty = first.MemberNames.FirstOrDefault();
            return false;
        }

        /// <summary>
        /// Escapes a cell value against Excel / CSV formula injection.
        ///
        /// IMPORTANT: call this ONLY when writing a value out to a .xlsx / .csv
        /// file, never when persisting to the database. The prepended apostrophe
        /// would otherwise become part of the stored data (e.g. a phone number
        /// legitimately starting with '+' would be saved as "'+201...").
        ///
        /// Prefix rule: if a trimmed value starts with '=', '+', '-' or '@',
        /// Excel evaluates it as a formula on paste/open. Prefixing with an
        /// apostrophe forces Excel to treat it as literal text.
        ///
        /// Currently unused in the save paths — kept as a helper for whenever
        /// we add "export suppliers to Excel" functionality.
        /// </summary>
        private static string SanitizeForExcelExport(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            var trimmed = value.TrimStart();
            if (trimmed.Length == 0) return value;
            return Array.IndexOf(ExcelInjectionTriggers, trimmed[0]) >= 0
                ? "'" + value
                : value;
        }

        /// <summary>
        /// Composes a comparison key for (Name, PhoneNumbers) duplicate detection.
        /// </summary>
        private static string ComposeKey(string name, string phone)
            => $"{(name ?? string.Empty).Trim()}|{(phone ?? string.Empty).Trim()}";

        /// <summary>
        /// Normalizes a phone number coming from Excel into E.164 format.
        ///
        /// Excel is notoriously aggressive about phone-number cells:
        ///   - Typing "+201012345678" in a General-formatted cell is stored as
        ///     the integer 201012345678 (leading '+' dropped silently).
        ///   - Users paste values with dashes, spaces, parentheses, or the
        ///     Arabic-script (٠١٢٣...) digits.
        ///
        /// This method is tolerant of all of the above:
        ///   1. Strip everything except ASCII digits and a leading '+'.
        ///   2. If the result starts with '+', keep it; otherwise prepend '+'.
        ///   3. Collapse any '+' that isn't at the very start (paranoia).
        ///
        /// Returns a string like "+201012345678". Validation of the final shape
        /// is done separately by <see cref="IsValidE164"/>.
        /// </summary>
        private static string NormalizeImportedPhone(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

            // Convert Arabic-Indic digits (٠-٩) to ASCII digits (0-9).
            var span = raw.Trim().AsSpan();
            var buffer = new System.Text.StringBuilder(span.Length + 1);
            bool sawPlus = false;

            foreach (var ch in span)
            {
                // Keep a leading '+' once; ignore any later ones.
                if (ch == '+')
                {
                    if (!sawPlus && buffer.Length == 0)
                    {
                        buffer.Append('+');
                        sawPlus = true;
                    }
                    continue;
                }

                // Arabic-Indic numerals (U+0660–U+0669) → 0..9
                if (ch >= '\u0660' && ch <= '\u0669')
                {
                    buffer.Append((char)('0' + (ch - '\u0660')));
                    continue;
                }

                // Eastern Arabic-Indic (Persian) numerals (U+06F0–U+06F9) → 0..9
                if (ch >= '\u06F0' && ch <= '\u06F9')
                {
                    buffer.Append((char)('0' + (ch - '\u06F0')));
                    continue;
                }

                // Standard ASCII digits
                if (ch >= '0' && ch <= '9')
                {
                    buffer.Append(ch);
                    continue;
                }

                // Everything else (spaces, dashes, dots, parens, letters) is discarded.
            }

            if (buffer.Length == 0) return string.Empty;

            // Ensure the result starts with '+'.
            if (buffer[0] != '+') buffer.Insert(0, '+');

            return buffer.ToString();
        }

        /// <summary>
        /// Returns true if the input matches the E.164 format:
        /// a leading '+', a non-zero leading digit, and 7–15 total digits.
        /// </summary>
        private static bool IsValidE164(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            if (value[0] != '+') return false;
            if (value.Length < 8 || value.Length > 16) return false;       // '+' + 7..15 digits
            if (value[1] < '1' || value[1] > '9') return false;             // first digit 1..9
            for (int i = 1; i < value.Length; i++)
            {
                if (value[i] < '0' || value[i] > '9') return false;
            }
            return true;
        }
    }
}
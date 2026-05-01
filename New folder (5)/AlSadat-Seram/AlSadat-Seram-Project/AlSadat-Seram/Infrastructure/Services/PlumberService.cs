using AlSadatSeram.Services.contract;
using Application.Services.contract;
using ClosedXML.Excel;
using Domain.Common;
using Domain.Entities;
using Domain.UnitOfWork.Contract;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;
using static Application.DTOs.PlumberDtos;

namespace Infrastructure.Services
{
    /// <summary>
    /// Application service for plumber management.
    /// <para>
    /// This service intentionally does NOT integrate with the chart of accounts —
    /// plumbers are pure master data, in contrast to <see cref="SupplierService"/>.
    /// That removes the need for cross-aggregate atomicity in Add/Edit, which
    /// simplifies the code considerably.
    /// </para>
    /// </summary>
    public sealed class PlumberService : IPlumberContract
    {
        private readonly IUnitOfWork _unitOfWork;

        public PlumberService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =======================================================================
        // 1) GET ALL — paginated + filterable
        // =======================================================================
        public async Task<Result<AlSadatSeram.Services.contract.ApiResponse<List<PlumberDto>>>> GetAllPlumbers(PlumberFilteration req)
        {
            try
            {
                var query = _unitOfWork.GetRepository<Plumber, int>()
                    .GetQueryable()
                    .Include(p => p.city)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(req.name))
                {
                    var n = req.name.Trim();
                    query = query.Where(p => p.Name.Contains(n));
                }

                if (!string.IsNullOrWhiteSpace(req.phoneNumbers))
                {
                    var ph = req.phoneNumbers.Trim();
                    query = query.Where(p => p.phoneNumbers.Contains(ph));
                }

                if (!string.IsNullOrWhiteSpace(req.licenseNumber))
                {
                    var ln = req.licenseNumber.Trim();
                    query = query.Where(p => p.LicenseNumber != null && p.LicenseNumber.Contains(ln));
                }

                if (!string.IsNullOrWhiteSpace(req.specialty))
                {
                    var sp = req.specialty.Trim();
                    query = query.Where(p => p.Specialty != null && p.Specialty.Contains(sp));
                }

                if (req.isDeleted.HasValue)
                    query = query.Where(p => p.IsDeleted == req.isDeleted.Value);

                var totalCount = await query.CountAsync();

                int page;
                int pageSize;
                int totalPages;
                List<PlumberDto> data;

                if (req.page.HasValue && req.pageSize.HasValue && req.pageSize.Value > 0)
                {
                    page = req.page.Value < 1 ? 1 : req.page.Value;
                    pageSize = req.pageSize.Value;
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                    data = await query
                        .OrderBy(p => p.Name)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Select(p => new PlumberDto
                        {
                            id = p.Id,
                            name = p.Name,
                            phoneNumbers = p.phoneNumbers,
                            address = p.address,
                            cityId = p.cityId,
                            cityName = p.city != null ? p.city.Name : null,
                            licenseNumber = p.LicenseNumber,
                            specialty = p.Specialty,
                            isDeleted = p.IsDeleted
                        })
                        .ToListAsync();
                }
                else
                {
                    data = await query
                        .OrderBy(p => p.Name)
                        .Select(p => new PlumberDto
                        {
                            id = p.Id,
                            name = p.Name,
                            phoneNumbers = p.phoneNumbers,
                            address = p.address,
                            cityId = p.cityId,
                            cityName = p.city != null ? p.city.Name : null,
                            licenseNumber = p.LicenseNumber,
                            specialty = p.Specialty,
                            isDeleted = p.IsDeleted
                        })
                        .ToListAsync();

                    page = 1;
                    pageSize = totalCount;
                    totalPages = 1;
                }

                var envelope = new AlSadatSeram.Services.contract.ApiResponse<List<PlumberDto>>
                {
                    totalCount = totalCount,
                    page = page,
                    pageSize = pageSize,
                    totalPages = totalPages,
                    data = data
                };

                return Result<AlSadatSeram.Services.contract.ApiResponse<List<PlumberDto>>>.Success(envelope);
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<ApiResponse<List<PlumberDto>>>.Failure(
                    "حدث خطأ أثناء تحميل السباكين", HttpStatusCode.InternalServerError);
            }
        }

        // =======================================================================
        // 2) GET BY ID
        // =======================================================================
        public async Task<Result<PlumberDto>> GetById(int id)
        {
            try
            {
                var plumber = await _unitOfWork.GetRepository<Plumber, int>()
                    .GetQueryable()
                    .Include(p => p.city)
                    .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

                if (plumber == null)
                    return Result<PlumberDto>.Failure("السباك غير موجود", HttpStatusCode.NotFound);

                var dto = new PlumberDto
                {
                    id = plumber.Id,
                    name = plumber.Name,
                    phoneNumbers = plumber.phoneNumbers,
                    address = plumber.address,
                    cityId = plumber.cityId,
                    cityName = plumber.city?.Name,
                    licenseNumber = plumber.LicenseNumber,
                    specialty = plumber.Specialty,
                    isDeleted = plumber.IsDeleted
                };

                return Result<PlumberDto>.Success(dto);
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<PlumberDto>.Failure(
                    "حدث خطأ أثناء تحميل السباك", HttpStatusCode.InternalServerError);
            }
        }

        // =======================================================================
        // 3) LOOKUPS — for select boxes (active only)
        // =======================================================================
        public async Task<Result<List<PlumberLookupDto>>> GetPlumberLookups(PlumberLookupFilter filter)
        {
            try
            {
                var query = _unitOfWork.GetRepository<Plumber, int>()
                    .GetQueryable()
                    .Where(p => !p.IsDeleted);

                if (!string.IsNullOrWhiteSpace(filter.name))
                {
                    var name = filter.name.Trim();
                    query = query.Where(p => p.Name.Contains(name));
                }

                if (!string.IsNullOrWhiteSpace(filter.specialty))
                {
                    var sp = filter.specialty.Trim();
                    query = query.Where(p => p.Specialty != null && p.Specialty.Contains(sp));
                }

                var lookups = await query
                    .OrderBy(p => p.Name)
                    .Select(p => new PlumberLookupDto
                    {
                        id = p.Id,
                        name = p.Name,
                        specialty = p.Specialty
                    })
                    .ToListAsync();

                return Result<List<PlumberLookupDto>>.Success(lookups);
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<List<PlumberLookupDto>>.Failure(
                    "حدث خطأ أثناء تحميل قائمة السباكين", HttpStatusCode.InternalServerError);
            }
        }

        // =======================================================================
        // 4) CREATE
        // =======================================================================
        public async Task<Result<string>> AddNewPlumber(PlumberDto dto)
        {
            if (!TryValidate(dto, out var validationMessage))
                return Result<string>.Failure(validationMessage!, HttpStatusCode.BadRequest);

            try
            {
                var repo = _unitOfWork.GetRepository<Plumber, int>();

                var trimmedName = dto.name.Trim();
                var trimmedPhone = dto.phoneNumbers.Trim();
                var trimmedLicense = string.IsNullOrWhiteSpace(dto.licenseNumber)
                    ? null
                    : dto.licenseNumber.Trim();

                // Duplicate guard 1: same Name AND Phone among ACTIVE plumbers.
                var nameAndPhoneDup = await repo.GetQueryable()
                    .AnyAsync(p =>
                        !p.IsDeleted &&
                        p.Name == trimmedName &&
                        p.phoneNumbers == trimmedPhone);

                if (nameAndPhoneDup)
                    return Result<string>.Failure(
                        "يوجد سباك آخر بنفس الاسم ورقم الهاتف", HttpStatusCode.Conflict);

                // Duplicate guard 2: same LicenseNumber among ACTIVE plumbers (if provided).
                if (trimmedLicense != null)
                {
                    var licenseDup = await repo.GetQueryable()
                        .AnyAsync(p =>
                            !p.IsDeleted &&
                            p.LicenseNumber == trimmedLicense);

                    if (licenseDup)
                        return Result<string>.Failure(
                            "رقم الرخصة مسجل لسباك آخر", HttpStatusCode.Conflict);
                }

                var plumber = new Plumber
                {
                    Name = trimmedName,
                    phoneNumbers = trimmedPhone,
                    address = string.IsNullOrWhiteSpace(dto.address) ? null : dto.address.Trim(),
                    cityId = dto.cityId,
                    LicenseNumber = trimmedLicense,
                    Specialty = string.IsNullOrWhiteSpace(dto.specialty) ? null : dto.specialty.Trim(),
                    IsDeleted = false
                };

                await repo.AddWithoutSaveAsync(plumber);
                await _unitOfWork.SaveChangesAsync();

                return Result<string>.Success("تم إضافة السباك بنجاح");
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<string>.Failure(
                    "حدث خطأ أثناء حفظ السباك", HttpStatusCode.InternalServerError);
            }
        }

        // =======================================================================
        // 5) EDIT — cannot edit soft-deleted; does NOT touch IsDeleted
        // =======================================================================
        public async Task<Result<string>> EditPlumber(PlumberDto dto)
        {
            if (dto.id is null or <= 0)
                return Result<string>.Failure("معرّف السباك مطلوب", HttpStatusCode.BadRequest);

            if (!TryValidate(dto, out var validationMessage))
                return Result<string>.Failure(validationMessage!, HttpStatusCode.BadRequest);

            try
            {
                var repo = _unitOfWork.GetRepository<Plumber, int>();

                var plumber = await repo.GetQueryable()
                    .FirstOrDefaultAsync(p => p.Id == dto.id.Value);

                if (plumber == null)
                    return Result<string>.Failure("السباك غير موجود", HttpStatusCode.NotFound);

                if (plumber.IsDeleted)
                    return Result<string>.Failure(
                        "لا يمكن تعديل سباك محذوف — قم بإعادة تفعيله أولاً",
                        HttpStatusCode.BadRequest);

                var trimmedName = dto.name.Trim();
                var trimmedPhone = dto.phoneNumbers.Trim();
                var trimmedLicense = string.IsNullOrWhiteSpace(dto.licenseNumber)
                    ? null
                    : dto.licenseNumber.Trim();

                // Duplicate guard — same Name + Phone on a DIFFERENT active plumber
                var conflict = await repo.GetQueryable()
                    .AnyAsync(p =>
                        p.Id != dto.id.Value &&
                        !p.IsDeleted &&
                        p.Name == trimmedName &&
                        p.phoneNumbers == trimmedPhone);

                if (conflict)
                    return Result<string>.Failure(
                        "يوجد سباك آخر بنفس الاسم ورقم الهاتف", HttpStatusCode.Conflict);

                // Duplicate guard — same License on a DIFFERENT active plumber
                if (trimmedLicense != null)
                {
                    var licenseConflict = await repo.GetQueryable()
                        .AnyAsync(p =>
                            p.Id != dto.id.Value &&
                            !p.IsDeleted &&
                            p.LicenseNumber == trimmedLicense);

                    if (licenseConflict)
                        return Result<string>.Failure(
                            "رقم الرخصة مسجل لسباك آخر", HttpStatusCode.Conflict);
                }

                plumber.Name = trimmedName;
                plumber.phoneNumbers = trimmedPhone;
                plumber.address = string.IsNullOrWhiteSpace(dto.address) ? null : dto.address.Trim();
                plumber.cityId = dto.cityId;
                plumber.LicenseNumber = trimmedLicense;
                plumber.Specialty = string.IsNullOrWhiteSpace(dto.specialty)
                    ? null
                    : dto.specialty.Trim();
                // IsDeleted intentionally NOT touched — use ToggleStatus.

                repo.UpdateWithoutSaveAsync(plumber);
                await _unitOfWork.SaveChangesAsync();

                return Result<string>.Success("تم تحديث السباك بنجاح");
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<string>.Failure(
                    "حدث خطأ أثناء تحديث السباك", HttpStatusCode.InternalServerError);
            }
        }

        // =======================================================================
        // 6) TOGGLE STATUS — dedicated endpoint flips IsDeleted
        // =======================================================================
        public async Task<Result<string>> TogglePlumberStatus(int id)
        {
            try
            {
                var repo = _unitOfWork.GetRepository<Plumber, int>();
                var plumber = await repo.GetByIdAsync(id);

                if (plumber == null)
                    return Result<string>.Failure("السباك غير موجود", HttpStatusCode.NotFound);

                plumber.IsDeleted = !plumber.IsDeleted;
                repo.UpdateWithoutSaveAsync(plumber);
                await _unitOfWork.SaveChangesAsync();

                var msg = plumber.IsDeleted ? "تم إيقاف السباك" : "تم تفعيل السباك";
                return Result<string>.Success(msg);
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<string>.Failure(
                    "حدث خطأ أثناء تغيير حالة السباك", HttpStatusCode.InternalServerError);
            }
        }

        // =======================================================================
        // 7) IMPORT FROM EXCEL — partial success with per-row reporting
        // =======================================================================
        public async Task<Result<PlumberImportResultDto>> ImportFromExcelAsync(
            Stream fileStream, CancellationToken ct)
        {
            var result = new PlumberImportResultDto();

            try
            {
                ct.ThrowIfCancellationRequested();

                using var workbook = new XLWorkbook(fileStream);
                var ws = workbook.Worksheets.FirstOrDefault();
                if (ws is null)
                    return Result<PlumberImportResultDto>.Failure(
                        "الملف لا يحتوي على أي ورقة عمل", HttpStatusCode.BadRequest);

                // Header lookup (case-insensitive). Required: Name, PhoneNumbers.
                // Optional: LicenseNumber, Specialty.
                var headers = ws.Row(1).CellsUsed()
                    .ToDictionary(
                        c => c.GetString().Trim(),
                        c => c.Address.ColumnNumber,
                        StringComparer.OrdinalIgnoreCase);

                if (!headers.ContainsKey("Name") || !headers.ContainsKey("PhoneNumbers"))
                    return Result<PlumberImportResultDto>.Failure(
                        "تأكد من وجود الأعمدة المطلوبة: Name و PhoneNumbers",
                        HttpStatusCode.BadRequest);

                var dataRows = ws.RangeUsed()?.RowsUsed().Skip(1).ToList()
                               ?? new List<IXLRangeRow>();

                result.totalRows = dataRows.Count;

                // Pre-load active rows once for duplicate-against-DB check.
                var existingActive = await _unitOfWork.GetRepository<Plumber, int>()
                    .GetQueryable()
                    .Where(p => !p.IsDeleted)
                    .Select(p => new { p.Name, p.phoneNumbers, p.LicenseNumber })
                    .ToListAsync();

                var existingNamePhoneKeys = existingActive
                    .Select(p => ComposeKey(p.Name, p.phoneNumbers))
                    .ToHashSet();

                var existingLicenses = existingActive
                    .Where(p => !string.IsNullOrWhiteSpace(p.LicenseNumber))
                    .Select(p => p.LicenseNumber!)
                    .ToHashSet();

                var seenInFileNamePhone = new HashSet<string>();
                var seenInFileLicense = new HashSet<string>();

                foreach (var row in dataRows)
                {
                    ct.ThrowIfCancellationRequested();

                    var excelRow = row.RangeAddress.FirstAddress.RowNumber;

                    string rawName = ReadCell(row, headers, "Name");
                    string rawPhone = ReadCell(row, headers, "PhoneNumbers");
                    string rawLicense = ReadCell(row, headers, "LicenseNumber");
                    string rawSpecialty = ReadCell(row, headers, "Specialty");

                    // Skip fully-empty rows silently
                    if (string.IsNullOrWhiteSpace(rawName) &&
                        string.IsNullOrWhiteSpace(rawPhone) &&
                        string.IsNullOrWhiteSpace(rawLicense) &&
                        string.IsNullOrWhiteSpace(rawSpecialty))
                    {
                        result.totalRows--;
                        continue;
                    }

                    // Field-level validation
                    if (string.IsNullOrWhiteSpace(rawName))
                    {
                        result.errors.Add(BuildError(excelRow, rawName, "Name", "اسم السباك مطلوب"));
                        continue;
                    }

                    var normalizedPhone = NormalizePhone(rawPhone);
                    if (!IsValidE164(normalizedPhone))
                    {
                        result.errors.Add(BuildError(excelRow, rawName, "PhoneNumbers",
                            "رقم الهاتف غير صالح — استخدم صيغة دولية مثل +201012345678"));
                        continue;
                    }

                    // Length guards mirror the DTO MaxLengths
                    if (rawName.Length > 200)
                    {
                        result.errors.Add(BuildError(excelRow, rawName, "Name",
                            "اسم السباك لا يمكن أن يتجاوز 200 حرف"));
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(rawLicense) && rawLicense.Length > 50)
                    {
                        result.errors.Add(BuildError(excelRow, rawName, "LicenseNumber",
                            "رقم الرخصة لا يمكن أن يتجاوز 50 حرف"));
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(rawSpecialty) && rawSpecialty.Length > 100)
                    {
                        result.errors.Add(BuildError(excelRow, rawName, "Specialty",
                            "التخصص لا يمكن أن يتجاوز 100 حرف"));
                        continue;
                    }

                    // Duplicate within the file — Name + Phone
                    var fileKey = ComposeKey(rawName.Trim(), normalizedPhone);
                    if (!seenInFileNamePhone.Add(fileKey))
                    {
                        result.errors.Add(BuildError(excelRow, rawName, "Name",
                            "هذا السباك مكرر داخل الملف نفسه"));
                        continue;
                    }

                    // Duplicate within the file — LicenseNumber
                    if (!string.IsNullOrWhiteSpace(rawLicense))
                    {
                        var lic = rawLicense.Trim();
                        if (!seenInFileLicense.Add(lic))
                        {
                            result.errors.Add(BuildError(excelRow, rawName, "LicenseNumber",
                                "رقم الرخصة مكرر داخل الملف"));
                            continue;
                        }
                    }

                    // Duplicate against DB — Name + Phone
                    if (existingNamePhoneKeys.Contains(fileKey))
                    {
                        result.errors.Add(BuildError(excelRow, rawName, "Name",
                            "يوجد سباك مسجل مسبقًا بنفس الاسم ورقم الهاتف"));
                        continue;
                    }

                    // Duplicate against DB — LicenseNumber
                    if (!string.IsNullOrWhiteSpace(rawLicense) &&
                        existingLicenses.Contains(rawLicense.Trim()))
                    {
                        result.errors.Add(BuildError(excelRow, rawName, "LicenseNumber",
                            "رقم الرخصة مسجل لسباك آخر"));
                        continue;
                    }

                    // Persist the row — its own SaveChangesAsync, no ambient transaction.
                    var plumber = new Plumber
                    {
                        Name = rawName.Trim(),
                        phoneNumbers = normalizedPhone,
                        LicenseNumber = string.IsNullOrWhiteSpace(rawLicense) ? null : rawLicense.Trim(),
                        Specialty = string.IsNullOrWhiteSpace(rawSpecialty) ? null : rawSpecialty.Trim(),
                        address = null,
                        cityId = null,
                        IsDeleted = false
                    };

                    try
                    {
                        await _unitOfWork.GetRepository<Plumber, int>().AddWithoutSaveAsync(plumber);
                        await _unitOfWork.SaveChangesAsync();

                        result.successCount++;
                        result.imported.Add(new PlumberDto
                        {
                            id = plumber.Id,
                            name = plumber.Name,
                            phoneNumbers = plumber.phoneNumbers,
                            licenseNumber = plumber.LicenseNumber,
                            specialty = plumber.Specialty,
                            isDeleted = false
                        });
                    }
                    catch (Exception ex)
                    {
                        await _unitOfWork.LogError(ex);
                        result.errors.Add(BuildError(excelRow, rawName, "row",
                            "فشل الحفظ في قاعدة البيانات"));
                    }
                }

                result.failedCount = result.errors.Count;

                var message = (result.successCount, result.failedCount) switch
                {
                    ( > 0, 0) => "تم استيراد جميع السباكين بنجاح",
                    ( > 0, > 0) => $"تم استيراد {result.successCount} سباك بنجاح، وفشل {result.failedCount}",
                    (0, > 0) => "فشل استيراد جميع الصفوف — راجع قائمة الأخطاء",
                    _ => "لا توجد بيانات للاستيراد"
                };

                return Result<PlumberImportResultDto>.Success(result, message);
            }
            catch (OperationCanceledException)
            {
                return Result<PlumberImportResultDto>.Failure(
                    "تم إلغاء عملية الاستيراد", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<PlumberImportResultDto>.Failure(
                    "حدث خطأ أثناء استيراد الملف — تأكد من صحة التنسيق",
                    HttpStatusCode.InternalServerError);
            }
        }

        // =======================================================================
        // 8) GENERATE TEMPLATE — two-sheet .xlsx via ClosedXML
        // =======================================================================
        public Task<Result<byte[]>> GenerateImportTemplateAsync(CancellationToken ct)
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                using var workbook = new XLWorkbook();

                // ---- Sheet 1: data sheet the importer reads ----
                var ws = workbook.Worksheets.Add("Plumbers");

                ws.Cell(1, 1).Value = "Name";
                ws.Cell(1, 2).Value = "PhoneNumbers";
                ws.Cell(1, 3).Value = "LicenseNumber";
                ws.Cell(1, 4).Value = "Specialty";

                var header = ws.Range(1, 1, 1, 4);
                header.Style.Font.Bold = true;
                header.Style.Font.FontColor = XLColor.Black;
                header.Style.Fill.BackgroundColor = XLColor.FromHtml("#D4AF37");
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Force PhoneNumbers + LicenseNumber columns to TEXT format so Excel
                // doesn't silently coerce "+201012345678" into a number.
                var phoneCol = ws.Column(2);
                phoneCol.Style.NumberFormat.Format = "@";
                ws.Range(2, 2, 1000, 2).Style.NumberFormat.Format = "@";

                var licenseCol = ws.Column(3);
                licenseCol.Style.NumberFormat.Format = "@";
                ws.Range(2, 3, 1000, 3).Style.NumberFormat.Format = "@";

                // Example row (italic + grey to signal "delete me before import")
                ws.Cell(2, 1).Value = "أحمد محمود";
                ws.Cell(2, 2).SetValue("+201012345678");
                ws.Cell(2, 3).SetValue("LIC-2024-001");
                ws.Cell(2, 4).Value = "سباكة منزلية";

                ws.Range(2, 1, 2, 4).Style.Font.Italic = true;
                ws.Range(2, 1, 2, 4).Style.Font.FontColor = XLColor.Gray;

                ws.SheetView.FreezeRows(1);
                ws.Columns().AdjustToContents();

                // ---- Sheet 2: Instructions ----
                var help = workbook.Worksheets.Add("Instructions");

                help.Cell(1, 1).Value = "تعليمات الاستيراد — السباكون";
                help.Cell(1, 1).Style.Font.Bold = true;
                help.Cell(1, 1).Style.Font.FontSize = 14;

                help.Cell(3, 1).Value = "الأعمدة المطلوبة:";
                help.Cell(3, 1).Style.Font.Bold = true;
                help.Cell(4, 1).Value = "• Name — اسم السباك (إلزامي)";
                help.Cell(5, 1).Value = "• PhoneNumbers — رقم الهاتف بصيغة دولية مثل +201012345678 (إلزامي)";

                help.Cell(7, 1).Value = "الأعمدة الاختيارية:";
                help.Cell(7, 1).Style.Font.Bold = true;
                help.Cell(8, 1).Value = "• LicenseNumber — رقم الرخصة المهنية (يجب أن يكون فريدًا)";
                help.Cell(9, 1).Value = "• Specialty — التخصص، مثل: سباكة منزلية / سباكة تجارية / سباكة صناعية";

                help.Cell(11, 1).Value = "ملاحظات عامة:";
                help.Cell(11, 1).Style.Font.Bold = true;
                help.Cell(12, 1).Value = "1. احذف الصف التجريبي (الثاني) في ورقة Plumbers قبل الاستيراد.";
                help.Cell(13, 1).Value = "2. الصفوف الفارغة يتم تجاهلها تلقائيًا.";
                help.Cell(14, 1).Value = "3. الصفوف المكررة (نفس الاسم ورقم الهاتف) داخل الملف أو في النظام يتم رفضها.";
                help.Cell(15, 1).Value = "4. رقم الرخصة المكرر يتم رفضه أيضًا.";
                help.Cell(16, 1).Value = "5. بعد الاستيراد، ادخل لتعديل كل سباك وإضافة المدينة والعنوان.";

                help.Column(1).Width = 90;

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
        // 9) EXPORT — current filtered list as .xlsx
        // =======================================================================
        public async Task<Result<byte[]>> ExportToExcelAsync(
            PlumberFilteration filter, CancellationToken ct)
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                // Reuse GetAllPlumbers but force "all" pagination so export is complete.
                var listFilter = new PlumberFilteration
                {
                    name = filter.name,
                    phoneNumbers = filter.phoneNumbers,
                    licenseNumber = filter.licenseNumber,
                    specialty = filter.specialty,
                    isDeleted = filter.isDeleted,
                    page = null,
                    pageSize = null
                };

                var listResult = await GetAllPlumbers(listFilter);
                if (!listResult.IsSuccess || listResult.Data is null)
                    return Result<byte[]>.Failure(
                        listResult.Message ?? "تعذر تحميل البيانات للتصدير",
                        listResult.StatusCode);

                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Plumbers");
                ws.RightToLeft = true;

                // -----------------------------------------------------------------
                // CRITICAL: pre-format phone (col 2) and license (col 3) as TEXT
                // BEFORE writing any values. ClosedXML decides storage at SetValue
                // time — applying "@" afterwards only changes display, not storage,
                // and Excel would still strip leading '+' and leading zeros.
                // -----------------------------------------------------------------
                ws.Column(2).Style.NumberFormat.Format = "@";
                ws.Column(3).Style.NumberFormat.Format = "@";

                // Header row
                ws.Cell(1, 1).Value = "الاسم";
                ws.Cell(1, 2).Value = "رقم الهاتف";
                ws.Cell(1, 3).Value = "رقم الرخصة";
                ws.Cell(1, 4).Value = "التخصص";
                ws.Cell(1, 5).Value = "العنوان";
                ws.Cell(1, 6).Value = "المدينة";
                ws.Cell(1, 7).Value = "الحالة";

                var header = ws.Range(1, 1, 1, 7);
                header.Style.Font.Bold = true;
                header.Style.Font.FontColor = XLColor.Black;
                header.Style.Fill.BackgroundColor = XLColor.FromHtml("#D4AF37");
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Data rows — column-level "@" format already applied,
                // so SetValue will store these as strings without numeric coercion.
                int row = 2;
                foreach (var p in listResult.Data.data)
                {
                    ws.Cell(row, 1).Value = SanitizeForExcel(p.name);
                    ws.Cell(row, 2).SetValue(SanitizeForExcel(p.phoneNumbers) ?? string.Empty);
                    ws.Cell(row, 3).SetValue(SanitizeForExcel(p.licenseNumber) ?? string.Empty);
                    ws.Cell(row, 4).Value = SanitizeForExcel(p.specialty);
                    ws.Cell(row, 5).Value = SanitizeForExcel(p.address);
                    ws.Cell(row, 6).Value = SanitizeForExcel(p.cityName);
                    ws.Cell(row, 7).Value = p.isDeleted ? "موقوف" : "فعال";
                    row++;
                }

                ws.SheetView.FreezeRows(1);
                ws.Columns().AdjustToContents();

                using var ms = new MemoryStream();
                workbook.SaveAs(ms);
                return Result<byte[]>.Success(ms.ToArray());
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<byte[]>.Failure(
                    "تعذر تصدير البيانات", HttpStatusCode.InternalServerError);
            }
        }

        // =======================================================================
        // Helpers
        // =======================================================================

        private static bool TryValidate(object instance, out string? message)
        {
            var ctx = new ValidationContext(instance);
            var errors = new List<ValidationResult>();
            var ok = Validator.TryValidateObject(instance, ctx, errors, validateAllProperties: true);

            if (ok) { message = null; return true; }

            message = errors[0].ErrorMessage ?? "البيانات غير صالحة";
            return false;
        }

        private static string ReadCell(IXLRangeRow row, Dictionary<string, int> headers, string columnName)
        {
            if (!headers.TryGetValue(columnName, out var colIndex)) return string.Empty;
            return row.Cell(colIndex).GetString().Trim();
        }

        private static PlumberImportRowError BuildError(int rowNumber, string? plumberName, string column, string message)
            => new()
            {
                rowNumber = rowNumber,
                plumberName = plumberName,
                column = column,
                message = message
            };

        /// <summary>
        /// Composes a case-insensitive lookup key for duplicate detection.
        /// Same algorithm as <c>SupplierService.ComposeKey</c> for consistency.
        /// </summary>
        private static string ComposeKey(string name, string phone)
            => $"{name.Trim().ToLowerInvariant()}|{phone.Trim()}";

        /// <summary>
        /// Coerces user input into E.164 by adding a leading '+' when missing.
        /// Matches the supplier-import behaviour.
        /// </summary>
        private static string NormalizePhone(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            var trimmed = raw.Trim();
            return trimmed.StartsWith("+") ? trimmed : "+" + trimmed;
        }

        private static bool IsValidE164(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(
                phone, @"^\+[1-9]\d{6,14}$");
        }

        /// <summary>
        /// Escapes a cell value against Excel formula injection on EXPORT only.
        /// </summary>
        private static string? SanitizeForExcel(string? value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            var trimmed = value.TrimStart();
            if (trimmed.Length == 0) return value;
            char first = trimmed[0];
            if (first is '=' or '+' or '-' or '@') return "'" + value;
            return value;
        }
    }
}

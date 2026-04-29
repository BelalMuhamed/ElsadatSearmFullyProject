using AlSadatSeram.Services.contract;
using Application.DTOs;
using Application.DTOs.FinanceDtos;
using Application.Services.contract;
using Application.Services.contract.CurrentUserService;
using ClosedXML.Excel;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.Finance;
using Domain.Entities.Transactions;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.UnitOfWork.Contract;
using Infrastructure.Services.CurrentUserServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class DistributorsAndMerchantsService : IDistributorsAndMerchantsService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IServiceManager _serviceManager;
        private readonly ICurrentUserService currentUserService;
        private readonly IExcelReaderService excelService;

        public DistributorsAndMerchantsService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager,IServiceManager serviceManager, ICurrentUserService currentUserService,
        IExcelReaderService excelService)
        {
            this.unitOfWork = unitOfWork;
            this._userManager = userManager;
            _serviceManager = serviceManager;
            this.currentUserService = currentUserService;
            this.excelService = excelService;
        }

        public async Task<Result<string>> AddNewDistributorOrMerchant(DistributorsAndMerchantsAndAgentsDto dto)
        {
            await unitOfWork.BeginTransactionAsync();

            try
            {
                // 1️⃣ VALIDATION
                if (string.IsNullOrWhiteSpace(dto.fullName))
                    return Result<string>.Failure("الإسم مطلوب", HttpStatusCode.BadRequest);

                if (string.IsNullOrWhiteSpace(dto.phoneNumber))
                    return Result<string>.Failure("رقم الهاتف مطلوب", HttpStatusCode.BadRequest);

                


                // 2️⃣ CHECK UNIQUE PHONE
                var userRepo = unitOfWork.GetRepository<ApplicationUser, string>();

                bool exists = await userRepo.GetQueryable()
                                            .AnyAsync(u => u.PhoneNumber == dto.phoneNumber);

                if (exists)
                    return Result<string>.Failure("رقم الهاتف مستخدم بالفعل", HttpStatusCode.Conflict);


                // 3️⃣ CREATE USER
                var passwordHasher = new PasswordHasher<ApplicationUser>();

                var user = new ApplicationUser
                {
                    FullName = dto.fullName,
                    UserName = dto.phoneNumber,
                    Email = dto.phoneNumber,
                    NormalizedUserName = dto.phoneNumber.ToUpper(),
                    NormalizedEmail = dto.phoneNumber.ToUpper(),
                    PhoneNumber = dto.phoneNumber,
                   
                    CityID = dto.cityId,
                    CreateAt = DateTime.Now,
                    CreateBy = dto.createdBy,
                    Address = dto.address,
                    IsDeleted = false,
                    
                };

                user.PasswordHash = passwordHasher.HashPassword(user, "12345678Ss+");

                await userRepo.AddWithoutSaveAsync(user);


                // 4️⃣ SAVE USER FIRST — so we get user.Id
                int saved = await unitOfWork.SaveChangesAsync();

                if (saved <= 0)
                {
                    await unitOfWork.RollbackAsync();
                    return Result<string>.Failure("فشل إنشاء المستخدم", HttpStatusCode.BadRequest);
                }
                var userType = (DistributorOrMerchantOrAgent)dto.type;
                // 4.5️⃣ ASSIGN ROLE
                string roleToAssign = userType switch
                {
                 DistributorOrMerchantOrAgent.Merchant => AppRoles.Merchant,
                    DistributorOrMerchantOrAgent.Agents => AppRoles.Agent,
                    DistributorOrMerchantOrAgent.Distributor => AppRoles.Distributor,
                    _ => AppRoles.Merchant
                };

                var roleResult = await _userManager.AddToRoleAsync(user, roleToAssign);

                if (!roleResult.Succeeded)
                {
                    await unitOfWork.RollbackAsync();
                    return Result<string>.Failure("فشل تعيين الدور للمستخدم", HttpStatusCode.BadRequest);
                }
                // 5️⃣ NOW CREATE Distributor/Merchant using the generated user.Id
                var distRepo = unitOfWork.GetRepository<Distributor_Merchant_Agent, string>();

                var entity = new Distributor_Merchant_Agent
                {
                    UserId = user.Id,   // <<< NOW it's available
                    Balance = 0,
                    CashBalance = 0,
                    Indebtedness = 0,
                    FirstSpecialDiscount=dto.firstSpecialDiscount,
                    SecondSpecialDiscount=dto.secondSpecialDiscount,
                    ThirdSpecialDiscount=dto.thirdSpecialDiscount,
                    Type = (DistributorOrMerchantOrAgent)dto.type
                };
                
                await distRepo.AddWithoutSaveAsync(entity);
                #region account
                // add Accountant account for supplier in tree account

                AccountDto CustomerAccountToAddInTreeAccountant = new AccountDto()
                {
                    userId = user.Id.ToString(),
                    accountName = user.FullName,
                    type = 0,
                    parentAccountId =8,
                    isLeaf = true,
                    isActive = true,

                };
                var IsAccountAdded = await _serviceManager.treeService.AddNewAccount(CustomerAccountToAddInTreeAccountant);
                if (!IsAccountAdded.IsSuccess)
                    return IsAccountAdded;
                #endregion


                // 6️⃣ SAVE BOTH CHANGES


                if (saved > 0)
                {
                    await unitOfWork.CommitAsync();
                    return Result<string>.Success($"تم إضافة {dto.fullName} بنجاح", HttpStatusCode.OK);
                }
                else
                {
                    await unitOfWork.RollbackAsync();
                    return Result<string>.Failure("فشل إضافة التاجر/الموزع/الوكيل", HttpStatusCode.BadRequest);
                }
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync();
                return Result<string>.Failure("حدث خطأ أثناء الإضافة", HttpStatusCode.InternalServerError);
            }
        }

      
        public async Task<ApiResponse<List<DistributorsAndMerchantsAndAgentsDto>>>
        GetAllDistributorsAndMerchants(DistributorsAndMerchantsFilters req)
        {
            IQueryable<Distributor_Merchant_Agent> query  = unitOfWork
                .GetRepository<Distributor_Merchant_Agent, string>()
                .GetQueryable()
                .Include(x => x.User)
                .ThenInclude(u => u.City);

            // -------------------------------
            // Filters
            // -------------------------------
            if (!string.IsNullOrWhiteSpace(req.fullName))
                query = query.Where(x => x.User.FullName.Contains(req.fullName));

            if (!string.IsNullOrWhiteSpace(req.phoneNumber))
                query = query.Where(x => x.User.PhoneNumber.Contains(req.phoneNumber));

            if (!string.IsNullOrWhiteSpace(req.cityName))
                query = query.Where(x => x.User.City.Name.Contains(req.cityName));

            if (req.type != null)
                query = query.Where(x => (int)x.Type == req.type);

            if (req.isDeleted != null)
                query = query.Where(x => x.User.IsDeleted == req.isDeleted);

            // -------------------------------
            // Total Count (after filters)
            // -------------------------------
            var totalCount = await query.CountAsync();

            // -------------------------------
            // Sorting
            // -------------------------------
            query = query.OrderByDescending(x => x.User.CreateAt);

            // -------------------------------
            // Pagination (ONLY if page & pageSize are NOT null)
            // -------------------------------
            int page = 1;
            int pageSize = totalCount;

            if (req.page.HasValue && req.pageSize.HasValue)
            {
                page = req.page.Value <= 0 ? 1 : req.page.Value;
                pageSize = req.pageSize.Value <= 0 ? 10 : req.pageSize.Value;

                var skip = (page - 1) * pageSize;
                query = query.Skip(skip).Take(pageSize);
            }

            // -------------------------------
            // Select DTO
            // -------------------------------
            var list = await query
                .Select(x => new DistributorsAndMerchantsAndAgentsDto
                {
                    userId = x.UserId,
                    fullName = x.User.FullName,
                    address = x.User.Address,
                    gender = (int?)x.User.Gender,
                    type = (int)x.Type,

                    createdAt = x.User.CreateAt,
                    createdBy = x.User.CreateBy,
                    updatedAt = x.User.UpdateAt,
                    updatedBy = x.User.UpdateBy,
                    isDelted = x.User.IsDeleted,
                    deletedAt = x.User.DeleteAt,
                    deletedBy = x.User.DeleteBy,

                    cityId = x.User.CityID,
                    cityName = x.User.City != null ? x.User.City.Name : null,

                    phoneNumber = x.User.PhoneNumber,

                    firstSpecialDiscount = x.FirstSpecialDiscount,
                    secondSpecialDiscount = x.SecondSpecialDiscount,
                    thirdSpecialDiscount = x.ThirdSpecialDiscount,

                    PointsBalance = x.Balance,
                    cashBalance = x.CashBalance,
                    indebtedness = x.Indebtedness
                })
                .ToListAsync();

            // -------------------------------
            // Final Response
            // -------------------------------
            return new ApiResponse<List<DistributorsAndMerchantsAndAgentsDto>>
            {
                data = list,
                totalCount = totalCount,
                page = req.page ?? 1,
                pageSize = req.pageSize ?? totalCount,
                totalPages = req.page.HasValue && req.pageSize.HasValue
                    ? (int)Math.Ceiling(totalCount / (double)pageSize)
                    : 1
            };
        }
        public async Task<Result<string>> EditDistributorOrMerchant(
            DistributorsAndMerchantsAndAgentsDto dto)
        {
            // ───────── 1. Validation (aligned with Excel import contract) ─────────
            if (string.IsNullOrWhiteSpace(dto.userId))
                return Result<string>.Failure("معرف المستخدم مطلوب", HttpStatusCode.BadRequest);

            if (string.IsNullOrWhiteSpace(dto.fullName))
                return Result<string>.Failure("الإسم مطلوب", HttpStatusCode.BadRequest);

            if (string.IsNullOrWhiteSpace(dto.phoneNumber))
                return Result<string>.Failure("رقم الهاتف مطلوب", HttpStatusCode.BadRequest);

            if (dto.type is null || dto.type < 0 || dto.type > 2)
                return Result<string>.Failure("النوع غير صالح", HttpStatusCode.BadRequest);

            // ⛔ REMOVED: cityId required check — city is OPTIONAL (Excel import support).
            // ⛔ REMOVED: address required check — address is OPTIONAL.

            await unitOfWork.BeginTransactionAsync();

            try
            {
                var userRepo = unitOfWork.GetRepository<ApplicationUser, string>();
                var distRepo = unitOfWork.GetRepository<Distributor_Merchant_Agent, string>();

                // ───────── 2. Load existing aggregate ─────────
                var user = await userRepo.GetQueryable()
                                         .Include(x => x.City)
                                         .FirstOrDefaultAsync(u => u.Id == dto.userId);

                if (user is null)
                {
                    await unitOfWork.RollbackAsync();
                    return Result<string>.Failure("المستخدم غير موجود", HttpStatusCode.NotFound);
                }

                var dist = await distRepo.GetQueryable()
                                         .FirstOrDefaultAsync(x => x.UserId == user.Id);

                if (dist is null)
                {
                    await unitOfWork.RollbackAsync();
                    return Result<string>.Failure("البيانات غير موجودة", HttpStatusCode.NotFound);
                }

                // ───────── 3. Phone uniqueness ─────────
                bool phoneExists = await userRepo.GetQueryable()
                    .AnyAsync(x => x.UserName == dto.phoneNumber && x.Id != user.Id);

                if (phoneExists)
                {
                    await unitOfWork.RollbackAsync();
                    return Result<string>.Failure(
                        "رقم الهاتف مستخدم بالفعل من مستخدم آخر",
                        HttpStatusCode.Conflict);
                }

                // ───────── 4. Apply patch ─────────
                // Required fields (validated above) overwrite directly.
                user.FullName = dto.fullName!;
                user.PhoneNumber = dto.phoneNumber!;
                user.UserName = dto.phoneNumber!;
                user.Email = dto.phoneNumber!;
                user.NormalizedUserName = dto.phoneNumber!.ToUpper();
                user.NormalizedEmail = dto.phoneNumber!.ToUpper();

                // Optional fields — null means "set to null" (true PUT semantics).
                user.Address = dto.address;
                user.CityID = dto.cityId;
                if (dto.gender.HasValue) user.Gender = (Gender)dto.gender.Value;

                // Server-owned audit (DO NOT trust client-supplied values).
                var currentUserId = currentUserService.UserId;
                var currentUser= await userRepo.FindAsync(u=>u.Id==currentUserId);
               
                user.UpdateAt = DateTime.UtcNow;
                user.UpdateBy = currentUser is not null
                    ? $"{currentUser.FullName} / {currentUser.Email}"
                    : user.UpdateBy;

                // Soft-delete toggling (controller-driven; safe to honor here).
                if (dto.isDelted.HasValue)
                {
                    user.IsDeleted = dto.isDelted.Value;
                    user.DeleteAt = dto.isDelted.Value ? DateTime.UtcNow : null;
                    user.DeleteBy = dto.isDelted.Value
                        ? user.UpdateBy
                        : null;
                }

                // Distributor record
                dist.Type = (DistributorOrMerchantOrAgent)dto.type!.Value;
                dist.CashBalance = dto.cashBalance ?? dist.CashBalance;
                dist.Balance = dto.PointsBalance ?? dist.Balance;
                dist.Indebtedness = dto.indebtedness ?? dist.Indebtedness;
                dist.FirstSpecialDiscount = dto.firstSpecialDiscount;
                dist.SecondSpecialDiscount = dto.secondSpecialDiscount;
                dist.ThirdSpecialDiscount = dto.thirdSpecialDiscount;

                // ───────── 5. Persist ─────────
                int saved = await unitOfWork.SaveChangesAsync();

                // saved == 0 is legitimate when nothing changed; treat as success.
                await unitOfWork.CommitAsync();
                return Result<string>.Success("تم التعديل بنجاح", HttpStatusCode.OK);
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync();
                return Result<string>.Failure(
                    "حدث خطأ أثناء تعديل البيانات",
                    HttpStatusCode.InternalServerError);
            }
        }
        public async Task<Result<DistributorsAndMerchantsAndAgentsDto>> GetDistributorOrMerchantById(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return Result<DistributorsAndMerchantsAndAgentsDto>.Failure("رقم المستخدم غير صحيح", HttpStatusCode.BadRequest);

                var distRepo = unitOfWork.GetRepository<Distributor_Merchant_Agent, string>();

                var distributor = await distRepo
                    .GetQueryable()
                    .Include(x => x.User)
                    .ThenInclude(u => u.City)
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (distributor == null)
                    return Result<DistributorsAndMerchantsAndAgentsDto>.Failure("لا يوجد مستخدم بهذا المعرف", HttpStatusCode.NotFound);


                var u = distributor.User;

                var accountRepo = unitOfWork.GetRepository<ChartOfAccounts, int>();
                var detailsRepo = unitOfWork.GetRepository<JournalEntryDetails, int>();

                var account = await accountRepo
                    .GetQueryable()
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.IsLeaf);

                decimal cashBalance = 0;

                if (account != null)
                {
                    var totals = await detailsRepo.GetQueryable().Include(d=>d.JournalEntry)
                        .Where(d => d.AccountId == account.Id &&  d.JournalEntry.IsPosted == true)
                        .GroupBy(d => d.AccountId)
                        .Select(g => new
                        {
                            Debit = g.Sum(x => x.Debit),
                            Credit = g.Sum(x => x.Credit)
                        })
                        .FirstOrDefaultAsync();

                    var totalDebit = totals?.Debit ?? 0;
                    var totalCredit = totals?.Credit ?? 0;

                    cashBalance = totalDebit - totalCredit;
                }
                else
                {
                    return Result<DistributorsAndMerchantsAndAgentsDto>.Failure("لا يوجد حساب في شجرة الحسابات ", HttpStatusCode.BadRequest);
                }
                var pointsRepo = unitOfWork.GetRepository<PointTransactions, int>();

                var receivedPoints = await pointsRepo.GetQueryable()
                    .Where(x => x.ReceverId == userId)
                    .SumAsync(x => (int?)x.TotalPoints) ?? 0;

                var sentPoints = await pointsRepo.GetQueryable()
                    .Where(x => x.SenderId == userId)
                    .SumAsync(x => (int?)x.TotalPoints) ?? 0;

                int pointsBalance = receivedPoints - sentPoints;

                var dto = new DistributorsAndMerchantsAndAgentsDto
                    {
                        userId = u.Id,
                        fullName = u.FullName,
                        address = u.Address,
                        gender = (int?)u.Gender,
                        createdAt = u.CreateAt,
                        createdBy = u.CreateBy,
                        updatedAt = u.UpdateAt,
                        updatedBy = u.UpdateBy,
                        isDelted = u.IsDeleted,
                        deletedAt = u.DeleteAt,
                        deletedBy = u.DeleteBy,
                        cityId = u.CityID,
                        cityName = u.City?.Name,
                        phoneNumber = u.PhoneNumber,
                        type = (int)distributor.Type,
                        PointsBalance = pointsBalance,
                        cashBalance = cashBalance,
                        indebtedness = distributor.Indebtedness,
                        firstSpecialDiscount = distributor.FirstSpecialDiscount,
                        secondSpecialDiscount = distributor.SecondSpecialDiscount,
                        thirdSpecialDiscount = distributor.ThirdSpecialDiscount,

                    };

                return Result<DistributorsAndMerchantsAndAgentsDto>.Success(dto, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Result<DistributorsAndMerchantsAndAgentsDto>.Failure("حدث خطأ أثناء جلب البيانات", HttpStatusCode.InternalServerError);
            }
        }

       

        public async Task<Result<byte[]>> ExportTemplateAsync(CancellationToken ct)
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                using var workbook = new XLWorkbook();

                // =========================
                // Sheet 1
                // =========================
                var ws = workbook.Worksheets.Add("Distributors");

                ws.Cell(1, 1).Value = "الاسم_بالكامل";
                ws.Cell(1, 2).Value = "النوع";
                ws.Cell(1, 3).Value = "رقم_الهاتف";

                // Header style
                var header = ws.Range(1, 1, 1, 3);
                header.Style.Font.Bold = true;
                header.Style.Font.FontSize = 12;
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                header.Style.Fill.BackgroundColor = XLColor.FromHtml("#D4AF37");
                header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // =========================
                // Example row
                // =========================
                ws.Cell(2, 1).Value = "شركة النور التجارية";
                ws.Cell(2, 2).Value = "تاجر";
                ws.Cell(2, 3).SetValue("01012345678");

                ws.Range(2, 1, 2, 3).Style.Font.Italic = true;
                ws.Range(2, 1, 2, 3).Style.Font.FontColor = XLColor.Gray;

                // =========================
                // Phone format as TEXT
                // =========================
                ws.Column(3).Style.NumberFormat.Format = "@";

                // =========================
                // SIMPLE FIX (IMPORTANT)
                // Instead of string list → use array
                // =========================
                var validationRange = ws.Range("B2:B1000");

                var validation = validationRange.CreateDataValidation();

                validation.IgnoreBlanks = true;
                validation.InCellDropdown = true;
                validation.AllowedValues = XLAllowedValues.List;

                // ✅ FIX: array instead of CSV string
                var values = new[] { "موزع", "تاجر", "وكيل" };

                validation.List(string.Join(",", values));              // =========================
                // Freeze header
                // =========================
                ws.SheetView.FreezeRows(1);

                ws.Columns().AdjustToContents();

                // =========================
                // Sheet 2
                // =========================
                var help = workbook.Worksheets.Add("Instructions");

                help.Cell(1, 1).Value = "تعليمات الاستيراد";
                help.Cell(1, 1).Style.Font.Bold = true;

                help.Cell(3, 1).Value = "الأعمدة:";
                help.Cell(4, 1).Value = "الاسم بالكامل - النوع - رقم الهاتف";

                help.Column(1).Width = 80;

                using var ms = new MemoryStream();
                workbook.SaveAs(ms);

                return Result<byte[]>.Success(
                    ms.ToArray(),
                    "تم إنشاء القالب بنجاح");
            }
            catch (OperationCanceledException)
            {
                return Result<byte[]>.Failure(
                    "تم إلغاء العملية",
                    HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                await unitOfWork.LogError(ex);

                return Result<byte[]>.Failure(
                    "حدث خطأ أثناء إنشاء الملف",
                    HttpStatusCode.InternalServerError);
            }
        }

        // =======================================================
        // DistributorsAndMerchantsService
        // UPDATED IMPORT METHOD USING NEW ExcelReaderService.ImportAsync
        // =======================================================

        public async Task<Result<ExcelImportResult<DistributorsAndMerchantsAndAgentsDto>>>
        ImportFromExcelAsync(Stream fileStream, CancellationToken ct)
        {
            try
            {
                var currentUserId = currentUserService.UserId;

                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Result<ExcelImportResult<DistributorsAndMerchantsAndAgentsDto>>
                        .Failure("المستخدم الحالي غير معروف", HttpStatusCode.Unauthorized);

                // preload phones once
                var userRepo = unitOfWork.GetRepository<ApplicationUser, string>();
                var curretUser = await userRepo.FindAsync(u=>u.Id== currentUserId);
                var existingPhones = await userRepo
                    .GetQueryable()
                    .Where(x => !x.IsDeleted)
                    .Select(x => x.PhoneNumber)
                    .ToListAsync(ct);

                var dbPhones = existingPhones
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => NormalizePhone(x!))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var filePhones = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                var importResult =
                    await excelService.ImportAsync<
                        DistributorMerchantExcelDto,
                        DistributorsAndMerchantsAndAgentsDto>(
                    fileStream,

                    async (row, ctx) =>
                    {
                        ct.ThrowIfCancellationRequested();

                        // ==================================
                        // validate full name
                        // ==================================
                        var fullName = row.الاسم_بالكامل?.Trim();
                        Console.WriteLine($"FullName raw: '{row.الاسم_بالكامل}'");
                        if (string.IsNullOrWhiteSpace(fullName))
                            return RowImportResult<DistributorsAndMerchantsAndAgentsDto>
                                .Fail("الاسم بالكامل مطلوب", "الاسم بالكامل");

                        if (fullName.Length > 200)
                            return RowImportResult<DistributorsAndMerchantsAndAgentsDto>
                                .Fail("الاسم بالكامل تجاوز الحد المسموح", "الاسم بالكامل");


                        // ==================================
                        // validate type
                        // ==================================
                        int type;

                        var rawType = row.النوع?.Trim();

                        if (rawType == "موزع")
                            type = (int)DistributorOrMerchantOrAgent.Distributor;

                        else if (rawType == "تاجر")
                            type = (int)DistributorOrMerchantOrAgent.Merchant;

                        else if (rawType == "وكيل")
                            type = (int)DistributorOrMerchantOrAgent.Agents;

                        else
                            return RowImportResult<DistributorsAndMerchantsAndAgentsDto>
                                .Fail("النوع يجب أن يكون موزع أو تاجر أو وكيل", "النوع");


                        // ==================================
                        // validate phone
                        // ==================================
                        var phone = NormalizePhone(row.رقم_الهاتف);

                        if (!IsValidPhone(phone))
                            return RowImportResult<DistributorsAndMerchantsAndAgentsDto>
                                .Fail("رقم الهاتف غير صالح", "رقم الهاتف");


                        // duplicate in file
                        if (!filePhones.Add(phone))
                            return RowImportResult<DistributorsAndMerchantsAndAgentsDto>
                                .Fail("رقم الهاتف مكرر داخل الملف", "رقم الهاتف");


                        // duplicate in DB
                        if (dbPhones.Contains(phone))
                            return RowImportResult<DistributorsAndMerchantsAndAgentsDto>
                                .Fail("رقم الهاتف مستخدم بالفعل", "رقم الهاتف");


                        // ==================================
                        // map dto
                        // ==================================
                        var dto = new DistributorsAndMerchantsAndAgentsDto
                        {
                            fullName = fullName,
                            phoneNumber = phone,
                            type = type,
                         

                            createdBy = curretUser.FullName+ " / " + curretUser.Email,
                            createdAt = DateTime.Now
                        };

                        return RowImportResult<DistributorsAndMerchantsAndAgentsDto>
                            .Success(dto);
                    },

                    ct);


                // ==================================
                // Save valid rows
                // ==================================
                var saveErrors = new List<string>();

                foreach (var item in importResult.Imported)
                {
                    ct.ThrowIfCancellationRequested();

                    var result = await AddNewDistributorOrMerchant(item);

                    if (!result.IsSuccess)
                    {
                        saveErrors.Add($"{item.fullName}: {result.Message}");
                    }
                }
                if (saveErrors.Any())
                {
                    return Result<ExcelImportResult<DistributorsAndMerchantsAndAgentsDto>>
                        .Failure(string.Join(" | ", saveErrors), HttpStatusCode.BadRequest);
                }
                return Result<ExcelImportResult<DistributorsAndMerchantsAndAgentsDto>>
                    .Success(importResult, "تم تنفيذ الاستيراد");
            }
            catch (OperationCanceledException)
            {
                return Result<ExcelImportResult<DistributorsAndMerchantsAndAgentsDto>>
                    .Failure("تم إلغاء العملية", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                await unitOfWork.LogError(ex);

                return Result<ExcelImportResult<DistributorsAndMerchantsAndAgentsDto>>
                    .Failure("حدث خطأ أثناء الاستيراد", HttpStatusCode.InternalServerError);
            }
        }


        // =======================================================
        // HELPERS
        // =======================================================

        private static string NormalizePhone(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            // keep digits only
            var digits = new string(value.Where(char.IsDigit).ToArray());

            if (string.IsNullOrWhiteSpace(digits))
                return string.Empty;

            // Egyptian normalization
            if (digits.StartsWith("0") && digits.Length == 11)
            {
                // 010xxxxxxxx → +2010xxxxxxxx
                return "+2" + digits;
            }

            if (digits.StartsWith("1") && digits.Length == 10)
            {
                // 10xxxxxxxx → +2010xxxxxxxx
                return "+20" + digits;
            }

            if (digits.StartsWith("20") && digits.Length == 12)
            {
                // 2010xxxxxxxx → +2010xxxxxxxx
                return "+" + digits;
            }

            if (digits.StartsWith("201") && digits.Length == 13)
            {
                // already correct without +
                return "+" + digits;
            }

            if (value.StartsWith("+") && digits.StartsWith("20"))
            {
                // already formatted correctly
                return "+" + digits;
            }

            return digits; // fallback (invalid format)
        }

        private static bool IsValidPhone(string phone)
        {
            return phone.StartsWith("+20") && phone.Length == 13;
        }

    }
}

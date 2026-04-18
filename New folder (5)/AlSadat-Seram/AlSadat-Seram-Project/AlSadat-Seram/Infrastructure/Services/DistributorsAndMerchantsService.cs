using AlSadatSeram.Services.contract;
using Application.DTOs;
using Application.Services.contract;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.Finance;
using Domain.Entities.Transactions;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.UnitOfWork.Contract;
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

        public DistributorsAndMerchantsService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this._userManager = userManager;
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

                if (dto.cityId is null)
                    return Result<string>.Failure("المدينة مطلوبة", HttpStatusCode.BadRequest);


                // 2️⃣ CHECK UNIQUE PHONE
                var userRepo = unitOfWork.GetRepository<ApplicationUser, string>();

                bool exists = await userRepo.GetQueryable()
                                            .AnyAsync(u => u.UserName == dto.phoneNumber);

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
                    Gender = (Gender)dto.gender,
                    CityID = dto.cityId.Value,
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
                var accountsRepo = unitOfWork.GetRepository<ChartOfAccounts, int>();

                var lastAccountCode = accountsRepo
                    .GetQueryable()
                    .Where(a => a.ParentAccountId == 4)
                    .Select(a => a.AccountCode)
                    .OrderByDescending(c => c)
                    .FirstOrDefault();

                string newAccountCode =
                    lastAccountCode == null
                        ? "102101"
                        : (int.Parse(lastAccountCode) + 1).ToString();

                var DisOrMerchAccount = new ChartOfAccounts
                {
                    AccountCode = newAccountCode,
                    UserId = user.Id,
                    AccountName = dto.fullName,
                    ParentAccountId = 4,
                    Type = AccountTypes.Assets,
                    IsLeaf = true,
                    IsActive = true
                };

                await accountsRepo.AddWithoutSaveAsync(DisOrMerchAccount);
                #endregion

              
                // 6️⃣ SAVE BOTH CHANGES
                saved = await unitOfWork.SaveChangesAsync();

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

        //   public async Task<ApiResponse<List<DistributorsAndMerchantsAndAgentsDto>>> GetAllDistributorsAndMerchants(DistributorsAndMerchantsFilters req)
        //   {
        //       var query = unitOfWork
        //.GetRepository<Distributor_Merchant_Agent, string>()
        //.GetQueryable();

        //       // Apply Includes first
        //       query = query
        //           .Include(x => x.User)
        //           .ThenInclude(u => u.City);

        //       // -------------------------------
        //       // Apply Filters
        //       // -------------------------------

        //       if (!string.IsNullOrWhiteSpace(req.fullName))
        //           query = query.Where(x => x.User.FullName.Contains(req.fullName));

        //       if (!string.IsNullOrWhiteSpace(req.phoneNumber))
        //           query = query.Where(x => x.User.PhoneNumber.Contains(req.phoneNumber));

        //       if (!string.IsNullOrWhiteSpace(req.cityName))
        //           query = query.Where(x => x.User.City.Name.Contains(req.cityName));

        //       if (req.type != null)
        //           query = query.Where(x =>(int)x.Type == req.type);
        //       if (req.isDeleted!= null)
        //           query=query.Where(x=>x.User.IsDeleted == req.isDeleted);
        //       // -------------------------------
        //       // Total Count
        //       // -------------------------------
        //       var totalCount = await query.CountAsync();

        //       // -------------------------------
        //       // Pagination
        //       // -------------------------------
        //       int page = req.page <= 0 ? 1 : req.page;
        //       int pageSize = req.pageSize <= 0 ? 10 : req.pageSize;

        //       var skip = (page - 1) * pageSize;

        //       // -------------------------------
        //       // Select DTO
        //       // -------------------------------
        //       var list = await query
        //           .OrderByDescending(x => x.User.CreateAt)
        //           .Skip(skip)
        //           .Take(pageSize)
        //           .Select(x => new DistributorsAndMerchantsAndAgentsDto
        //           {
        //               userId = x.UserId,
        //               fullName = x.User.FullName,
        //               address = x.User.Address,
        //               gender = (int?)x.User.Gender,
        //               type =(int) x.Type,

        //               createdAt = x.User.CreateAt,
        //               createdBy = x.User.CreateBy,
        //               updatedAt = x.User.UpdateAt,
        //               updatedBy = x.User.UpdateBy,
        //               isDelted = x.User.IsDeleted,
        //               deletedAt = x.User.DeleteAt,
        //               deletedBy = x.User.DeleteBy,

        //               cityId = x.User.CityID,
        //               cityName = x.User.City != null ? x.User.City.Name : null,

        //               phoneNumber = x.User.PhoneNumber,
        //               password = null,
        //               firstSpecialDiscount = x.FirstSpecialDiscount,
        //               secondSpecialDiscount = x.SecondSpecialDiscount,
        //               thirdSpecialDiscount = x.ThirdSpecialDiscount,
        //               PointsBalance = x.Balance,
        //               cashBalance = x.CashBalance,
        //               indebtedness = x.Indebtedness
        //           })
        //           .ToListAsync();

        //       // -------------------------------
        //       // Final Response
        //       // -------------------------------
        //       return new ApiResponse<List<DistributorsAndMerchantsAndAgentsDto>>
        //       {
        //           data = list,
        //           totalCount = totalCount,
        //           page = page,
        //           pageSize = pageSize,
        //           totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        //       };
        //   }
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
                    password = null,

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
        public async Task<Result<string>> EditDistributorOrMerchant(DistributorsAndMerchantsAndAgentsDto dto)
        {
            await unitOfWork.BeginTransactionAsync();

            try
            {
                
                if (dto.userId is null)
                    return Result<string>.Failure("المستخدم غير موجود", HttpStatusCode.BadRequest);

                if (string.IsNullOrWhiteSpace(dto.fullName))
                    return Result<string>.Failure("الإسم مطلوب", HttpStatusCode.BadRequest);

                if (string.IsNullOrWhiteSpace(dto.phoneNumber))
                    return Result<string>.Failure("رقم الهاتف مطلوب", HttpStatusCode.BadRequest);

                if (dto.cityId is null)
                    return Result<string>.Failure("المدينة مطلوبة", HttpStatusCode.BadRequest);


                
                var userRepo = unitOfWork.GetRepository<ApplicationUser, string>();
                var distRepo = unitOfWork.GetRepository<Distributor_Merchant_Agent, string>();

                var user = await userRepo.GetQueryable()
                                         .Include(x => x.City)
                                         .FirstOrDefaultAsync(u => u.Id == dto.userId);

                if (user == null)
                {
                    await unitOfWork.RollbackAsync();
                    return Result<string>.Failure("المستخدم غير موجود", HttpStatusCode.NotFound);
                }

                var dist = await distRepo.GetQueryable()
                                         .FirstOrDefaultAsync(x => x.UserId == user.Id);

                if (dist == null)
                {
                    await unitOfWork.RollbackAsync();
                    return Result<string>.Failure("البيانات غير موجودة", HttpStatusCode.NotFound);
                }


                
                bool phoneExists = userRepo.GetQueryable()
                                           .Any(x => x.UserName == dto.phoneNumber && x.Id != user.Id);

                if (phoneExists)
                {
                    await unitOfWork.RollbackAsync();
                    return Result<string>.Failure("رقم الهاتف مستخدم بالفعل من مستخدم آخر", HttpStatusCode.Conflict);
                }


               
                user.FullName = dto.fullName??user.FullName;
                user.CityID = dto.cityId??user.CityID;
                user.PhoneNumber = dto.phoneNumber??user.PhoneNumber;
                user.Address=dto.address??user.Address;
                user.UserName = dto.phoneNumber ?? user.PhoneNumber;
                user.Email = dto.phoneNumber ?? user.PhoneNumber;
                user.Gender = (Gender)dto.gender;
                user.UpdateAt = dto.updatedAt??user.UpdateAt;
                user.UpdateBy=dto.updatedBy??user.UpdateBy;
                user.DeleteAt = dto.deletedAt??user.DeleteAt;
                user.DeleteBy=dto.deletedBy??user.DeleteBy;
                user.NormalizedUserName = dto.phoneNumber ?? user.PhoneNumber.ToUpper();
                user.NormalizedEmail = dto.phoneNumber ?? user.PhoneNumber.ToUpper();
                dist.Type = (DistributorOrMerchantOrAgent)dto.type;
                dist.CashBalance = dto.cashBalance??dist.CashBalance;
                dist.Balance = dto.PointsBalance??dist.Balance; 
                dist.Indebtedness=dto.indebtedness??dist.Indebtedness;
                user.IsDeleted=dto.isDelted??user.IsDeleted;
                dist.FirstSpecialDiscount = dto.firstSpecialDiscount;
                dist.SecondSpecialDiscount = dto.secondSpecialDiscount;
                dist.ThirdSpecialDiscount = dto.thirdSpecialDiscount;
                int saved = await unitOfWork.SaveChangesAsync();

                if (saved > 0)
                {
                    await unitOfWork.CommitAsync();
                    return Result<string>.Success("تم التعديل بنجاح", HttpStatusCode.OK);
                }
                else
                {
                    await unitOfWork.RollbackAsync();
                    return Result<string>.Failure("لم يتم حفظ التعديلات", HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();

                return Result<string>.Failure("حدث خطأ أثناء تعديل البيانات", HttpStatusCode.InternalServerError);
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

    }
}

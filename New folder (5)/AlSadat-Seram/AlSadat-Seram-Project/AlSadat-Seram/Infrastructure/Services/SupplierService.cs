using AlSadatSeram.Services.contract;
using Application.DTOs;
using Application.Services.contract;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.Finance;
using Domain.Enums;
using Domain.UnitOfWork.Contract;
using Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class SupplierService : ISupplierContract
    {
        private readonly IUnitOfWork unitOfWork;

        public SupplierService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> AddNewSupllier(SupplierDtos.SupplierDto dto)
        {
            await unitOfWork.BeginTransactionAsync();

            try
            {
                #region 1️⃣ Add Supplier
                var supplier = new Supplier
                {
                    Name = dto.name,
                    phoneNumbers = dto.phoneNumbers,
                    address = dto.address,
                    cityId = dto.cityId,
                    IsDeleted = false
                };

                await unitOfWork
                    .GetRepository<Supplier, int>()
                    .AddWithoutSaveAsync(supplier);

                await unitOfWork.SaveChangesAsync(); 
                #endregion

                #region 2️⃣ Add Supplier Products
                if (dto.products != null && dto.products.Any())
                {
                    var supplierProducts = dto.products.Select(p => new SupplierProducts
                    {
                        SupplierId = supplier.Id,
                        ProductId = p.productId
                    });

                    await unitOfWork
                        .GetRepository<SupplierProducts, int>()
                        .AddRangeAsync(supplierProducts);
                }
                #endregion

                //#region 3️⃣ Create Account for Supplier
                //var accountsRepo = unitOfWork.GetRepository<ChartOfAccounts, int>();

                //var lastAccountCode = accountsRepo
                //    .GetQueryable()
                //    .Where(a => a.ParentAccountId == 8)
                //    .Select(a => a.AccountCode)
                //    .OrderByDescending(c => c)
                //    .FirstOrDefault();

                //string newAccountCode =
                //    lastAccountCode == null
                //        ? "20101"
                //        : (int.Parse(lastAccountCode) + 1).ToString();

                //var supplierAccount = new ChartOfAccounts
                //{
                //    AccountCode = newAccountCode,
                //    UserId= supplier.Id.ToString(),
                //    AccountName = supplier.Name,
                //    ParentAccountId = 8,
                //    Type = AccountTypes.Liabilities,
                //    IsLeaf = true,
                //    IsActive = true
                //};

                //await accountsRepo.AddWithoutSaveAsync(supplierAccount);
                //#endregion
                #region 3️⃣ Create Account for Supplier

                var accountsRepo = unitOfWork.GetRepository<ChartOfAccounts, int>();

                int parentId = 10;

                var lastAccountCode = accountsRepo
                    .GetQueryable()
                    .Where(a => a.ParentAccountId == parentId)
                    .Select(a => a.AccountCode)
                    .OrderByDescending(c => c)
                    .FirstOrDefault();

                string newAccountCode;

                if (lastAccountCode == null)
                {
                    newAccountCode = "2.1.1";
                }
                else
                {
                    var parts = lastAccountCode.Split('.');
                    int lastNumber = int.Parse(parts.Last());
                    parts[parts.Length - 1] = (lastNumber + 1).ToString();

                    newAccountCode = string.Join(".", parts);
                }

                var supplierAccount = new ChartOfAccounts
                {
                    AccountCode = newAccountCode,
                    UserId = supplier.Id.ToString(),
                    AccountName = supplier.Name,
                    ParentAccountId = parentId,
                    Type = AccountTypes.Liabilities,
                    IsLeaf = true,
                    IsActive = true
                };

                await accountsRepo.AddWithoutSaveAsync(supplierAccount);

                #endregion

                #region 4️⃣ Commit
                await unitOfWork.SaveChangesAsync();
                await unitOfWork.CommitAsync();
                #endregion

                return Result<string>.Success("Supplier added successfully");
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                await unitOfWork.LogError(ex);

                return Result<string>.Failure("Failed to add supplier");
            }
        }


        public async Task<Result<string>> EditSupplier(SupplierDtos.SupplierDto dto)
        {
            if (dto.id == null)
                return Result<string>.Failure("Supplier Id is required");

            await unitOfWork.BeginTransactionAsync();

            try
            {
                // 1️⃣ Get Supplier
                var supplierRepo = unitOfWork.GetRepository<Supplier, int>();
                var supplier = await supplierRepo
                    .GetQueryable()
                    .Include(s => s.SupplierProducts)
                    .FirstOrDefaultAsync(s => s.Id == dto.id.Value);

                if (supplier == null)
                    return Result<string>.Failure("المورد غير موجود");

              
                supplier.Name = dto.name;
                supplier.phoneNumbers = dto.phoneNumbers;
                supplier.address = dto.address;
                supplier.cityId = dto.cityId;
                supplier.IsDeleted = dto.isDeleted;

                supplierRepo.UpdateWithoutSaveAsync(supplier);

                
                if (dto.products != null)
                {
                    var supplierProductsRepo =
                        unitOfWork.GetRepository<SupplierProducts, int>();

                  
                    foreach (var item in supplier.SupplierProducts.ToList())
                        supplierProductsRepo.DeleteWithoutSaveAsync(item);

                    // add new
                    var newProducts = dto.products.Select(p => new SupplierProducts
                    {
                        SupplierId = supplier.Id,
                        ProductId = p.productId
                    });

                    await supplierProductsRepo.AddRangeAsync(newProducts);
                }

                // 4️⃣ Sync Supplier Account Status (IMPORTANT PART 🔥)
                var chartRepo =
                    unitOfWork.GetRepository<ChartOfAccounts, int>();

                var supplierAccount = await chartRepo.FindAsync(a =>
                    a.UserId == supplier.Id.ToString() &&
                    a.Type == AccountTypes.Liabilities);

                if (supplierAccount != null)
                {
                    supplierAccount.IsActive = !dto.isDeleted;
                    chartRepo.UpdateWithoutSaveAsync(supplierAccount);
                }

                // 5️⃣ Save + Commit
                await unitOfWork.SaveChangesAsync();
                await unitOfWork.CommitAsync();

                return Result<string>.Success("تم التحديث بنجاح ");
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                await unitOfWork.LogError(ex);

                return Result<string>.Failure("حدث مشكلة اثناء التحديث حاول المحاولة لاحقا ");
            }
        }


        public async Task<ApiResponse<List<Application.DTOs.SupplierDtos.SupplierDto>>> GetAllSuppliers(SupplierDtos.SupplierFilteration req)
        {
            IQueryable<Supplier> query = unitOfWork.GetRepository<Supplier, int>()
                                                   .GetQueryable()
                                                   .Include(s => s.city)
                                                  .AsQueryable();
            if (req.isDeleted != null) 
            {
                query = query.Where(s => s.IsDeleted==req.isDeleted);
            }
            // فلترة حسب الاسم
            if (!string.IsNullOrWhiteSpace(req.name))
            {
                query = query.Where(s => s.Name.Contains(req.name.ToLower()));
            }

            // فلترة حسب رقم الهاتف
            if (!string.IsNullOrWhiteSpace(req.phoneNumbers))
            {
                query = query.Where(s => s.phoneNumbers.Contains(req.phoneNumbers));
            }

            var totalCount = await query.CountAsync();

            List<SupplierDtos.SupplierDto> suppliers;

            int page = req.page ?? 1;
            int pageSize = req.pageSize ?? 0;
            int totalPages = 1;

            if (req.page.HasValue && req.pageSize.HasValue)
            {
                suppliers = await query
                    .OrderBy(s => s.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new SupplierDtos.SupplierDto
                    {
                        id = s.Id,
                        name = s.Name,
                        phoneNumbers = s.phoneNumbers,
                        address = s.address,
                        cityId = s.cityId,
                        cityName = s.city.Name,
                        isDeleted = s.IsDeleted,
                        products = null // null as you requested
                    })
                    .ToListAsync();

                totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            }
            else
            {
                suppliers = await query
                    .OrderBy(s => s.Id)
                    .Select(s => new SupplierDtos.SupplierDto
                    {
                        id = s.Id,
                        name = s.Name,
                        phoneNumbers = s.phoneNumbers,
                        address = s.address,
                        cityId = s.cityId,
                        cityName = s.city.Name,
                        isDeleted = s.IsDeleted,
                        products = null
                    })
                    .ToListAsync();

                page = 1;
                pageSize = totalCount;
                totalPages = 1;
            }

            return new ApiResponse<List<SupplierDtos.SupplierDto>>
            {
                totalCount = totalCount,
                page = page,
                pageSize = pageSize,
                totalPages = totalPages,
                data = suppliers
            };
        }

        public async Task<Result<SupplierDtos.SupplierDto>> GetById(int id)
        {
            try
            {
                var supplier = await unitOfWork.GetRepository<Supplier, int>()
                    .GetQueryable()
                    .Include(s => s.city)
                    .Include(s => s.SupplierProducts)  
                        .ThenInclude(sp => sp.Product)
                    .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

                if (supplier == null)
                    return Result<SupplierDtos.SupplierDto>.Failure("المورد غير موجود ");

                var supplierDto = new SupplierDtos.SupplierDto
                {
                    id = supplier.Id,
                    name = supplier.Name,
                    phoneNumbers = supplier.phoneNumbers,
                    address = supplier.address,
                    cityId = supplier.cityId,
                    cityName = supplier.city.Name,
                    isDeleted = supplier.IsDeleted,
                    products = supplier.SupplierProducts?.Select(sp => new SupplierDtos.ProductsForSupplierDto
                    {
                        productId = sp.ProductId,
                        productName = sp.Product.Name
                    }).ToList()
                };

                return Result<SupplierDtos.SupplierDto>.Success(supplierDto);
            }
            catch (Exception ex)
            {
                return Result<SupplierDtos.SupplierDto>.Failure("خطأ في الاتصال بقاعدة البيانات: " + ex.Message);
            }
        }

    }
}

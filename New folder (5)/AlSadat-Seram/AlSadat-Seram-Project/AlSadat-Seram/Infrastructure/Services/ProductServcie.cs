using AlSadatSeram.Services.contract;
using Application.DTOs;
using Application.DTOs.ProductsDtos;
using Application.Services.contract;
using DocumentFormat.OpenXml.InkML;
using Domain.Common;
using Domain.Entities;
using Domain.UnitOfWork.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ProductServcie : IProductService
    {
        private readonly IUnitOfWork unitOfWork;

        public ProductServcie(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        private ExcelReaderService _excelService= new ExcelReaderService();
        public async Task AddNewProduct(ProductDto product)
        {
            var AddedProduct = FromDto(product);


            await unitOfWork.GetRepository<Products, int>().AddAsync(AddedProduct);
        }

        public async Task EditProduct(ProductDto dto)
        {
            var UpdatedProduct= await unitOfWork.GetRepository<Products, int>().FindAsync(c => c.Id == dto.id);
            UpdatedProduct.Name = dto.name;
            UpdatedProduct.SellingPrice = dto.sellingPrice;
            UpdatedProduct.PointPerUnit = dto.pointPerUnit;
            UpdatedProduct.UpdateBy = dto.updateBy;
            UpdatedProduct.UpdateAt = dto.updateAt;
            UpdatedProduct.IsDeleted = dto.isDeleted;
            UpdatedProduct.productCode = dto.productCode;
            UpdatedProduct.DeleteBy = dto.deleteBy;
            UpdatedProduct.DeleteAt = dto.deleteAt;
           
          
            UpdatedProduct.TheSmallestPossibleQuantity = dto.theSmallestPossibleQuantity;
            

            await unitOfWork.SaveChangesAsync();
        }

        public async Task<ApiResponse<List<ProductDto>>> GetAllProducts(ProductFilterationDto req)
        {
            IQueryable<Products> query = unitOfWork.GetRepository<Products, int>().GetQueryable();


            if (req.isDeleted != null)
            {
                query = query.Where(p => p.IsDeleted == req.isDeleted );
            }

            if (!string.IsNullOrEmpty(req.name))
            {
                query = query.Where(p => p.Name.ToLower().Contains(req.name.ToLower()));
            }

         

            var totalCount = await query.CountAsync();

            // Only apply pagination if both page and pageSize are provided
            List<ProductDto> result;
            int page = req.page ?? 0;
            int pageSize = req.pageSize ?? 0;

            if (page > 0 && pageSize > 0)
            {
                result = await query
                    .OrderBy(p => p.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new ProductDto
                    {
                        id = p.Id,
                        productCode = p.productCode,
                        name = p.Name,
                        sellingPrice = p.SellingPrice,
                        pointPerUnit = p.PointPerUnit,
                        createBy = p.CreateBy,
                        createAt = p.CreateAt,
                        updateBy = p.UpdateBy,
                        updateAt = p.UpdateAt,
                        isDeleted = p.IsDeleted,
                        deleteBy = p.DeleteBy,
                        deleteAt = p.DeleteAt,
                     
                       
                       
                        theSmallestPossibleQuantity = p.TheSmallestPossibleQuantity
                    })
                    .ToListAsync();
            }
            else
            {
                // return all products
                result = await query
                    .OrderBy(p => p.Id)
                    .Select(p => new ProductDto
                    {
                        id = p.Id,
                        name = p.Name,
                        sellingPrice = p.SellingPrice,

                        pointPerUnit = p.PointPerUnit,
                        createBy = p.CreateBy,
                        productCode=p.productCode,
                        createAt = p.CreateAt,
                        updateBy = p.UpdateBy,
                        updateAt = p.UpdateAt,
                        isDeleted = p.IsDeleted,
                        deleteBy = p.DeleteBy,
                        deleteAt = p.DeleteAt,
                       
                       
                      
                        theSmallestPossibleQuantity = p.TheSmallestPossibleQuantity
                    })
                    .ToListAsync();

                // Set default paging info
                page = 1;
                pageSize = result.Count;
            }

            var response = new ApiResponse<List<ProductDto>>
            {
                totalCount = totalCount,
                page = page,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)(pageSize > 0 ? pageSize : 1)),
                data = result
            };

            return response;
        }

        public async Task<ProductDto> GetByName(string name)
        {
            var entity = await unitOfWork.GetRepository<Products, int>().GetQueryable().FirstOrDefaultAsync(p=>p.Name==name);
            if (entity == null) { return null; }
            var res = ToDto(entity);
            return res;
        }
        public async Task<ProductDto> GetByProductCode(string productCode)
        {
            if (string.IsNullOrWhiteSpace(productCode))
                return null;

            var entity = await unitOfWork
                .GetRepository<Products, int>()
                .GetQueryable()
                
                .FirstOrDefaultAsync(p =>
                    p.productCode == productCode &&
                    p.IsDeleted == false 
                   
                );

            if (entity == null)
                return null;

            return ToDto(entity);
        }
        public async Task<bool> IsProductCodeExists(string productCode, int? excludeProductId = null)
        {
            if (string.IsNullOrWhiteSpace(productCode))
                return false;

            var query = unitOfWork
                .GetRepository<Products, int>()
                .GetQueryable()
                .Where(p => p.productCode == productCode && !p.IsDeleted);

            if (excludeProductId.HasValue)
            {
                query = query.Where(p => p.Id != excludeProductId.Value);
            }

            return await query.AnyAsync();
        }

        public Products FromDto(ProductDto dto)
        {
            if (dto == null) return new Products();

            return new Products
            {
                Name = dto.name ?? string.Empty,
                SellingPrice = dto.sellingPrice ,
                productCode=dto.productCode,
                PointPerUnit = dto.pointPerUnit ,
                CreateBy = dto.createBy,
                CreateAt = dto.createAt ,
                UpdateBy = dto.updateBy,
                UpdateAt = dto.updateAt,
                IsDeleted = dto.isDeleted ,
                DeleteBy = dto.deleteBy,
                DeleteAt = dto.deleteAt,
                
             
                TheSmallestPossibleQuantity=dto.theSmallestPossibleQuantity
            };
        }
        public ProductDto ToDto(Products p)
        {
            return new ProductDto
            {
                id = p.Id,
                name = p.Name,
                sellingPrice = p.SellingPrice,
                productCode=p.productCode,
                pointPerUnit = p.PointPerUnit,
                createBy = p.CreateBy,
                createAt = p.CreateAt,
                updateBy = p.UpdateBy,
                updateAt = p.UpdateAt,
                isDeleted = p.IsDeleted,
                deleteBy = p.DeleteBy,
                deleteAt = p.DeleteAt,
              
                theSmallestPossibleQuantity=p.TheSmallestPossibleQuantity
            };
        }

        public async Task<List<Products>> GetAsync(string name)
        {
            var CheckProducts = await unitOfWork.GetRepository<Products, int>().GetAsync(x => x.Name == name);
            return CheckProducts.ToList();
        }

        //public async Task<ExcelReaderDtos.ExcelReadResult<ExcelProductDto>> BulkAddFromExcel(Stream fileStream)
        //{
        //    try
        //    {
        //        var res = _excelService.Read<ExcelProductDto>(fileStream);

        //        if (res.Data == null || res.Data.Count <= 0)
        //            return null;
        //        foreach(var item in res.Data)
        //        {
        //            var isExist =await unitOfWork.GetRepository<Products, int>().AnyAsync(p => p.productCode == item.productCode || p.Name == item.productName);
        //            if (isExist) 
        //            {
        //                res.Errors.Add(new ExcelReaderDtos.ExcelError
        //                {
        //                    Column = item.productName + item.productCode,
        //                    Message = "هذا المنتج موجود بالفعل في قاعدة البيانات "
        //                });
        //            }
        //         await unitOfWork.GetRepository<Products, int>().AddWithoutSaveAsync(new Products()
        //            {
        //                Name = item.productName,
        //                productCode = item.productCode,
        //                CreateAt = DateTime.Now,
        //                CreateBy = "Admin",
        //                IsDeleted = false,
        //                SellingPrice = item.sellingPrice,
        //                PointPerUnit = (int)item.pointsPerUnit,
        //                TheSmallestPossibleQuantity = (int)item.minQuantity
        //            });
        //            try
        //            {
        //                await unitOfWork.SaveChangesAsync();
        //            }
        //            catch (DbUpdateException ex) 
        //            {

        //            }
        //        }



        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }

        //}
        //public async Task<ExcelReaderDtos.ExcelReadResult<ExcelProductDto>> BulkAddFromExcel(Stream fileStream, string c)
        //{
        //    var res = _excelService.Read<ExcelProductDto>(fileStream);
        //    if (res.Data == null || res.Data.Count == 0)
        //        return res;

        //    var failedList = new List<ExcelReaderDtos.ExcelError>();

        //    foreach (var item in res.Data)
        //    {
        //        try
        //        {



        //            var newProduct = new Products
        //            {
        //                Name = item.productName,
        //                productCode = item.productCode,
        //                SellingPrice = item.sellingPrice,
        //                PointPerUnit = (int)item.pointsPerUnit,
        //                TheSmallestPossibleQuantity = (int)item.minQuantity,
        //                CreateAt = DateTime.Now,
        //                CreateBy = c,
        //                IsDeleted = false
        //            };

        //            await unitOfWork.GetRepository<Products, int>().AddWithoutSaveAsync(newProduct);


        //            try
        //            {
        //                await unitOfWork.SaveChangesAsync();
        //            }
        //            catch (DbUpdateException dbEx)
        //            {

        //                unitOfWork.GetRepository<Products, int>()
        //                    .Detach(newProduct);

        //                failedList.Add(new ExcelReaderDtos.ExcelError
        //                {
        //                    Column = $"{item.productName} | {item.productCode}",
        //                    Message = dbEx.InnerException?.Message ?? dbEx.Message
        //                });
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            failedList.Add(new ExcelReaderDtos.ExcelError
        //            {
        //                Column = $"{item.productName} | {item.productCode}",
        //                Message = ex.Message
        //            });
        //        }
        //    }

        //    res.Errors.AddRange(failedList);
        //    return res;
        //}
        public async Task<ExcelReaderDtos.ExcelReadResult<ExcelProductDto>> BulkAddFromExcel(Stream fileStream, string createdBy)
        {
            var res = _excelService.Read<ExcelProductDto>(fileStream);
            if (res.data == null || res.data.Count == 0)
                return res;

            var failedList = new List<ExcelReaderDtos.ExcelError>();

            foreach (var item in res.data)
            {
                try
                {
                    var newProduct = new Products
                    {
                        Name = item.productName,
                        productCode = item.productCode,
                        SellingPrice = item.sellingPrice,
                        PointPerUnit = (int)item.pointsPerUnit,
                        TheSmallestPossibleQuantity = (int)item.minQuantity,
                        CreateAt = DateTime.Now,
                        CreateBy = createdBy,
                        IsDeleted = false
                    };

                    await unitOfWork.GetRepository<Products, int>().AddWithoutSaveAsync(newProduct);

                    try
                    {
                        await unitOfWork.SaveChangesAsync();
                    }
                    catch (DbUpdateException dbEx)
                    {
                        unitOfWork.GetRepository<Products, int>()
                            .Detach(newProduct);

                        string message = "حدث خطأ أثناء حفظ المنتج";

                        var sqlMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                        if (sqlMessage.Contains("duplicate key") ||
                            sqlMessage.Contains("UNIQUE"))
                        {
                            message = "هذا المنتج موجود بالفعل في قاعدة البيانات";
                        }

                        failedList.Add(new ExcelReaderDtos.ExcelError
                        {
                            Column = $"{item.productName} | {item.productCode}",
                            Message = message
                        });
                    }
                }
                catch (Exception ex)
                {
                    failedList.Add(new ExcelReaderDtos.ExcelError
                    {
                        Column = $"{item.productName} | {item.productCode}",
                        Message = ex.Message
                    });
                }
            }

            // إضافة كل الأخطاء إلى res.Errors
            res.errors.AddRange(failedList);

            // إذا كل المنتجات فشلت، نفرغ res.Data
            if (failedList.Count > 0 && failedList.Count >= res.data.Count)
            {
                res.data.Clear();
            }

            return res;
        }
    }
}

using AlSadatSeram.Services.contract;
using Application.DTOs;
using Application.DTOs.ProductsDtos;
using Application.Services.contract;
using Application.Services.contract.CurrentUserService;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.Users;
using Domain.UnitOfWork.Contract;
using Infrastructure.Services.CurrentUserServices;
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
        private readonly ICurrentUserService currentUserService;
        private readonly IExcelReaderService excelService;

        public ProductServcie(IUnitOfWork unitOfWork, ICurrentUserService currentUserService,
        IExcelReaderService excelService)
        {
            this.unitOfWork = unitOfWork;
            this.currentUserService = currentUserService;
            this.excelService = excelService;
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

        public async Task<Result<byte[]>> ExportProductsTemplateAsync(CancellationToken ct)
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                using var workbook = new XLWorkbook();

                var ws = workbook.Worksheets.Add("Products");

                ws.Cell(1, 1).Value = "اسم_المنتج";
                ws.Cell(1, 2).Value = "كود_المنتج";
                ws.Cell(1, 3).Value = "سعر_البيع";
                ws.Cell(1, 4).Value = "عدد_النقاط";
                ws.Cell(1, 5).Value = "اقل_كمية";

                // Header style
                var header = ws.Range(1, 1, 1, 5);
                header.Style.Font.Bold = true;
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                header.Style.Fill.BackgroundColor = XLColor.FromHtml("#D4AF37");
                header.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                header.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Example row
                ws.Cell(2, 1).Value = "منتج تجريبي";
                ws.Cell(2, 2).Value = "PRD-001";
                ws.Cell(2, 3).Value = 100;
                ws.Cell(2, 4).Value = 10;
                ws.Cell(2, 5).Value = 1;

                ws.Range(2, 1, 2, 5).Style.Font.Italic = true;
                ws.Range(2, 1, 2, 5).Style.Font.FontColor = XLColor.Gray;

                ws.SheetView.FreezeRows(1);
                ws.Columns().AdjustToContents();

                // Instructions
                var help = workbook.Worksheets.Add("Instructions");

                help.Cell(1, 1).Value = "تعليمات الاستيراد";
                help.Cell(1, 1).Style.Font.Bold = true;

                help.Cell(3, 1).Value = "الأعمدة:";
                help.Cell(4, 1).Value = "اسم المنتج - كود المنتج - سعر البيع - عدد النقاط - اقل كمية";

                help.Cell(6, 1).Value = "ملاحظات:";
                help.Cell(7, 1).Value = "اسم المنتج وكود المنتج يجب أن يكونوا غير مكررين";

                help.Column(1).Width = 80;

                using var ms = new MemoryStream();
                workbook.SaveAs(ms);

                return Result<byte[]>.Success(ms.ToArray(), "تم إنشاء القالب بنجاح");
            }
            catch (Exception ex)
            {
                await unitOfWork.LogError(ex);
                return Result<byte[]>.Failure("حدث خطأ", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result<ExcelImportResult<ProductDto>>>ImportProductsFromExcelAsync(Stream fileStream, CancellationToken ct)
        {
            try
            {
                var currentUserId = currentUserService.UserId;

                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Result<ExcelImportResult<ProductDto>>
                        .Failure("المستخدم الحالي غير معروف", HttpStatusCode.Unauthorized);

                // =========================================
                // Repositories
                // =========================================
                var productRepo = unitOfWork.GetRepository<Products, int>();
                var userRepo = unitOfWork.GetRepository<ApplicationUser, string>();

                var currentUser = await userRepo.FindAsync(u => u.Id == currentUserId);

                // =========================================
                // Load existing products (DB)
                // =========================================
                var existingProducts = await productRepo
                    .GetQueryable()
                    .Where(x => !x.IsDeleted)
                    .Select(x => new { x.Name, x.productCode })
                    .ToListAsync(ct);

                var dbNames = existingProducts
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                    .Select(x => Normalize(x.Name))
                    .ToHashSet();

                var dbCodes = existingProducts
                    .Where(x => !string.IsNullOrWhiteSpace(x.productCode))
                    .Select(x => Normalize(x.productCode))
                    .ToHashSet();

                // =========================================
                // Track duplicates inside file
                // =========================================
                var fileNames = new HashSet<string>();
                var fileCodes = new HashSet<string>();

                // =========================================
                // Import
                // =========================================
                var importResult =
                    await excelService.ImportAsync<
                        ProductExcelDto,
                        ProductDto>(
                    fileStream,

                    async (row, ctx) =>
                    {
                        ct.ThrowIfCancellationRequested();

                        // =========================
                        // Name
                        // =========================
                        var name = row.اسم_المنتج?.Trim();

                        if (string.IsNullOrWhiteSpace(name))
                            return RowImportResult<ProductDto>
                                .Fail("اسم المنتج مطلوب", "اسم المنتج");

                        if (name.Length > 200)
                            return RowImportResult<ProductDto>
                                .Fail("اسم المنتج تجاوز الحد المسموح", "اسم المنتج");

                        var normalizedName = Normalize(name);

                        if (!fileNames.Add(normalizedName))
                            return RowImportResult<ProductDto>
                                .Fail("اسم المنتج مكرر داخل الملف", "اسم المنتج");

                        if (dbNames.Contains(normalizedName))
                            return RowImportResult<ProductDto>
                                .Fail("اسم المنتج موجود بالفعل", "اسم المنتج");


                        // =========================
                        // Code
                        // =========================
                        var code = row.كود_المنتج?.Trim();

                        if (string.IsNullOrWhiteSpace(code))
                            return RowImportResult<ProductDto>
                                .Fail("كود المنتج مطلوب", "كود المنتج");

                        if (code.Length > 100)
                            return RowImportResult<ProductDto>
                                .Fail("كود المنتج تجاوز الحد المسموح", "كود المنتج");

                        var normalizedCode = Normalize(code);

                        if (!fileCodes.Add(normalizedCode))
                            return RowImportResult<ProductDto>
                                .Fail("كود المنتج مكرر داخل الملف", "كود المنتج");

                        if (dbCodes.Contains(normalizedCode))
                            return RowImportResult<ProductDto>
                                .Fail("كود المنتج مستخدم بالفعل", "كود المنتج");


                        // =========================
                        // Price
                        // =========================
                        if (row.سعر_البيع <= 0)
                            return RowImportResult<ProductDto>
                                .Fail("سعر البيع يجب أن يكون أكبر من صفر", "سعر البيع");


                        // =========================
                        // Points
                        // =========================
                        if (row.عدد_النقاط < 0)
                            return RowImportResult<ProductDto>
                                .Fail("عدد النقاط غير صحيح", "عدد النقاط");


                        // =========================
                        // Min Quantity
                        // =========================
                        if (row.اقل_كمية <= 0)
                            return RowImportResult<ProductDto>
                                .Fail("اقل كمية يجب أن تكون أكبر من صفر", "اقل كمية");


                        // =========================
                        // Mapping
                        // =========================
                        ProductDto dto = new ProductDto
                        {
                            name = name,
                            productCode = code,
                            sellingPrice = row.سعر_البيع,
                            pointPerUnit = row.عدد_النقاط,
                            theSmallestPossibleQuantity = row.اقل_كمية,
                            createAt = DateTime.Now,
                            createBy = $"{currentUser.FullName} / {currentUser.Email}"
                        };

                        return RowImportResult<ProductDto>.Success(dto);
                    },

                    ct);

                // =========================================
                // Save to DB
                // =========================================
                var saveErrors = new List<string>();

                foreach (var item in importResult.Imported)
                {
                    ct.ThrowIfCancellationRequested();
                    var AddedProduct = FromDto(item);
                    var result = await unitOfWork.GetRepository<Products, int>().AddAsync(AddedProduct);

                    if (result==null)
                    {

                        saveErrors.Add($"{item.name}: حدث مشكلة اثناء حفظ المنتج");
                    }
                }

                if (saveErrors.Any())
                {
                    return Result<ExcelImportResult<ProductDto>>
                        .Failure(string.Join(" | ", saveErrors), HttpStatusCode.BadRequest);
                }

                return Result<ExcelImportResult<ProductDto>>
                    .Success(importResult, "تم استيراد المنتجات بنجاح");
            }
            catch (OperationCanceledException)
            {
                return Result<ExcelImportResult<ProductDto>>
                    .Failure("تم إلغاء العملية", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                await unitOfWork.LogError(ex);

                return Result<ExcelImportResult<ProductDto>>
                    .Failure("حدث خطأ أثناء الاستيراد", HttpStatusCode.InternalServerError);
            }
        }


        private string Normalize(string value)
        {
            return value.Trim().ToLowerInvariant();
        }
    }
}

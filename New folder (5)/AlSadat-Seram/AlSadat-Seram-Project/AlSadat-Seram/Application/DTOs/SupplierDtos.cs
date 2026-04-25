using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class SupplierDtos
    {
        // -------------------------------------------------------------
        // Core Supplier DTO (used by Add, Edit, GetById)
        // -------------------------------------------------------------
        public class SupplierDto
        {
            public int? id { get; set; }

            [Required(ErrorMessage = "اسم المورد مطلوب")]
            [MaxLength(200, ErrorMessage = "اسم المورد لا يمكن أن يتجاوز 200 حرف")]
            public string name { get; set; } = string.Empty;

            [Required(ErrorMessage = "رقم الهاتف مطلوب")]
            [MaxLength(50, ErrorMessage = "رقم الهاتف لا يمكن أن يتجاوز 50 حرف")]
            [RegularExpression(@"^\+[1-9]\d{6,14}$",
                ErrorMessage = "رقم الهاتف غير صالح — يجب أن يكون بصيغة دولية مثل +201012345678")]
            public string phoneNumbers { get; set; } = string.Empty;

            [MaxLength(500, ErrorMessage = "العنوان لا يمكن أن يتجاوز 500 حرف")]
            public string? address { get; set; }

            // cityId is OPTIONAL — suppliers can be saved without a city,
            // and Excel imports always leave it null.
            public int? cityId { get; set; }

            // Read-only fields populated by the server on responses
            public string? cityName { get; set; }
            public bool isDeleted { get; set; }
        }

        // -------------------------------------------------------------
        // Filter for the paginated list endpoint
        // -------------------------------------------------------------
        public class SupplierFilteration
        {
            public string? name { get; set; }
            public string? phoneNumbers { get; set; }
            public bool? isDeleted { get; set; }
            public int? page { get; set; }
            public int? pageSize { get; set; }
        }

        // -------------------------------------------------------------
        // Lightweight DTO for select boxes — only active suppliers
        // -------------------------------------------------------------
        public class SupplierLookupDto
        {
            public int id { get; set; }
            public string name { get; set; } = string.Empty;
        }

        public class SupplierLookupFilter
        {
            public string? name { get; set; }
        }

        // -------------------------------------------------------------
        // Excel import — minimal shape per business decision (Name + Phone only)
        // Property names = Excel header names (ExcelReaderService.Read<T> is reflection-based)
        // -------------------------------------------------------------
        public class ExcelSupplierDto
        {
            [Required(ErrorMessage = "اسم المورد مطلوب")]
            [MaxLength(200, ErrorMessage = "اسم المورد لا يمكن أن يتجاوز 200 حرف")]
            public string Name { get; set; } = string.Empty;

            // NOTE: no regex / max length at DTO level for phone — the import service
            // runs its own NormalizeImportedPhone + IsValidE164 pipeline because Excel
            // strips the leading '+'. See SupplierService.ImportFromExcelAsync.
            [Required(ErrorMessage = "رقم الهاتف مطلوب")]
            [MaxLength(50, ErrorMessage = "رقم الهاتف لا يمكن أن يتجاوز 50 حرف")]
            public string PhoneNumbers { get; set; } = string.Empty;
        }

        // -------------------------------------------------------------
        // Per-row error reporting for partial-success imports
        // -------------------------------------------------------------
        public class SupplierImportRowError
        {
            public int rowNumber { get; set; }
            public string? supplierName { get; set; }
            public string column { get; set; } = string.Empty;
            public string message { get; set; } = string.Empty;
        }

        public class SupplierImportResultDto
        {
            public int totalRows { get; set; }
            public int successCount { get; set; }
            public int failedCount { get; set; }
            public List<SupplierDto> imported { get; set; } = new();
            public List<SupplierImportRowError> errors { get; set; } = new();
        }
    }
}
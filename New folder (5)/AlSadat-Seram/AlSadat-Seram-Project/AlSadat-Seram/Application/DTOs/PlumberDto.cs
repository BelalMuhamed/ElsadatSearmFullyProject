using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    /// <summary>
    /// DTO bundle for plumber management. Follows the same nested-class layout as
    /// <see cref="SupplierDtos"/> for consistency across the codebase.
    /// </summary>
    public class PlumberDtos
    {
        // -------------------------------------------------------------
        // Core Plumber DTO (used by Add, Edit, GetById)
        // -------------------------------------------------------------
        public class PlumberDto
        {
            public int? id { get; set; }

            [Required(ErrorMessage = "اسم السباك مطلوب")]
            [MaxLength(200, ErrorMessage = "اسم السباك لا يمكن أن يتجاوز 200 حرف")]
            public string name { get; set; } = string.Empty;

            [Required(ErrorMessage = "رقم الهاتف مطلوب")]
            [MaxLength(50, ErrorMessage = "رقم الهاتف لا يمكن أن يتجاوز 50 حرف")]
            [RegularExpression(@"^\+[1-9]\d{6,14}$",
                ErrorMessage = "رقم الهاتف غير صالح — يجب أن يكون بصيغة دولية مثل +201012345678")]
            public string phoneNumbers { get; set; } = string.Empty;

            [MaxLength(500, ErrorMessage = "العنوان لا يمكن أن يتجاوز 500 حرف")]
            public string? address { get; set; }

            // Optional — plumbers can be saved without a city.
            public int? cityId { get; set; }

            [MaxLength(50, ErrorMessage = "رقم الرخصة لا يمكن أن يتجاوز 50 حرف")]
            public string? licenseNumber { get; set; }

            [MaxLength(100, ErrorMessage = "التخصص لا يمكن أن يتجاوز 100 حرف")]
            public string? specialty { get; set; }

            // Read-only fields populated by the server on responses
            public string? cityName { get; set; }
            public bool isDeleted { get; set; }
        }

        // -------------------------------------------------------------
        // Filter for the paginated list endpoint
        // -------------------------------------------------------------
        public class PlumberFilteration
        {
            public string? name { get; set; }
            public string? phoneNumbers { get; set; }
            public string? licenseNumber { get; set; }
            public string? specialty { get; set; }
            public bool? isDeleted { get; set; }
            public int? page { get; set; }
            public int? pageSize { get; set; }
        }

        // -------------------------------------------------------------
        // Lightweight DTO for select boxes — only active plumbers
        // -------------------------------------------------------------
        public class PlumberLookupDto
        {
            public int id { get; set; }
            public string name { get; set; } = string.Empty;
            public string? specialty { get; set; }
        }

        public class PlumberLookupFilter
        {
            public string? name { get; set; }
            public string? specialty { get; set; }
        }

        // -------------------------------------------------------------
        // Excel import payloads
        // -------------------------------------------------------------
        public class PlumberImportRowError
        {
            public int rowNumber { get; set; }
            public string? plumberName { get; set; }
            public string column { get; set; } = string.Empty;
            public string message { get; set; } = string.Empty;
        }

        public class PlumberImportResultDto
        {
            public int totalRows { get; set; }
            public int successCount { get; set; }
            public int failedCount { get; set; }
            public List<PlumberDto> imported { get; set; } = new();
            public List<PlumberImportRowError> errors { get; set; } = new();
        }
    }
}

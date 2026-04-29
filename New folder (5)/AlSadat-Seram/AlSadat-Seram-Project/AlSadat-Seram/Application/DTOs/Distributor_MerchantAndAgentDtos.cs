using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    /// <summary>
    /// Distributor / Merchant / Agent unified DTO.
    /// Required for ALL flows (manual + Excel): fullName, phoneNumber, type.
    /// Optional everywhere (added back on-screen later if needed): address, cityId, gender.
    /// Audit fields (createdAt/By, updatedAt/By, deletedAt/By) are server-owned —
    /// they are accepted but ignored on Add/Edit; the service overwrites them
    /// from ICurrentUserService + DateTime.UtcNow.
    /// </summary>
    public class DistributorsAndMerchantsAndAgentsDto
    {
        public string? userId { get; set; }

        [Required(ErrorMessage = "الإسم مطلوب")]
        [MinLength(3, ErrorMessage = "يجب ألا يقل الاسم عن 3 أحرف")]
        [MaxLength(200, ErrorMessage = "الاسم لا يمكن أن يتجاوز 200 حرف")]
        public string fullName { get; set; } = string.Empty;

        // OPTIONAL — supports Excel import which never carries an address.
        [MaxLength(500, ErrorMessage = "العنوان لا يمكن أن يتجاوز 500 حرف")]
        public string? address { get; set; }

        public int? gender { get; set; }   // OPTIONAL

        // 0 = Distributor, 1 = Merchant, 2 = Agent
        [Required(ErrorMessage = "النوع مطلوب")]
        [Range(0, 2, ErrorMessage = "النوع غير صالح")]
        public int? type { get; set; }

        // OPTIONAL — Excel import always leaves cityId null.
        public int? cityId { get; set; }

        // Read-only on responses
        public string? cityName { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [RegularExpression(@"^\+?[0-9]{8,15}$",
            ErrorMessage = "رقم الهاتف غير صالح")]
        public string? phoneNumber { get; set; }

        // Server-owned audit fields (read-only on responses).
        public DateTime? createdAt { get; set; }
        public string? createdBy { get; set; }
        public DateTime? updatedAt { get; set; }
        public string? updatedBy { get; set; }
        public bool? isDelted { get; set; }
        public DateTime? deletedAt { get; set; }
        public string? deletedBy { get; set; }

        // Financial / discount fields
        public int? PointsBalance { get; set; }
        public decimal? cashBalance { get; set; }
        public decimal? indebtedness { get; set; }

        public decimal? firstSpecialDiscount { get; set; }
        public decimal? secondSpecialDiscount { get; set; }
        public decimal? thirdSpecialDiscount { get; set; }
    }

    public class DistributorsAndMerchantsFilters
    {
        public string? phoneNumber { get; set; }
        public string? fullName { get; set; }
        public string? cityName { get; set; }
        public int? type { get; set; }
        public bool? isDeleted { get; set; }
        public int? page { get; set; }
        public int? pageSize { get; set; }
    }

    public class DistributorMerchantExcelDto
    {
        public string الاسم_بالكامل { get; set; } = null!;
        public string النوع { get; set; } = null!; // موزع / تاجر / وكيل
        public string رقم_الهاتف { get; set; } = null!;
    }
}
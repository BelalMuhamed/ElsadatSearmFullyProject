using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class DistributorsAndMerchantsAndAgentsDto
    {
        public string? userId { get; set; }
        public string fullName { get; set; }
        public string address { get; set; }
        public int? gender { get; set; }
        public int type { get; set; } //0:Distributor, 1:Merchant
        public DateTime? createdAt { get; set; }
        public string? createdBy { get; set; }
        public DateTime? updatedAt { get; set; }
        public string? updatedBy { get; set; }
        public bool? isDelted { get; set; }
        public DateTime? deletedAt { get; set; }
        public string? deletedBy { get; set; }
        public int? cityId { get; set; }
        public string? cityName { get; set; }
        public string? phoneNumber { get; set; } //here we will use phone as email , phone ,username to make them login using it 
        public string? password { get; set; }
        public int? PointsBalance { get; set; }
        public decimal? cashBalance { get; set; }
        public decimal? indebtedness { get; set; } //المديونية
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


}

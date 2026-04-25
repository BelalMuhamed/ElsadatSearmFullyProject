using Domain.Entities.Commonitems;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Supplier : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public string phoneNumbers { get; set; } = string.Empty;
        public string? address { get; set; }

        // cityId is now NULLABLE:
        //  - Add/Edit forms may skip it (city is not mandatory anymore).
        //  - Excel import always leaves it null (import DTO is Name + Phone only).
        //  - City navigation property is also nullable to match.
        [ForeignKey(nameof(city))]
        public int? cityId { get; set; }
        public City? city { get; set; }
    }
}
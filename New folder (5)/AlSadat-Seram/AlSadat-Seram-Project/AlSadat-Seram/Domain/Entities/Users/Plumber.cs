using Domain.Entities.Commonitems;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// A plumber registered in the system as master data.
    /// <para>
    /// Mirrors <see cref="Supplier"/> in its core shape (name, phone, address, optional city,
    /// soft-delete) but adds two domain-specific fields:
    /// <list type="bullet">
    ///   <item><b>LicenseNumber</b> — the plumber's professional license identifier.</item>
    ///   <item><b>Specialty</b> — categorical free-text label (e.g. "سباكة منزلية",
    ///   "سباكة تجارية", "سباكة صناعية"). Kept as a string rather than an enum so new
    ///   specialties can be added without a schema migration.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Unlike <see cref="Supplier"/>, Plumber has <b>NO chart-of-accounts integration</b>.
    /// It is pure master data — no leaf account is created on insert, and it does not
    /// participate in journal entries. This is an explicit business decision.
    /// </para>
    /// </summary>
    public class Plumber : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public string phoneNumbers { get; set; } = string.Empty;
        public string? address { get; set; }

        /// <summary>Optional city — same nullable semantics as <see cref="Supplier.cityId"/>.</summary>
        [ForeignKey(nameof(city))]
        public int? cityId { get; set; }
        public City? city { get; set; }

        /// <summary>Professional license number. Optional but unique among active plumbers when present.</summary>
        public string? LicenseNumber { get; set; }

        /// <summary>
        /// Categorical text label for the plumber's specialty.
        /// Optional. The UI offers a dropdown of common values but accepts free input.
        /// </summary>
        public string? Specialty { get; set; }
    }
}
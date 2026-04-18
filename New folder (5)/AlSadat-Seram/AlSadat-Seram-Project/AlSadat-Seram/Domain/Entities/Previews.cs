using Domain.Entities.Commonitems;
using Domain.Entities.Transactions;
using Domain.Entities.Users;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Previews : BaseEntity
    {
        public string? CustomerName { get; set; }
        public int? CustomerPhoneNumber { get; set; }
        public string PreviewAddress { get; set; } = string.Empty;
        public PreviewStatus PreviewStatus { get; set; }
        public TypeOfCopon TypeOfCopon { get; set; }
        public int TotalPoint { get; set; }
        public bool WithCertificate { get; set; }
        public bool? WithCompaneyAccepted { get; set; }
        public bool? MerchantAccepted { get; set; } = true;


        public string? CreateBy { get; set; }
        public DateTime? CreateAt { get; set; } = DateTime.UtcNow;
        public string? UpdateBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool IsDeleted { get; set; }
        public string? DeleteBy { get; set; }
        public DateTime? DeleteAt { get; set; }

        public string? Notes { get; set; }
        public string? RejectionReason { get; set; }

        //------------- Obj From User and ForeignKey MerchantID ----------------
        [ForeignKey(nameof(Merchant))]
        public string MerchantID { get; set; } = string.Empty;
        [InverseProperty("MerchantPreviews")]
        public virtual ApplicationUser? Merchant { get; set; }

        //------------- Obj From User and ForeignKey PlumberID ----------------
        [ForeignKey(nameof(Plumber))]
        public string PlumberID { get; set; } = string.Empty;
        [InverseProperty("PlumberPreviews")]
        public virtual ApplicationUser? Plumber { get; set; }

        //------------- Obj From User and ForeignKey RepresentativeID ----------------
        [ForeignKey(nameof(Representative))]
        public string RepresentativeID { get; set; } = string.Empty;
        [InverseProperty("RepresentativePreviews")]
        public virtual ApplicationUser? Representative { get; set; }
        //------------- Obj From City and ForeignKey CityID ----------------
        [ForeignKey(nameof(City))]
        public int CityID { get; set; }
        public virtual City? City { get; set; }
        //------------- Obj From WarrantCertificate and ForeignKey WarrantCertificateID ----------------
        [ForeignKey(nameof(WarrantCertificate))]
        public int WarrantCertificateID { get; set; }
        public virtual WarrantCertificates? WarrantCertificate { get; set; }
        //------------- ICollection From PreviewItems ------------------------------
        public virtual ICollection<PreviewItems> PreviewItems { get; set; } = [];
        //------------- ICollection From PointTransactions ------------------------------
        public virtual ICollection<PointTransactions> PointTransactions { get; set; } = [];
        //------------- ICollection From RepresentativeCashTransactions ------------------------------
        public virtual ICollection<RepresentativeCashTransactions> RepresentativeCashTransactions { get; set; } = [];

    }

}

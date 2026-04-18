using Domain.Entities.Commonitems;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class WarrantCertificates : BaseEntity
    {
        public DateTime IssueDate { get; set; } = DateTime.Now;
        public DateTime ExpiryeDate { get; set; }
        public CertificateStatus CertificateStatus { get; set; } = CertificateStatus.Active;
        public string? CreateBy { get; set; }
        public DateTime? CreateAt { get; set; } = DateTime.UtcNow;
        public string? UpdateBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool IsDeleted { get; set; }
        public string? DeleteBy { get; set; }
        public DateTime? DeleteAt { get; set; }
        //----------- Obj From Previews and ForeignKey PreviewId -------------------
        [ForeignKey(nameof(Previews))]
        public int PreviewId { get; set; }
        public virtual Previews Previews { get; set; }
    }
}

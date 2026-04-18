using Domain.Entities.Transactions;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Users
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            Id = Guid.CreateVersion7().ToString();
            SecurityStamp = Guid.CreateVersion7().ToString();
        }
        public string FullName { get; set; }
        public string? Address { get; set; }
        public Gender Gender { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateAt { get; set; } = DateTime.UtcNow;
        public string? UpdateBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool IsDeleted { get; set; }
        public string? DeleteBy { get; set; }
        public DateTime? DeleteAt { get; set; }
        //----------- ICollection From RefreshTokens  ---------------------------------
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        //----------- Obj From City and ForeignKey CityId ---------------------------------
        [ForeignKey(nameof(City))]
        public int CityID { get; set; }
        public virtual City? City { get; set; }
        //----------- ICollection From Previews  ---------------------------------
        public virtual ICollection<Previews> MerchantPreviews { get; set; } = [];
        public virtual ICollection<Previews> PlumberPreviews { get; set; } = [];
        public virtual ICollection<Previews> RepresentativePreviews { get; set; } = [];
        //----------- ICollection From PointTransactions  ---------------------------------
        public virtual ICollection<PointTransactions> SenderTransactions { get; set; } = [];
        public virtual ICollection<PointTransactions> ReceverTransactions { get; set; } = [];

        // ----------- ICollection From RepresentativeCashTransactions  ---------------------------------
        public virtual ICollection<RepresentativeCashTransactions> RepresentativeCashTransactions { get; set; } = [];
    }
}


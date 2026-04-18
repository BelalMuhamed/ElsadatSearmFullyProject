using Domain.Entities.Commonitems;
using Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    namespace Domain.Entities
    {
        public class SpecialRepresentiveCity:BaseEntity
        {
            public DateTime? CreateAt { get; set; } = DateTime.Now;
            public string? CreateBy { get; set; }
            //----------- Obj From Region and ForeignKey RegionId ---------------------------------
            [ForeignKey(nameof(City))]
            public int CityID { get; set; }
            public virtual City? City { get; set; }
            //----------- Obj From User and ForeignKey CourierId ---------------------------------
            [Required]
            [ForeignKey(nameof(Representive))]
            public string RepresentiveCode { get; set; } = string.Empty;
            public virtual Representatives? Representive { get; set; }
        }
    }

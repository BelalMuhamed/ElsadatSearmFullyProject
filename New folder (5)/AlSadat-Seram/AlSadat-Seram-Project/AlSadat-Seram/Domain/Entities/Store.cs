using Domain.Entities.Commonitems;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Store:BaseEntity
    {
        [Required]
        public string StoreName { get; set; }
        public bool isDeleted { get; set; }
    }
}

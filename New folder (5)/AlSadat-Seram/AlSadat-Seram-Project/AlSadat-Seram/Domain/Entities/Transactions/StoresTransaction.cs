using Domain.Entities.Commonitems;
using Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Transactions
{
    public class StoresTransaction:BaseEntity
    {
        [ForeignKey(nameof(Source))]

        public int? SourceId { get; set; }
        [ForeignKey(nameof(Destenation))]

        public int? DestenationId { get; set; }
        public  Store? Source { get; set; }
        public Store? Destenation { get; set; }
        public string MakeTransactionUser { get; set; }
        public DateTime CreatedAt { get; set; } 


    }
}

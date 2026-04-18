using Domain.Entities;
using Domain.Entities.Commonitems;
using Domain.Entities.Transactions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class StoreTransactionDto
    {
        public int? id { get; set; }
        public int? sourceId { get; set; }
        public int? destenationId { get; set; }
        public string? sourceName { get; set; }
        public string? destenationName { get; set; }
        public string makeTransactionUser { get; set; }
        public List<StoreTransactionProductsDto>? transactionProducts { get; set; }
        public DateTime? createdAt { get; set; }= TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Egypt Standard Time");


    }
    public class StoreTransactionProductsDto
    {

        public int? transactionId { get; set; }
        public int productId { get; set; }
        public string? productName { get; set; }
        public decimal quantity { get; set; }
    }
    public class StoreTransactionFilters
    {
        public string? sourceName { get; set; }
        public string? destenationName { get; set; }
        public DateTime createdAt { get; set; }
        public int? page { get; set; }
        public int? pageSize { get; set; }

    }
}

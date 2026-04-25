using AlSadatSeram.Services.contract;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.FinanceDtos
{
    public class TreeAccountDto
    {
        public int? id { get; set; }
        public string accountName { get; set; }
        public string? accountCode { get; set; }
        public int? parentId { get; set; }
        public decimal? debit { get; set; }
        public bool isLeaf { get; set; }
        public bool isActive { get; set; }
        public decimal? credit { get; set; }
        public List<TreeAccountDto> children { get; set; } = new List<TreeAccountDto>();
    }
    public class AccountDto
    {
        public int? id { get; set; }
        public string? accountCode { get; set; }
        public string? userId { get; set; }
        public string accountName { get; set; }
        public int type { get; set; }
        public int? parentAccountId { get; set; }
        public bool isLeaf { get; set; }  
        public bool isActive { get; set; }
    }
    public class DisAndMerchAccountDto()
    {
        public string accountCode { get; set; }
        public string? userId { get; set; }
        public string accountName { get; set; }
        public int type { get; set; }
        public int? parentAccountId { get; set; }
        public bool isLeaf { get; set; }
        public bool isActive { get; set; }
        public decimal debit { get; set; }
        public decimal credit { get; set; }
    }

    public class AccountDetailsDtoReq
    {
        public int accountId { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public int?entryId { get; set; }
        public DateTime? entryDate { get; set; }
    }
    public class AccountDetailsDto
    {
        public int accountId { get; set; }
        public string accountName { get; set; }
        public string accountCode { get; set; }
        public int type { get; set; }
        public bool isActive { get; set; }

        public decimal currentBalance { get; set; }

        public ApiResponse<List<AccountMovementDto>> movements { get; set; }
    }
    public class AccountMovementDto
    {
        public int entryId { get; set; }
        public DateTime entryDate { get; set; }
        public string description { get; set; }
        public decimal debit { get; set; }
        public decimal credit { get; set; }
        public decimal runningBalance { get; set; }
    }
    public class AccountTotals
    {
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }
}

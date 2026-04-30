// File: Domain/Enums/SystemAccountCode.cs
// Stable identifiers for accounts the system depends on.
// Adding a new system account = add an enum member + seed it. No magic strings.

namespace Domain.Enums
{
    public enum SystemAccountCode
    {
        // Roots
        AssetsRoot = 1,
        LiabilitiesRoot = 2,
        EquityRoot = 3,
        IncomeRoot = 4,
        ExpensesRoot = 5,

        // Branches
        CurrentAssets = 10,
        ReceivablesParent = 11,    // المدينون — parent for customer sub-accounts
        SuppliersParent = 12,      // الموردين — parent for supplier sub-accounts

        // Leaves used by code
        Cash = 20,                 // النقدية
        Inventory = 21,            // المخزون
        Capital = 22,              // رأس المال
        SalesRevenue = 23,         // مبيعات المنتجات
        SalariesExpense = 24,      // رواتب الموظفين
        CostOfGoodsSold = 25       // تكلفة البضاعة المباعة
    }
}

// File: Domain/Enums/AccountNature.cs
namespace Domain.Enums
{
    public enum AccountNature
    {
        Debit = 1,
        Credit = 2
    }

    public static class AccountTypesExtensions
    {
        /// <summary>
        /// Normal balance side for a given account type.
        /// Assets & Expenses: Debit. Liabilities, Equity, Income: Credit.
        /// </summary>
        public static AccountNature Nature(this AccountTypes type) => type switch
        {
            AccountTypes.Assets => AccountNature.Debit,
            AccountTypes.Expenses => AccountNature.Debit,
            AccountTypes.Liabilities => AccountNature.Credit,
            AccountTypes.Equity => AccountNature.Credit,
            AccountTypes.Income => AccountNature.Credit,
            _ => AccountNature.Debit
        };
    }
}

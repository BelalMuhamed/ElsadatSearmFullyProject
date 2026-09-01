using Domain.Common;
using Domain.Entities;
using Domain.Entities.copounModel;
using Domain.Entities.Finance;
using Domain.Entities.Users;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class DbInitializer
    {
        private static readonly PasswordHasher<ApplicationUser> PasswordHasher = new();

        private static readonly List<(string Gov, List<string> Cities)> Egypt = new()
        {
            ("Cairo", new() { "Nasr City", "Heliopolis", "Maadi", "Shubra", "Downtown Cairo" }),
            ("Giza", new() { "6th of October", "Sheikh Zayed", "Dokki", "Mohandessin", "Haram" }),
            ("Alexandria", new() { "Smouha", "Sidi Gaber", "Miami", "Montaza", "Bolkly" }),
            ("Dakahlia", new() { "Mansoura", "Talkha", "Mit Ghamr" }),
            ("Sharqia", new() { "Zagazig", "10th of Ramadan", "Faqous" }),
            ("Qalyubia", new() { "Banha", "Shubra El Kheima", "Qalyub" }),
            ("Gharbia", new() { "Tanta", "El Mahalla El Kubra", "Kafr El Zayat" }),
            ("Monufia", new() { "Shebin El Kom", "Menouf", "Ashmoun" }),
            ("Beheira", new() { "Damanhur", "Kafr El Dawwar", "Rosetta" }),
            ("Fayoum", new() { "Fayoum City", "Tamiya", "Sinnuris" }),
            ("Beni Suef", new() { "Beni Suef City", "Nasser", "Ehnasia" }),
            ("Minya", new() { "Minya City", "Maghagha", "Beni Mazar" }),
            ("Assiut", new() { "Assiut City", "Dayrut", "Abnoub" }),
            ("Sohag", new() { "Sohag City", "Akhmim", "Girga" }),
            ("Qena", new() { "Qena City", "Nag Hammadi", "Dishna" }),
            ("Luxor", new() { "Luxor City", "Karnak", "Esna" }),
            ("Aswan", new() { "Aswan City", "Kom Ombo", "Edfu" }),
            ("Red Sea", new() { "Hurghada", "Safaga", "El Quseir" }),
            ("Suez", new() { "Suez City", "Ataqa", "Arbaeen" }),
            ("Ismailia", new() { "Ismailia City", "Fayed", "Qantara" }),
            ("North Sinai", new() { "Arish", "Sheikh Zuweid", "Rafah" }),
            ("South Sinai", new() { "Sharm El Sheikh", "Dahab", "Nuweiba" })
        };

        /// <summary>
        /// Orchestrates database seeding. Each step is independent and gated on its own
        /// "table empty" check, so re-running this after a partial seed only fills the gaps.
        /// This is NOT yet idempotent upsert-by-code (that lands with the permission
        /// catalogue in Phase 3) — it's still "seed once if empty", just decomposed and
        /// no longer sourcing secrets from a hardcoded literal.
        /// </summary>
        public static async Task SeedAsync(AppDbContext context, IConfiguration configuration)
        {
            await SeedLocationsAsync(context);

            var seededUserIds = await SeedUsersAsync(context, configuration);
            var seededRoleIds = await SeedRolesAsync(context);
            await SeedUserRolesAsync(context, seededUserIds, seededRoleIds);

            await SeedEmployeePermissionsAsync(context);
            await SeedAccountsAsync(context);
            await SeedBillDiscountsAsync(context);
            await SeedCouponsAsync(context);
        }

        private static async Task SeedLocationsAsync(AppDbContext context)
        {
            if (context.Governrate.Any())
                return;

            var governrates = Egypt.Select(g => new Governrate
            {
                Name = g.Gov,
                Cities = g.Cities.Select(c => new City { Name = c }).ToList()
            }).ToList();

            context.Governrate.AddRange(governrates);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Every seeded account — both admins and every legacy-role demo user —
        /// shares one password sourced from Auth:SeedUsersPassword. There is no
        /// hardcoded credential anywhere in this method.
        /// </summary>
        private static async Task<SeededUserIds> SeedUsersAsync(AppDbContext context, IConfiguration configuration)
        {
            if (context.Users.Any())
                return SeededUserIds.Empty;

            var seedPassword = configuration["Auth:SeedUsersPassword"]
                ?? throw new InvalidOperationException(
                    "Auth:SeedUsersPassword is not configured. Set it via 'dotnet user-secrets set \"Auth:SeedUsersPassword\" \"<password>\"' before seeding.");

            var hashedPassword = PasswordHasher.HashPassword(null!, seedPassword);

            var ids = new SeededUserIds
            {
                Admin1 = Guid.CreateVersion7().ToString(),
                Admin2 = Guid.CreateVersion7().ToString(),
                Hr = Guid.CreateVersion7().ToString(),
                Accountant = Guid.CreateVersion7().ToString(),
                StockManager = Guid.CreateVersion7().ToString()
            };

            context.Users.AddRange(
                new ApplicationUser
                {
                    Id = ids.Admin1,
                    FullName = "Mahmoud Elweswemy",
                    UserName = "m.elweswemy",
                    Email = "Weso430@gmail.com",
                    NormalizedUserName = "m.elweswemy".ToUpper(),
                    NormalizedEmail = "Weso430@gmail.com".ToUpper(),
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    PasswordHash = hashedPassword,
                    PhoneNumber = "01032500077",
                    Gender = Gender.Male,
                    CityID = 1,
                },
                new ApplicationUser
                {
                    Id = ids.Admin2,
                    FullName = "Belal Basal",
                    UserName = "b.basal",
                    Email = "basalbelal25@gmail.com",
                    NormalizedUserName = "b.basal".ToUpper(),
                    NormalizedEmail = "basalbelal25@gmail.com".ToUpper(),
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    PasswordHash = hashedPassword,
                    PhoneNumber = "01008319684",
                    Gender = Gender.Male,
                    CityID = 1,
                },
                new ApplicationUser
                {
                    Id = ids.Hr,
                    FullName = "Hr",
                    UserName = "Hr",
                    Email = "Hr@gmail.com",
                    NormalizedUserName = "H.r".ToUpper(),
                    NormalizedEmail = "Hr@gmail.com".ToUpper(),
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    PasswordHash = hashedPassword,
                    PhoneNumber = "01008219684",
                    Gender = Gender.Male,
                    CityID = 1,
                },
                new ApplicationUser
                {
                    Id = ids.Accountant,
                    FullName = "Accountatnt",
                    UserName = "accountatnt",
                    Email = "Accountatnt@gmail.com",
                    NormalizedUserName = "accountatnt".ToUpper(),
                    NormalizedEmail = "Accountatnt@gmail.com".ToUpper(),
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    PasswordHash = hashedPassword,
                    PhoneNumber = "01008218684",
                    Gender = Gender.Male,
                    CityID = 1,
                },
                new ApplicationUser
                {
                    Id = ids.StockManager,
                    FullName = "Stock Manager",
                    UserName = "stockManager",
                    Email = "stockManager@gmail.com",
                    NormalizedUserName = "stockManager".ToUpper(),
                    NormalizedEmail = "stockManager@gmail.com".ToUpper(),
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    PasswordHash = hashedPassword,
                    PhoneNumber = "01008218784",
                    Gender = Gender.Male,
                    CityID = 1,
                }
            );

            await context.SaveChangesAsync();
            return ids;
        }

        private static async Task<SeededRoleIds> SeedRolesAsync(AppDbContext context)
        {
            if (context.Roles.Any())
                return SeededRoleIds.Empty;

            var ids = new SeededRoleIds
            {
                Admin = Guid.CreateVersion7().ToString(),
                Hr = Guid.CreateVersion7().ToString(),
                Accountant = Guid.CreateVersion7().ToString(),
                StockManager = Guid.CreateVersion7().ToString()
            };

            context.Roles.AddRange(
                new ApplicationRole { Id = ids.Admin, Name = AppRoles.Admin, NormalizedName = AppRoles.Admin.ToUpper(), ConcurrencyStamp = Guid.NewGuid().ToString() },
                new ApplicationRole { Id = ids.Hr, Name = AppRoles.HR, NormalizedName = AppRoles.HR.ToUpper(), ConcurrencyStamp = Guid.NewGuid().ToString() },
                new ApplicationRole { Id = Guid.CreateVersion7().ToString(), Name = AppRoles.Merchant, NormalizedName = AppRoles.Merchant.ToUpper(), ConcurrencyStamp = Guid.NewGuid().ToString() },
                new ApplicationRole { Id = Guid.CreateVersion7().ToString(), Name = AppRoles.Plumber, NormalizedName = AppRoles.Plumber.ToUpper(), ConcurrencyStamp = Guid.NewGuid().ToString() },
                new ApplicationRole { Id = Guid.CreateVersion7().ToString(), Name = AppRoles.Representative, NormalizedName = AppRoles.Representative.ToUpper(), ConcurrencyStamp = Guid.NewGuid().ToString() },
                new ApplicationRole { Id = ids.Accountant, Name = AppRoles.Accountant, NormalizedName = AppRoles.Accountant.ToUpper(), ConcurrencyStamp = Guid.NewGuid().ToString() },
                new ApplicationRole { Id = Guid.CreateVersion7().ToString(), Name = AppRoles.Agent, NormalizedName = AppRoles.Agent.ToUpper(), ConcurrencyStamp = Guid.NewGuid().ToString() },
                new ApplicationRole { Id = Guid.CreateVersion7().ToString(), Name = AppRoles.Distributor, NormalizedName = AppRoles.Distributor.ToUpper(), ConcurrencyStamp = Guid.NewGuid().ToString() },
                new ApplicationRole { Id = ids.StockManager, Name = AppRoles.StockManager, NormalizedName = AppRoles.StockManager.ToUpper(), ConcurrencyStamp = Guid.NewGuid().ToString() },
                new ApplicationRole { Id = Guid.CreateVersion7().ToString(), Name = AppRoles.Employee, NormalizedName = AppRoles.Employee.ToUpper(), ConcurrencyStamp = Guid.NewGuid().ToString() }
            );

            await context.SaveChangesAsync();
            return ids;
        }

        private static async Task SeedUserRolesAsync(AppDbContext context, SeededUserIds userIds, SeededRoleIds roleIds)
        {
            if (context.UserRoles.Any() || userIds.IsEmpty || roleIds.IsEmpty)
                return;

            var userRoles = new List<IdentityUserRole<string>>
            {
                new() { UserId = userIds.Admin1, RoleId = roleIds.Admin },
                new() { UserId = userIds.Admin2, RoleId = roleIds.Admin },
                new() { UserId = userIds.Hr, RoleId = roleIds.Hr },
                new() { UserId = userIds.Accountant, RoleId = roleIds.Accountant },
                new() { UserId = userIds.StockManager, RoleId = roleIds.StockManager }
            };

            context.UserRoles.AddRange(userRoles);
            await context.SaveChangesAsync();
        }

        private static async Task SeedEmployeePermissionsAsync(AppDbContext context)
        {
            if (context.Set<Domain.Entities.Authorization.Module>().Any())
                return;

            var employeesModule = new Domain.Entities.Authorization.Module { Name = EmployeePermissions.ModuleName };
            context.Set<Domain.Entities.Authorization.Module>().Add(employeesModule);
            await context.SaveChangesAsync();

            context.Set<Domain.Entities.Authorization.Permission>().AddRange(
                new Domain.Entities.Authorization.Permission { ModuleId = employeesModule.Id, Name = "View", Description = "View employees" },
                new Domain.Entities.Authorization.Permission { ModuleId = employeesModule.Id, Name = "Create", Description = "Create employees" },
                new Domain.Entities.Authorization.Permission { ModuleId = employeesModule.Id, Name = "Update", Description = "Update employees" },
                new Domain.Entities.Authorization.Permission { ModuleId = employeesModule.Id, Name = "Delete", Description = "Soft-delete/restore employees" },
                new Domain.Entities.Authorization.Permission { ModuleId = employeesModule.Id, Name = "AssignPermissions", Description = "Manage employee permissions" }
            );
            await context.SaveChangesAsync();

            // Deliberately NO UserPermissions rows here — deny-by-default (Decision 8).
            // Your seeded HR demo user will need Employees.* granted explicitly via
            // PUT api/Permission/user/{hrUserId} after this migration runs, or they will
            // lose access to Employee Management until you do.
        }

        private static async Task SeedAccountsAsync(AppDbContext context)
        {
            if (context.Accounts.Any())
                return;

            // Note: AccountCode values match the existing seed (1, 1.1, 1.1.1, ...).
            // Payroll service will be migrated to look up by SystemCode (no magic strings).
            var accounts = new List<ChartOfAccounts>
            {
                new() { Id = 1,  AccountCode = "1",     AccountName = "الأصول",            Type = AccountTypes.Assets,      IsLeaf = false, IsActive = true, IsSystemAccount = true, SystemCode = SystemAccountCode.AssetsRoot },
                new() { Id = 2,  AccountCode = "2",     AccountName = "الخصوم",            Type = AccountTypes.Liabilities, IsLeaf = false, IsActive = true, IsSystemAccount = true, SystemCode = SystemAccountCode.LiabilitiesRoot },
                new() { Id = 3,  AccountCode = "3",     AccountName = "حقوق الملكية",      Type = AccountTypes.Equity,      IsLeaf = false, IsActive = true, IsSystemAccount = true, SystemCode = SystemAccountCode.EquityRoot },
                new() { Id = 4,  AccountCode = "4",     AccountName = "الإيرادات",         Type = AccountTypes.Income,      IsLeaf = false, IsActive = true, IsSystemAccount = true, SystemCode = SystemAccountCode.IncomeRoot },
                new() { Id = 5,  AccountCode = "5",     AccountName = "المصروفات",         Type = AccountTypes.Expenses,    IsLeaf = false, IsActive = true, IsSystemAccount = true, SystemCode = SystemAccountCode.ExpensesRoot },

                new() { Id = 6,  AccountCode = "1.1",   AccountName = "الأصول المتداولة",  ParentAccountId = 1, Type = AccountTypes.Assets,      IsLeaf = false, IsActive = true, IsSystemAccount = true, SystemCode = SystemAccountCode.CurrentAssets },
                new() { Id = 7,  AccountCode = "1.1.1", AccountName = "النقدية",           ParentAccountId = 6, Type = AccountTypes.Assets,      IsLeaf = true,  IsActive = true, IsSystemAccount = true, SystemCode = SystemAccountCode.Cash },
                new() { Id = 8,  AccountCode = "1.1.2", AccountName = "المدينون",          ParentAccountId = 6, Type = AccountTypes.Assets,      IsLeaf = false, IsActive = true, IsSystemAccount = true, SystemCode = SystemAccountCode.ReceivablesParent },
                new() { Id = 9,  AccountCode = "1.1.3", AccountName = "المخزون",           ParentAccountId = 6, Type = AccountTypes.Assets,      IsLeaf = true,  IsActive = true, IsSystemAccount = true, SystemCode = SystemAccountCode.Inventory },

                new() { Id = 10, AccountCode = "2.1",   AccountName = "الموردين",          ParentAccountId = 2, Type = AccountTypes.Liabilities, IsLeaf = false, IsActive = true, IsSystemAccount = true, SystemCode = SystemAccountCode.SuppliersParent },
                new() { Id = 11, AccountCode = "3.1",   AccountName = "رأس المال",         ParentAccountId = 3, Type = AccountTypes.Equity,      IsLeaf = true,  IsActive = true, IsSystemAccount = true, SystemCode = SystemAccountCode.Capital },
                new() { Id = 12, AccountCode = "4.1",   AccountName = "مبيعات المنتجات",   ParentAccountId = 4, Type = AccountTypes.Income,      IsLeaf = true,  IsActive = true, IsSystemAccount = true, SystemCode = SystemAccountCode.SalesRevenue },
                new() { Id = 13, AccountCode = "5.1",   AccountName = "رواتب الموظفين",    ParentAccountId = 5, Type = AccountTypes.Expenses,    IsLeaf = true,  IsActive = true, IsSystemAccount = true, SystemCode = SystemAccountCode.SalariesExpense },
                new() { Id = 14, AccountCode = "5.2",   AccountName = "تكلفة البضاعة المباعة", ParentAccountId = 5, Type = AccountTypes.Expenses, IsLeaf = true, IsActive = true, IsSystemAccount = true, SystemCode = SystemAccountCode.CostOfGoodsSold },
            };

            await context.Database.OpenConnectionAsync();
            try
            {
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Accounts ON");
                context.Accounts.AddRange(accounts);
                await context.SaveChangesAsync();
                await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Accounts OFF");
            }
            finally
            {
                await context.Database.CloseConnectionAsync();
            }
        }

        private static async Task SeedBillDiscountsAsync(AppDbContext context)
        {
            if (context.BillDiscounts.Any())
                return;

            context.BillDiscounts.Add(new Domain.Entities.Invoices.Billdiscounts
            {
                FirstDiscount = 5,
                SecondDiscount = 5,
                ThirdDiscount = 5
            });
            await context.SaveChangesAsync();
        }

        private static async Task SeedCouponsAsync(AppDbContext context)
        {
            if (context.CopounGeneralSetting.Any())
                return;

            context.CopounGeneralSetting.AddRange(
                new List<Copoun>
                {
                    new()
                    {
                        CopounDesc = "60 كاش",
                        CopounPaiedType = TypeOfCopon.Cash,
                        IsActive = true,
                        PaiedCash = 60,
                        PointsToCollectCopoun = 60,
                        Stars = 0
                    },
                    new()
                    {
                        CopounDesc = "  كاش 50 + 10 نجوم",
                        CopounPaiedType = TypeOfCopon.Cash,
                        IsActive = true,
                        PaiedCash = 50,
                        PointsToCollectCopoun = 60,
                        Stars = 10
                    }
                }
            );
            await context.SaveChangesAsync();
        }

        private sealed class SeededUserIds
        {
            public string Admin1 { get; init; } = string.Empty;
            public string Admin2 { get; init; } = string.Empty;
            public string Hr { get; init; } = string.Empty;
            public string Accountant { get; init; } = string.Empty;
            public string StockManager { get; init; } = string.Empty;

            public bool IsEmpty => string.IsNullOrEmpty(Admin1);
            public static SeededUserIds Empty => new();
        }

        private sealed class SeededRoleIds
        {
            public string Admin { get; init; } = string.Empty;
            public string Hr { get; init; } = string.Empty;
            public string Accountant { get; init; } = string.Empty;
            public string StockManager { get; init; } = string.Empty;

            public bool IsEmpty => string.IsNullOrEmpty(Admin);
            public static SeededRoleIds Empty => new();
        }
    }
}
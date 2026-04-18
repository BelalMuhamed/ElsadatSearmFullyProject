using Domain.Common;
using Domain.Entities;
using Domain.Entities.copounModel;
using Domain.Entities.Finance;
using Domain.Entities.Users;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class DbInitializer
    {
       static PasswordHasher<ApplicationUser> passwordHasher = new PasswordHasher<ApplicationUser>();
        static string hashedPassword = passwordHasher.HashPassword(null!, "12345678Ss+");
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
        public static async Task SeedAsync(AppDbContext context)
        {
            string user1AdminId = string.Empty;
            string user2AdminId = string.Empty;
            string user3HrId = string.Empty;
            string accountatntUserId=string.Empty;
            string stockManagerUserId=string.Empty;
            string adminRoleId = string.Empty;
            string hrRoleId=string.Empty;
            string accountRoleId=string.Empty;
            string stockManagerRoleId=string.Empty;
            if (!context.Governrate.Any())
            {
                var governrates = new List<Governrate>();

                foreach (var g in Egypt)
                {
                    governrates.Add(new Governrate
                    {
                        Name = g.Gov,
                        Cities = g.Cities.Select(c => new City
                        {
                            Name = c
                        }).ToList()
                    });
                }

                context.Governrate.AddRange(governrates);
                await context.SaveChangesAsync();
            }
            if (!context.Users.Any())
            {
                user1AdminId= Guid.CreateVersion7().ToString();
                user2AdminId= Guid.CreateVersion7().ToString();
                user3HrId= Guid.CreateVersion7().ToString();
                accountatntUserId= Guid.CreateVersion7().ToString();
                stockManagerUserId = Guid.CreateVersion7().ToString();
                context.Users.AddRange(
                 new ApplicationUser
                 {
                     Id = user1AdminId,
                     FullName = "Mahmoud Elweswemy",
                     UserName = "m.elweswemy",
                     Email = "Weso430@gmail.com",
                     NormalizedUserName = "m.elweswemy".ToUpper(),
                     NormalizedEmail = "Weso430@gmail.com".ToUpper(),
                     SecurityStamp = "0195d43be3f271878cc37be7dfc34361",
                     ConcurrencyStamp = "0195d43b-a808-757b-9c3e-bf90c6091133",
                     PasswordHash = hashedPassword,
                     PhoneNumber = "01032500077",
                     Gender = Gender.Male,
                     CityID = 1,
                 },
                 new ApplicationUser
                 {
                     Id = user2AdminId,
                     FullName = "Belal Basal",
                     UserName = "b.basal",
                     Email = "basalbelal25@gmail.com",
                     NormalizedUserName = "b.basal".ToUpper(),
                     NormalizedEmail = "basalbelal25@gmail.com".ToUpper(),
                     SecurityStamp = "0185d43be5f271878cc37be7dfc34361",
                     ConcurrencyStamp = "01875d43b-a78-757b-9c3e-bf90c6091133",
                     PasswordHash = hashedPassword,
                     PhoneNumber = "01008319684",
                     Gender = Gender.Male,
                     CityID = 1,
                 },
                  new ApplicationUser
                  {
                      Id = user3HrId,
                      FullName = "Hr",
                      UserName = "Hr",
                      Email = "Hr@gmail.com",
                      NormalizedUserName = "H.r".ToUpper(),
                      NormalizedEmail = "Hr@gmail.com".ToUpper(),
                      SecurityStamp = "018453be5f271878cc37be7dfc34361",
                      ConcurrencyStamp = "01848d43a-a78-757b-9c3e-bf90c6091133",
                      PasswordHash = hashedPassword,
                      PhoneNumber = "01008219684",
                      Gender = Gender.Male,
                      CityID = 1,
                  },
                   new ApplicationUser
                   {
                       Id = accountatntUserId,
                       FullName = "Accountatnt",
                       UserName = "accountatnt",
                       Email = "Accountatnt@gmail.com",
                       NormalizedUserName = "accountatnt".ToUpper(),
                       NormalizedEmail = "Accountatnt@gmail.com".ToUpper(),
                       SecurityStamp = "018453be3cc271878cc37be7dfc34361",
                       ConcurrencyStamp = "01848d43a-7a8-757b-9c3e-bf90c6091133",
                       PasswordHash = hashedPassword,
                       PhoneNumber = "01008218684",
                       Gender = Gender.Male,
                       CityID = 1,
                   },
                   new ApplicationUser
                   {
                       Id = stockManagerUserId,
                       FullName = "Stock Manager",
                       UserName = "stockManager",
                       Email = "stockManager@gmail.com",
                       NormalizedUserName = "stockManager".ToUpper(),
                       NormalizedEmail = "stockManager@gmail.com".ToUpper(),
                       SecurityStamp = "018453be3cc172878cc37be7dfc34361",
                       ConcurrencyStamp = "01848d43a-7a8-757b-9c3e-bf07c6091133",
                       PasswordHash = hashedPassword,
                       PhoneNumber = "01008218784",
                       Gender = Gender.Male,
                       CityID = 1,
                   }
                );

                await context.SaveChangesAsync();
            }

            if(!context.Roles.Any())
            {
                adminRoleId = Guid.CreateVersion7().ToString();
                hrRoleId= Guid.CreateVersion7().ToString();
                accountRoleId= Guid.CreateVersion7().ToString();
                stockManagerRoleId = Guid.NewGuid().ToString();
                context.Roles.AddRange(
                    new ApplicationRole
                    {
                        Id = adminRoleId,
                        Name = AppRoles.Admin,
                        NormalizedName =AppRoles.Admin.ToUpper(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    },
                    new ApplicationRole
                    {
                        Id = hrRoleId,
                        Name = AppRoles.HR,
                        NormalizedName = AppRoles.HR.ToUpper(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    },
                    new ApplicationRole
                    {
                        Id = Guid.CreateVersion7().ToString(),
                        Name = AppRoles.Merchant,
                        NormalizedName = AppRoles.Merchant.ToUpper(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    },
                    new ApplicationRole
                    {
                        Id = Guid.CreateVersion7().ToString(),
                        Name = AppRoles.Plumber,
                        NormalizedName = AppRoles.Plumber.ToUpper(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    },
                    new ApplicationRole
                    {
                        Id = Guid.CreateVersion7().ToString(),
                        Name = AppRoles.Representative,
                        NormalizedName = AppRoles.Representative.ToUpper(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    },
                     new ApplicationRole
                     {
                         Id = accountRoleId,
                         Name = AppRoles.Accountant,
                         NormalizedName = AppRoles.Accountant.ToUpper(),
                         ConcurrencyStamp = Guid.NewGuid().ToString()
                     },
                    new ApplicationRole
                    {
                        Id = Guid.CreateVersion7().ToString(),
                        Name = AppRoles.Agent,
                        NormalizedName = AppRoles.Agent.ToUpper(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    },
                      new ApplicationRole
                      {
                          Id = Guid.CreateVersion7().ToString(),
                          Name = AppRoles.Distributor,
                          NormalizedName = AppRoles.Distributor.ToUpper(),
                          ConcurrencyStamp = Guid.NewGuid().ToString()
                      },
                         new ApplicationRole
                         {
                             Id = stockManagerRoleId,
                             Name = AppRoles.StockManager,
                             NormalizedName = AppRoles.StockManager.ToUpper(),
                             ConcurrencyStamp = Guid.NewGuid().ToString()
                         }
                    );
                await context.SaveChangesAsync();

             
            }
            if (!context.UserRoles.Any())
            {

                var userRoles = new List<IdentityUserRole<string>>
                        {
                            new IdentityUserRole<string>
                            {
                                UserId = user1AdminId,
                                RoleId = adminRoleId
                            },
                            new IdentityUserRole<string>
                            {
                                UserId = user2AdminId,
                                RoleId = adminRoleId
                            },
                            new IdentityUserRole<string>
                            {
                                UserId=user3HrId,
                                RoleId=hrRoleId
                            },
                            new IdentityUserRole<string>
                            {
                                UserId=accountatntUserId,
                                RoleId=accountRoleId
                            },
                            new IdentityUserRole<string>
                            {
                                UserId=stockManagerUserId,
                                RoleId=stockManagerRoleId
                            }
                        };

                context.UserRoles.AddRange(userRoles);
                await context.SaveChangesAsync();

            }


            if (!context.Accounts.Any())
            {
                var accounts = new List<ChartOfAccounts>
    {
        new ChartOfAccounts { Id = 1, AccountCode = "1", AccountName = "الأصول", Type = AccountTypes.Assets, IsLeaf = false, IsActive = true },
        new ChartOfAccounts { Id = 2, AccountCode = "2", AccountName = "الخصوم", Type = AccountTypes.Liabilities, IsLeaf = false, IsActive = true },
        new ChartOfAccounts { Id = 3, AccountCode = "3", AccountName = "حقوق الملكية", Type = AccountTypes.Equity, IsLeaf = false, IsActive = true },
        new ChartOfAccounts { Id = 4, AccountCode = "4", AccountName = "الإيرادات", Type = AccountTypes.Income, IsLeaf = false, IsActive = true },
        new ChartOfAccounts { Id = 5, AccountCode = "5", AccountName = "المصروفات", Type = AccountTypes.Expenses, IsLeaf = false, IsActive = true },

        new ChartOfAccounts { Id = 6, AccountCode = "1.1", AccountName = "الأصول المتداولة", ParentAccountId = 1, Type = AccountTypes.Assets, IsLeaf = false, IsActive = true },
        new ChartOfAccounts { Id = 7, AccountCode = "1.1.1", AccountName = "النقدية", ParentAccountId = 6, Type = AccountTypes.Assets, IsLeaf = true, IsActive = true },
        new ChartOfAccounts { Id = 8, AccountCode = "1.1.2", AccountName = "المدينون", ParentAccountId = 6, Type = AccountTypes.Assets, IsLeaf = false, IsActive = true },
        new ChartOfAccounts { Id = 9, AccountCode = "1.1.3", AccountName = "المخزون", ParentAccountId = 6, Type = AccountTypes.Assets, IsLeaf = true, IsActive = true },

        new ChartOfAccounts { Id = 10, AccountCode = "2.1", AccountName = "الموردين", ParentAccountId = 2, Type = AccountTypes.Liabilities, IsLeaf = false, IsActive = true },
        new ChartOfAccounts { Id = 11, AccountCode = "3.1", AccountName = "رأس المال", ParentAccountId = 3, Type = AccountTypes.Equity, IsLeaf = true, IsActive = true },
        new ChartOfAccounts { Id = 12, AccountCode = "4.1", AccountName = "مبيعات المنتجات", ParentAccountId = 4, Type = AccountTypes.Income, IsLeaf = true, IsActive = true },
        new ChartOfAccounts { Id = 13, AccountCode = "5.1", AccountName = "رواتب الموظفين", ParentAccountId = 5, Type = AccountTypes.Expenses, IsLeaf = false, IsActive = true }
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

            if(!context.BillDiscounts.Any())
            {
                context.BillDiscounts.Add(new Domain.Entities.Invoices.Billdiscounts() { 
                
                FirstDiscount=5,
                SecondDiscount=5,
                ThirdDiscount=5

                });
                await context.SaveChangesAsync();
            }
            if(!context.CopounGeneralSetting.Any())
            {
                context.CopounGeneralSetting.AddRange(
                    new List<Copoun>() {
                        new Copoun{
                            CopounDesc="60 كاش",
                             CopounPaiedType=TypeOfCopon.Cash,
                             IsActive=true,
                             PaiedCash=60,
                             PointsToCollectCopoun=60,
                             Stars=0
                    },new Copoun
                    {
                         CopounDesc="  كاش 50 + 10 نجوم",
                             CopounPaiedType=TypeOfCopon.Cash,
                             IsActive=true,
                             PaiedCash=50,
                             PointsToCollectCopoun=60,
                             Stars=10
                    } }
                    );
                await context.SaveChangesAsync();
            }
        }
    }
}

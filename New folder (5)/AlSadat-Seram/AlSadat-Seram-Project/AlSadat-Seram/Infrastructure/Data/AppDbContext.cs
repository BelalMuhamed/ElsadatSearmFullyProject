using Domain.Common;
using Domain.Entities;
using Domain.Entities.Audit;
using Domain.Entities.Authorization;
using Domain.Entities.copounModel;
using Domain.Entities.Finance;
using Domain.Entities.Numbering;
using Domain.Entities.HR;
using Domain.Entities.Invoices;
using Domain.Entities.Transactions;
using Domain.Entities.Users;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;


namespace Infrastructure.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
    {
        #region HR       
        public DbSet<CollectionRepresentiveRate> CollectionRepresentiveRates { get; set; }
        public DbSet<CoponCollectionRepresentiveRate> CoponCollectionRepresentiveRates { get; set; }
        public DbSet<EmployeeAttendance> EmployeeAttendance { get; set; }
        public DbSet<PublicHoliday> PublicHoliday { get; set; }
        public DbSet<RepresentativeAttendance> RepresentativeAttendance { get; set; }
        public DbSet<EmployeeBonus> EmployeeBonu { get; set; }
        public DbSet<EmployeeLeaveBalance> EmployeeLeaveBalance { get; set; }
        public DbSet<EmployeeLeaveRequest> EmployeeLeaveRequest { get; set; }
        public DbSet<EmployeeLoan> EmployeeLoan { get; set; }
        public DbSet<EmployeeLoanPayments> EmployeeLoanPayment { get; set; }
        public DbSet<LeaveType> LeaveType { get; set; }
        public DbSet<Payroll> Payroll { get; set; }
        public DbSet<PayrollDeductions> PayrollDeduction { get; set; }
        #endregion
        #region Invoices
        public DbSet<CompanyExpensesInvoices> CompanyExpensesInvoices { get; set; }
        public DbSet<PurchaseInvoice> PurchaseInvoice { get; set; }
        public DbSet<PurchaseInvoiceItems> PurchaseInvoiceItems { get; set; }
        public DbSet<SalesInvoiceItems> SalesInvoiceItems { get; set; }
        public DbSet<SalesInvoices> SalesInvoices { get; set; }
        #endregion
        #region Transactions
        public DbSet<PointTransactions> PointTransactions { get; set; }
        public DbSet<RepresentativeCashTransactions> RepresentativeCashTransactions { get; set; }
        #endregion
        #region Users
        public DbSet<Distributor_Merchant_Agent> Distributors_MerchantsAndAgents { get; set; }
        public DbSet<Employee> Employee { get; set; }
        //public DbSet<Plumbers> Plumbers { get; set; }
        public DbSet<Plumber> Plumbers { get; set; }
        public DbSet<Representatives> Representatives { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        #endregion
       
        public DbSet<City> City { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<Governrate> Governrate { get; set; }
        public DbSet<PreviewItems> PreviewItems { get; set; }
        public DbSet<Previews> Previews { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<SpecialRepresentiveCity> SpecialRepresentiveCity { get; set; }
        public DbSet<WarrantCertificates> WarrantCertificates { get; set; }
        public DbSet<Copoun> CopounGeneralSetting { get; set; }
        public DbSet<Billdiscounts> BillDiscounts { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<StoresTransaction> StoresTransaction { get; set; }
        public DbSet<TransactionProducts> transactionProducts { get; set; }
        public DbSet<JournalEntries> journalEntries { get; set; }
        public DbSet<JournalEntryDetails> journalEntriesDetails { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<ChartOfAccounts> Accounts { get; set; }
        public DbSet<Domain.Entities.Authorization.Module> Modules { get; set; }
        public DbSet<Domain.Entities.Authorization.Permission> Permissions { get; set; }
        public DbSet<Domain.Entities.Authorization.UserPermission> UserPermissions { get; set; }
        public DbSet<Domain.Entities.Auth.AuthAuditLog> AuthAuditLogs { get; set; }
        public DbSet<AuditEntry> AuditEntries { get; set; }
        public DbSet<DocumentSequence> DocumentSequences { get; set; }

        public DbSet<SalesInvoiceItemStoresQuantities> SalesInvoiceItemStoresQuantities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // ── Modules & Permissions: stable codes ─────────────────────────────
            builder.Entity<Domain.Entities.Authorization.Module>()
                .HasIndex(m => m.Code)
                .IsUnique();

            builder.Entity<Domain.Entities.Authorization.Permission>()
                .HasIndex(p => p.Code)
                .IsUnique();

            // ── UserPermission.GrantedByUserId: real FK, NO ACTION on delete ───
            builder.Entity<Domain.Entities.Authorization.UserPermission>()
                .HasOne(up => up.GrantedByUser)
                .WithMany()
                .HasForeignKey(up => up.GrantedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── AspNetUserRoles: exactly one role per user (R-5) ────────────────
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>()
                .HasIndex(ur => ur.UserId)
                .IsUnique();

            // ── ApplicationUser: phone required+unique, full name bounded ──────
            builder.Entity<ApplicationUser>()
                .Property(u => u.PhoneNumber)
                .HasMaxLength(20)
                .IsRequired();

            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();

            builder.Entity<ApplicationUser>()
                .Property(u => u.FullName)
                .HasMaxLength(200)
                .IsRequired();

            // ── ApplicationUser.DepartmentId shadow FK: dropped (D-2) ───────────
            // The existing HasOne(Department, null).WithMany("Employee").HasForeignKey("DepartmentId")
            // configuration is REMOVED, not added — delete that block wherever it currently
            // appears in this file (it configures a shadow property, not a declared one).

            // ── Employee / Representatives: enforce true 1:1 with ApplicationUser (D-1) ─
            builder.Entity<Domain.Entities.Users.Employee>()
                .HasIndex(e => e.UserId)
                .IsUnique();

            builder.Entity<Domain.Entities.Users.Representatives>()
                .HasIndex(r => r.UserId)
                .IsUnique();

            // ── RefreshTokens: new shape ─────────────────────────────────────────
            builder.Entity<RefreshToken>()
                .HasIndex(t => t.TokenHash)
                .IsUnique();

            builder.Entity<RefreshToken>()
                .HasIndex(t => t.FamilyId);

            builder.Entity<RefreshToken>()
                .HasOne(t => t.ReplacedByToken)
                .WithMany()
                .HasForeignKey(t => t.ReplacedByTokenId)
                .OnDelete(DeleteBehavior.NoAction);

            // ── AuthAuditLog ─────────────────────────────────────────────────────
            builder.Entity<Domain.Entities.Auth.AuthAuditLog>()
                .HasIndex(a => new { a.UserId, a.OccurredAt });

            builder.Entity<Domain.Entities.Auth.AuthAuditLog>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Domain.Entities.Auth.AuthAuditLog>()
                .HasOne(a => a.ActorUser)
                .WithMany()
                .HasForeignKey(a => a.ActorUserId)
                .OnDelete(DeleteBehavior.NoAction);
            // in OnModelCreating
            builder.Entity<UserPermission>()
                .HasKey(up => new { up.UserId, up.PermissionId });

            builder.Entity<UserPermission>()
                .HasOne(up => up.User)
                .WithMany()
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserPermission>()
                .HasOne(up => up.Permission)
                .WithMany(p => p.UserPermissions)
                .HasForeignKey(up => up.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Permission>()
                .HasIndex(p => new { p.ModuleId, p.Name })
                .IsUnique();

            builder.Entity<Module>()
                .HasIndex(m => m.Name)
                .IsUnique();
            builder.Entity<JournalEntries>()
           .Property(x => x.IsPosted)
           .HasDefaultValue(true);
           
            builder.Entity<Stock>()
                .HasKey(x => new { x.StoreId,x.ProductId });

            builder.Entity<TransactionProducts>()
                .HasKey(s => new { s.ProductId,s.TransactionId });

            builder.Entity<Distributor_Merchant_Agent>().HasNoKey();

            builder.Entity<Previews>()
                .HasOne(p => p.WarrantCertificate)
                .WithOne(w => w.Previews)
                .HasForeignKey<WarrantCertificates>(w => w.PreviewId);

            builder.Entity<Distributor_Merchant_Agent>()
                .HasKey(dm => dm.UserId);

            base.OnModelCreating(builder);

            // ======== إصلاح مشكلة Multiple Cascade Paths ========
            builder.Entity<Previews>()
                .HasOne(p => p.Merchant)
                .WithMany()
                .HasForeignKey(p => p.MerchantID)
                .OnDelete(DeleteBehavior.Restrict); // تغيير من Cascade إلى Restrict

            builder.Entity<Previews>()
                .HasOne(p => p.Plumber)
                .WithMany()
                .HasForeignKey(p => p.PlumberID)
                .OnDelete(DeleteBehavior.Restrict); // تغيير من Cascade إلى Restrict

            builder.Entity<Previews>()
                .HasOne(p => p.Representative)
                .WithMany()
                .HasForeignKey(p => p.RepresentativeID)
                .OnDelete(DeleteBehavior.Restrict); 
            builder.Entity<Products>()
             .HasIndex(p => p.productCode)
             .IsUnique()     
             .HasDatabaseName("IX_Product_Code") 
             .IsClustered(false);

            // Non-unique non-clustered index على الاسم
            builder.Entity<Products>()
               .HasIndex(p => p.Name)
                .HasDatabaseName("IX_Product_Name")
                .IsClustered(false);
            builder.Entity<Previews>()
                .HasOne(p => p.City)
                .WithMany(c => c.Previews)
                .HasForeignKey(p => p.CityID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Employee>()
               .Property(e => e.EmployeeCode)
               .HasMaxLength(150);

            // ======== إصلاح الخانات العشرية ========

            // 1. جدول JournalEntryDetails
            builder.Entity<JournalEntryDetails>()
                .Property(j => j.Debit)
                .HasColumnType("decimal(18,2)");

            builder.Entity<JournalEntryDetails>()
                .Property(j => j.Credit)
                .HasColumnType("decimal(18,2)");

            // 2. جدول CollectionRepresentiveRate
            builder.Entity<CollectionRepresentiveRate>()
                .Property(c => c.Precentage)
                .HasColumnType("decimal(5,2)"); // نسبة مئوية (5,2) تكفي

            // 3. جدول EmployeeBonus
            builder.Entity<EmployeeBonus>()
                .Property(e => e.BonusAmount)
                .HasColumnType("decimal(18,2)");

            // 4. جدول EmployeeLeaveBalance
            builder.Entity<EmployeeLeaveBalance>()
                .Property(e => e.OpeningBalance)
                .HasColumnType("decimal(6,2)"); // أيام عادية تكفي 6,2

            builder.Entity<EmployeeLeaveBalance>()
                .Property(e => e.Accrued)
                .HasColumnType("decimal(6,2)");

            builder.Entity<EmployeeLeaveBalance>()
                .Property(e => e.Used)
                .HasColumnType("decimal(6,2)");

            builder.Entity<EmployeeLeaveBalance>()
                .Property(e => e.Remaining)
                .HasColumnType("decimal(6,2)");

            // 5. جدول EmployeeLeaveRequest
            builder.Entity<EmployeeLeaveRequest>()
                .Property(e => e.DaysRequested)
                .HasColumnType("decimal(6,2)");

            // 7. جدول Distributor_Merchant_Agent
            builder.Entity<Distributor_Merchant_Agent>()
                .Property(d => d.FirstSpecialDiscount)
                .HasColumnType("decimal(5,2)");

            builder.Entity<Distributor_Merchant_Agent>()
                .Property(d => d.SecondSpecialDiscount)
                .HasColumnType("decimal(5,2)");

            builder.Entity<Distributor_Merchant_Agent>()
                .Property(d => d.ThirdSpecialDiscount)
                .HasColumnType("decimal(5,2)");

            // 8. جدول EmployeeLoan (تأكد من وجود هذه الإصلاحات)
            builder.Entity<EmployeeLoan>()
                .Property(e => e.LoanAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<EmployeeLoan>()
                .Property(e => e.InstallmentAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<EmployeeLoan>()
                .Property(e => e.RemainingAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<EmployeeLoan>()
                .Property(e => e.PaidAmount)
                .HasColumnType("decimal(18,2)");

            // 9. جدول EmployeeLoanPayments
            builder.Entity<EmployeeLoanPayments>()
                .Property(e => e.PaymentAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<EmployeeLoanPayments>()
                .Property(e => e.RemainingAmount)
                .HasColumnType("decimal(18,2)");

            // 10. إصلاح مشكلة الواجهة بين WarrantCertificates و Previews
            builder.Entity<WarrantCertificates>()
                .HasOne(w => w.Previews)
                .WithOne(p => p.WarrantCertificate)
                .HasForeignKey<WarrantCertificates>(w => w.PreviewId)
                .OnDelete(DeleteBehavior.Cascade);

            // ======== إصلاح مشكلة Employees (لو في علاقات) ========
            builder.Entity<Employee>()
                .HasOne(e => e.Manager)
                .WithMany()
                .HasForeignKey(e => e.ManagerEmployeeCode)
                .OnDelete(DeleteBehavior.Restrict); // لمنع Cascade Paths

            builder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentID)
                .OnDelete(DeleteBehavior.Restrict);

            // ======== إصلاح مشكلة Plumbers (لأنها HasNoKey) ========
     

            builder.Entity<Supplier>(e =>
            {
                e.Property(s => s.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                e.Property(s => s.phoneNumbers)
                    .IsRequired()
                    .HasMaxLength(50);

                e.Property(s => s.address)
                    .HasMaxLength(500);

                // cityId is NULLABLE → the supplier can exist without a city.
                // Relationship: many suppliers → optional one city.
                // OnDelete = Restrict so deleting a city never cascades into suppliers.
                e.HasOne(s => s.city)
                 .WithMany()
                 .HasForeignKey(s => s.cityId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.Restrict);

                // Performance: these columns are filtered/sorted on in GetAllSuppliers
                e.HasIndex(s => s.Name);
                e.HasIndex(s => s.phoneNumbers);
                e.HasIndex(s => s.IsDeleted);
                e.HasIndex(s => s.cityId);
            });
            builder.Entity<Plumber>(e =>
            {
                e.Property(p => p.Name).IsRequired().HasMaxLength(200);
                e.Property(p => p.phoneNumbers).IsRequired().HasMaxLength(50);
                e.Property(p => p.address).HasMaxLength(500);
                e.Property(p => p.LicenseNumber).HasMaxLength(50);
                e.Property(p => p.Specialty).HasMaxLength(100);

                // Many plumbers → optional one city, same convention as Supplier.
                e.HasOne(p => p.city)
                 .WithMany()
                 .HasForeignKey(p => p.cityId)
                 .IsRequired(false)
                 .OnDelete(DeleteBehavior.Restrict);

                // Performance: filtered/sorted columns
                e.HasIndex(p => p.Name);
                e.HasIndex(p => p.phoneNumbers);
                e.HasIndex(p => p.IsDeleted);
                e.HasIndex(p => p.cityId);

                // Filtered unique index on LicenseNumber for ACTIVE plumbers only.
                // LicenseNumber is nullable — many plumbers can have NULL — but two
                // ACTIVE plumbers must never share the same non-null license number.
                // The service-level duplicate guard is the primary defence; this DB
                // constraint catches concurrent inserts that race past that guard.
                e.HasIndex(p => p.LicenseNumber)
                 .IsUnique()
                 .HasFilter("[LicenseNumber] IS NOT NULL AND [IsDeleted] = 0")
                 .HasDatabaseName("IX_Plumbers_LicenseNumber_ActiveUnique");
            });


            builder.Entity<JournalEntries>()
               .HasIndex(j => j.ReferenceNo)
               .HasDatabaseName("IX_JournalEntries_ReferenceNo");
            builder.Entity<Products>()
      .HasIndex(p => p.productCode)
      .IsUnique()
      .HasFilter("[productCode] IS NOT NULL")
      .HasDatabaseName("IX_Prroducts_ReferenceNo")
      .IsClustered(false);
            builder.Entity<ChartOfAccounts>()
    .HasIndex(x => x.AccountCode)
    .HasDatabaseName("IX_ChartOfAccounts_AccountCode");
            builder.Entity<ChartOfAccounts>()
 .HasIndex(x => x.UserId)
 .HasDatabaseName("IX_ChartOfAccounts_UserId");

            ConfigureAuditEntries(builder);
            ConfigureDocumentSequences(builder);
            ConfigureJournalPosting(builder);

            // Keep last: it must see every relationship configured above.
            DisableDatabaseCascades(builder);
        }

        /// <summary>
        /// Document number counters (F4, D31–D34): the type is the key (no identity), one seeded row per
        /// <see cref="DocumentType"/>. <c>DocumentNumberService</c> updates the row with raw SQL, so the
        /// table and column names are fixed here.
        /// </summary>
        private static void ConfigureDocumentSequences(ModelBuilder builder)
        {
            builder.Entity<DocumentSequence>(sequence =>
            {
                sequence.ToTable("DocumentSequences");
                sequence.HasKey(s => s.DocumentType);
                sequence.Property(s => s.DocumentType).ValueGeneratedNever();
                sequence.Property(s => s.Prefix).HasMaxLength(DocumentSequence.PrefixMaxLength);
                sequence.HasData(DocumentSequence.Defaults);
            });
        }

        /// <summary>
        /// Journal entries written by the posting engine (F6, D35–D38): the human number, the source
        /// document, and the reversal link. A filtered unique index on <c>ReversalOfEntryId</c> makes
        /// a second reversal of the same entry impossible even when two requests race.
        /// </summary>
        private static void ConfigureJournalPosting(ModelBuilder builder)
        {
            builder.Entity<JournalEntries>(entry =>
            {
                entry.Property(e => e.Desc).HasMaxLength(JournalEntries.DescriptionMaxLength);
                entry.Property(e => e.EntryNumber).HasMaxLength(JournalEntries.EntryNumberMaxLength);
                entry.Property(e => e.SourceType).HasConversion<byte?>();
                entry.Property(e => e.CreatedBy).HasMaxLength(450);
                entry.Property(e => e.UpdatedBy).HasMaxLength(450);

                entry.HasIndex(e => e.EntryNumber).IsUnique().HasFilter("[EntryNumber] IS NOT NULL");
                entry.HasIndex(e => new { e.SourceType, e.SourceId });
                entry.HasIndex(e => e.ReversalOfEntryId).IsUnique().HasFilter("[ReversalOfEntryId] IS NOT NULL");

                entry.HasOne(e => e.ReversalOf)
                    .WithMany()
                    .HasForeignKey(e => e.ReversalOfEntryId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<JournalEntryDetails>()
                .Property(d => d.Description)
                .HasMaxLength(JournalEntryDetails.DescriptionMaxLength);
        }

        /// <summary>
        /// The change log (D28–D30): indexes for screen 78's filters (time, user, area, document) and
        /// the append-only trigger, declared so EF Core doesn't use an OUTPUT clause on that table.
        /// </summary>
        private static void ConfigureAuditEntries(ModelBuilder builder)
        {
            builder.Entity<AuditEntry>(entry =>
            {
                entry.ToTable(table => table.HasTrigger(AuditEntry.AppendOnlyTrigger));

                entry.Property(a => a.UserName).HasMaxLength(AuditEntry.UserNameMaxLength);
                entry.Property(a => a.Area).HasMaxLength(AuditEntry.AreaMaxLength);
                entry.Property(a => a.EventKey).HasMaxLength(AuditEntry.EventKeyMaxLength);
                entry.Property(a => a.EntityType).HasMaxLength(AuditEntry.EntityTypeMaxLength);
                entry.Property(a => a.EntityId).HasMaxLength(AuditEntry.EntityIdMaxLength);
                entry.Property(a => a.Reference).HasMaxLength(AuditEntry.ReferenceMaxLength);
                entry.Property(a => a.Note).HasMaxLength(AuditEntry.NoteMaxLength);
                entry.Property(a => a.IpAddress).HasMaxLength(AuditEntry.IpAddressMaxLength);
                entry.Property(a => a.TraceId).HasMaxLength(AuditEntry.TraceIdMaxLength);

                entry.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entry.HasIndex(a => a.OccurredAt);
                entry.HasIndex(a => new { a.UserId, a.OccurredAt });
                entry.HasIndex(a => new { a.Area, a.OccurredAt });
                entry.HasIndex(a => new { a.EntityType, a.EntityId });
                entry.HasIndex(a => a.Reference);
            });
        }

        /// <summary>
        /// Stops the database from cascading deletes (D21): EF only deletes the dependents it has loaded.
        /// The permission links (UserPermission, Permission) keep their database cascade.
        /// </summary>
        private static void DisableDatabaseCascades(ModelBuilder builder)
        {
            var cascades = builder.Model.GetEntityTypes()
                .Where(entity => entity.ClrType != typeof(UserPermission) && entity.ClrType != typeof(Permission))
                .SelectMany(entity => entity.GetDeclaredForeignKeys())
                .Where(foreignKey => foreignKey.DeleteBehavior == DeleteBehavior.Cascade)
                .ToList();

            foreach (var foreignKey in cascades)
                foreignKey.DeleteBehavior = DeleteBehavior.ClientCascade;
        }

       

    }
         
}


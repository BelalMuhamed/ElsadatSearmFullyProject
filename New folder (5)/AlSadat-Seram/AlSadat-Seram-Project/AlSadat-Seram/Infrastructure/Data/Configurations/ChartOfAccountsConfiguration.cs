using Domain.Entities.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public sealed class ChartOfAccountsConfiguration : IEntityTypeConfiguration<ChartOfAccounts>
    {
        public void Configure(EntityTypeBuilder<ChartOfAccounts> b)
        {
            b.ToTable("Accounts");
            b.HasKey(x => x.Id);

            b.Property(x => x.AccountCode).IsRequired().HasMaxLength(50);
            b.Property(x => x.AccountName).IsRequired().HasMaxLength(200);
            b.Property(x => x.IsSystemAccount).HasDefaultValue(false);
            b.Property(x => x.SystemCode).HasConversion<int?>();

            b.HasIndex(x => x.AccountCode).IsUnique();
            b.HasIndex(x => x.SystemCode).IsUnique().HasFilter("[SystemCode] IS NOT NULL");
            b.HasIndex(x => x.ParentAccountId);
            b.HasIndex(x => x.UserId);
        }
    }
}



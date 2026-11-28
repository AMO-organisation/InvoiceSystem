using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InvoiceSystem.Domain.Entities;

namespace InvoiceSystem.Infrastructure.Data.Configurations;

public class CompanyProfileConfiguration : IEntityTypeConfiguration<CompanyProfile>
{
    public void Configure(EntityTypeBuilder<CompanyProfile> builder)
    {
        builder.HasKey(cp => cp.Id);

        builder.Property(cp => cp.CompanyName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(cp => cp.Address)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(cp => cp.City)
            .HasMaxLength(100);

        builder.Property(cp => cp.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(cp => cp.Phone)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(cp => cp.CurrencySymbol)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("₦");

        builder.Property(cp => cp.DefaultTaxRate)
            .HasColumnType("decimal(5,2)")
            .HasDefaultValue(7.50m);

        builder.Property(cp => cp.PaymentInstructions)
            .HasMaxLength(500);
    }
}

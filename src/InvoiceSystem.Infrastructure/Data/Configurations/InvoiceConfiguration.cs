using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InvoiceSystem.Domain.Entities;

namespace InvoiceSystem.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasIndex(i => i.InvoiceNumber)
            .IsUnique();

        builder.Property(i => i.Subtotal)
            .HasColumnType("decimal(18,2)");

        builder.Property(i => i.DiscountAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(i => i.TaxRate)
            .HasColumnType("decimal(5,2)");

        builder.Property(i => i.TaxAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(i => i.GrandTotal)
            .HasColumnType("decimal(18,2)");

        builder.Property(i => i.Notes)
            .HasMaxLength(1000);

        builder.HasOne(i => i.Customer)
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

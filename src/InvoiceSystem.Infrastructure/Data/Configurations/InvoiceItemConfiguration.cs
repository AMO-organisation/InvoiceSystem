using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using InvoiceSystem.Domain.Entities;

namespace InvoiceSystem.Infrastructure.Data.Configurations;

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.HasKey(ii => ii.Id);

        builder.Property(ii => ii.ItemName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(ii => ii.Description)
            .HasMaxLength(300);

        builder.Property(ii => ii.UnitPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(ii => ii.LineTotal)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(ii => ii.Invoice)
            .WithMany(i => i.Items)
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ii => ii.Product)
            .WithMany()
            .HasForeignKey(ii => ii.ProductId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

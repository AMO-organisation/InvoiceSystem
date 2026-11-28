using InvoiceSystem.Domain.Enums;

namespace InvoiceSystem.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? Description { get; set; }
    public CatalogItemType Type { get; set; } = CatalogItemType.Product;
    public decimal UnitPrice { get; set; }
    public bool IsTaxable { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

namespace InvoiceSystem.Domain.Entities;

public class InvoiceItem
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsTaxable { get; set; }
    public decimal LineTotal { get; set; }
}

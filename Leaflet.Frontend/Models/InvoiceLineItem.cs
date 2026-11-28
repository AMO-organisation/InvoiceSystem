namespace Leaflet.Frontend.Models;

public sealed class InvoiceLineItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? ProductId { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Unit { get; set; } = "each";

    public decimal Quantity { get; set; } = 1;

    public decimal UnitPrice { get; set; }

    public decimal TaxRate { get; set; }

    public InvoiceLineItem Clone() => (InvoiceLineItem)MemberwiseClone();
}

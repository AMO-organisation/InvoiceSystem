namespace Leaflet.Frontend.Models;

public sealed class CompanySettings
{
    public string CompanyName { get; set; } = "Leaflet Studio";
    public Address Address { get; set; } = new();
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? LogoUrl { get; set; }
    public string? TaxId { get; set; }

    public string CurrencyCode { get; set; } = "NGN";
    public string CurrencySymbol { get; set; } = "\u20a6";
    public decimal DefaultTaxRate { get; set; } = 10m;
    public string InvoicePrefix { get; set; } = "INV";
    public int PaymentTermsDays { get; set; } = 30;
    public string FooterText { get; set; } = "Thank you for your business.";

    public CompanySettings Clone() => new()
    {
        CompanyName = CompanyName,
        Address = Address.Clone(),
        Email = Email,
        Phone = Phone,
        LogoUrl = LogoUrl,
        TaxId = TaxId,
        CurrencyCode = CurrencyCode,
        CurrencySymbol = CurrencySymbol,
        DefaultTaxRate = DefaultTaxRate,
        InvoicePrefix = InvoicePrefix,
        PaymentTermsDays = PaymentTermsDays,
        FooterText = FooterText
    };
}

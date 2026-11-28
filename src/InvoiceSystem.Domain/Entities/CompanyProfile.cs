namespace InvoiceSystem.Domain.Entities;

public class CompanyProfile
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = "₦";
    public decimal DefaultTaxRate { get; set; } = 7.50m;
    public string? PaymentInstructions { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

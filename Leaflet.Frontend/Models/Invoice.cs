namespace Leaflet.Frontend.Models;

public sealed class CustomerSnapshot
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public Address BillingAddress { get; set; } = new();
    public string? TaxId { get; set; }

    public static CustomerSnapshot FromCustomer(Customer customer) => new()
    {
        Name = customer.Name,
        Email = customer.Email,
        Phone = customer.Phone,
        BillingAddress = customer.BillingAddress.Clone(),
        TaxId = customer.TaxId
    };

    public CustomerSnapshot Clone() => new()
    {
        Name = Name,
        Email = Email,
        Phone = Phone,
        BillingAddress = BillingAddress.Clone(),
        TaxId = TaxId
    };
}

public sealed class Invoice
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Backend (database) primary key. Used to call backend endpoints such as /api/invoices/{id}/pdf.
    public int? BackendId { get; set; }

    public string? Number { get; set; }

    public Guid CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public CustomerSnapshot? CustomerSnapshot { get; set; }

    public DateOnly IssueDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public DateOnly DueDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(30));

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    public List<InvoiceLineItem> LineItems { get; set; } = new();

    public decimal DiscountPercent { get; set; }

    public string? Notes { get; set; }

    public string? PaymentTerms { get; set; }

    public decimal AmountPaid { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Invoice Clone() => new()
    {
        Id = Id,
        BackendId = BackendId,
        Number = Number,
        CustomerId = CustomerId,
        CustomerName = CustomerName,
        CustomerSnapshot = CustomerSnapshot?.Clone(),
        IssueDate = IssueDate,
        DueDate = DueDate,
        Status = Status,
        LineItems = LineItems.Select(line => line.Clone()).ToList(),
        DiscountPercent = DiscountPercent,
        Notes = Notes,
        PaymentTerms = PaymentTerms,
        AmountPaid = AmountPaid,
        CreatedAt = CreatedAt,
        UpdatedAt = UpdatedAt
    };
}

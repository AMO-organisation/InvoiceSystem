namespace Leaflet.Frontend.Contracts;

public enum CatalogItemType
{
    Product = 1,
    Service = 2
}

public enum InvoiceStatus
{
    Draft = 0,
    Unpaid = 1,
    Paid = 2,
    Overdue = 3,
    Cancelled = 4
}

public record CustomerDto(
    int Id,
    string FullName,
    string? CompanyName,
    string Email,
    string Phone,
    string? Address,
    string? City,
    bool IsDeleted,
    DateTime CreatedAt
);

public record CreateCustomerDto(
    string FullName,
    string? CompanyName,
    string Email,
    string Phone,
    string? Address,
    string? City
);

public record ProductDto(
    int Id,
    string Name,
    string? Sku,
    string? Description,
    CatalogItemType Type,
    decimal UnitPrice,
    bool IsTaxable,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateProductDto(
    string Name,
    string? Sku,
    string? Description,
    CatalogItemType Type,
    decimal UnitPrice,
    bool IsTaxable
);

public record InvoiceDto(
    int Id,
    string InvoiceNumber,
    int CustomerId,
    string CustomerName,
    string CustomerEmail,
    DateTime IssueDate,
    DateTime DueDate,
    InvoiceStatus Status,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TaxRate,
    decimal TaxAmount,
    decimal GrandTotal,
    string? Notes,
    DateTime CreatedAt,
    DateTime? PaidAt
);

public record InvoiceItemDto(
    int Id,
    int? ProductId,
    string ItemName,
    string? Description,
    int Quantity,
    decimal UnitPrice,
    bool IsTaxable,
    decimal LineTotal
);

public record InvoiceDetailDto(
    int Id,
    string InvoiceNumber,
    CustomerDto Customer,
    DateTime IssueDate,
    DateTime DueDate,
    InvoiceStatus Status,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TaxRate,
    decimal TaxAmount,
    decimal GrandTotal,
    string? Notes,
    DateTime CreatedAt,
    DateTime? PaidAt,
    List<InvoiceItemDto> Items
);

public record CreateInvoiceItemDto(
    int? ProductId,
    string? ItemName,
    string? Description,
    int Quantity,
    decimal? UnitPrice,
    bool? IsTaxable
);

public record CreateInvoiceDto(
    int CustomerId,
    DateTime IssueDate,
    DateTime DueDate,
    decimal DiscountAmount,
    decimal? TaxRate,
    string? Notes,
    List<CreateInvoiceItemDto> Items
);

public record UpdateInvoiceDto(
    int CustomerId,
    DateTime IssueDate,
    DateTime DueDate,
    decimal DiscountAmount,
    decimal TaxRate,
    string? Notes,
    List<CreateInvoiceItemDto> Items
);

public record CompanyProfileDto(
    int Id,
    string CompanyName,
    string Address,
    string? City,
    string Email,
    string Phone,
    string CurrencySymbol,
    decimal DefaultTaxRate,
    string? PaymentInstructions,
    DateTime UpdatedAt
);

public record UpdateCompanyProfileRequest(
    string CompanyName,
    string Address,
    string? City,
    string Email,
    string Phone,
    string CurrencySymbol,
    decimal DefaultTaxRate,
    string? PaymentInstructions
);

public record DashboardSummaryDto(
    int TotalInvoices,
    int PaidCount,
    int UnpaidCount,
    int OverdueCount,
    decimal TotalRevenue,
    decimal TotalReceivables,
    List<InvoiceDto> RecentInvoices
);

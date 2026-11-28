using InvoiceSystem.Domain.Enums;

namespace InvoiceSystem.Application.DTOs;

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

public record UpdateInvoiceStatusDto(
    InvoiceStatus Status
);

public record CalculatePreviewDto(
    decimal DiscountAmount,
    decimal TaxRate,
    List<CreateInvoiceItemDto> Items
);

public record InvoicePreviewResultDto(
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TaxRate,
    decimal TaxAmount,
    decimal GrandTotal,
    List<InvoiceItemDto> Items
);

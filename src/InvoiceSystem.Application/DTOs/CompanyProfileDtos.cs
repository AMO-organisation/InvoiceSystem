namespace InvoiceSystem.Application.DTOs;

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

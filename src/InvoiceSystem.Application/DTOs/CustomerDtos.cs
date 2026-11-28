namespace InvoiceSystem.Application.DTOs;

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

public record UpdateCustomerDto(
    string FullName,
    string? CompanyName,
    string Email,
    string Phone,
    string? Address,
    string? City
);

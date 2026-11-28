using InvoiceSystem.Domain.Enums;

namespace InvoiceSystem.Application.DTOs;

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

public record UpdateProductDto(
    string Name,
    string? Sku,
    string? Description,
    CatalogItemType Type,
    decimal UnitPrice,
    bool IsTaxable,
    bool IsActive
);

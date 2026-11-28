using InvoiceSystem.Domain.Entities;

namespace InvoiceSystem.Domain.Interfaces;

public record InvoiceCalculationResult(
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TaxRate,
    decimal TaxAmount,
    decimal GrandTotal,
    List<InvoiceItemCalculated> Items
);

public record InvoiceItemCalculated(
    int? ProductId,
    string ItemName,
    string? Description,
    int Quantity,
    decimal UnitPrice,
    bool IsTaxable,
    decimal LineTotal
);

public interface IInvoiceCalculator
{
    InvoiceCalculationResult Calculate(IEnumerable<InvoiceItemInput> items, decimal discountAmount, decimal taxRate);
}

public record InvoiceItemInput(
    int? ProductId,
    string ItemName,
    string? Description,
    int Quantity,
    decimal UnitPrice,
    bool IsTaxable
);

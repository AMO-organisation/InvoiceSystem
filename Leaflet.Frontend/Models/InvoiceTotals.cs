namespace Leaflet.Frontend.Models;

public sealed record LineTotal(
    Guid LineItemId,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TaxableAmount,
    decimal TaxAmount,
    decimal Total);

public sealed record InvoiceTotals(
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TaxTotal,
    decimal GrandTotal,
    decimal AmountPaid,
    decimal BalanceDue,
    IReadOnlyList<LineTotal> Lines);

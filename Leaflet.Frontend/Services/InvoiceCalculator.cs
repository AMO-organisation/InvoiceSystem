using Leaflet.Frontend.Models;

namespace Leaflet.Frontend.Services;

public static class InvoiceCalculator
{
    public static InvoiceTotals Calculate(Invoice invoice)
    {
        var rawLines = invoice.LineItems
            .Select(line => new
            {
                Line = line,
                Subtotal = Round(line.Quantity * line.UnitPrice)
            })
            .ToList();

        var subtotal = Round(rawLines.Sum(line => line.Subtotal));
        var discountPercent = Math.Clamp(invoice.DiscountPercent, 0m, 100m);
        var discountAmount = Round(subtotal * discountPercent / 100m);

        var remainingDiscount = discountAmount;
        var lineTotals = new List<LineTotal>(rawLines.Count);

        for (var index = 0; index < rawLines.Count; index++)
        {
            var line = rawLines[index];
            var isLast = index == rawLines.Count - 1;

            var maxDiscount = Math.Min(line.Subtotal, Math.Max(remainingDiscount, 0m));
            var lineDiscount = isLast
                ? maxDiscount
                : subtotal == 0m
                    ? 0m
                    : Round(discountAmount * line.Subtotal / subtotal);

            lineDiscount = Math.Clamp(lineDiscount, 0m, maxDiscount);
            remainingDiscount -= lineDiscount;

            var taxableAmount = Round(line.Subtotal - lineDiscount);
            var taxRate = Math.Clamp(line.Line.TaxRate, 0m, 100m);
            var taxAmount = Round(taxableAmount * taxRate / 100m);

            lineTotals.Add(new LineTotal(
                line.Line.Id,
                line.Subtotal,
                lineDiscount,
                taxableAmount,
                taxAmount,
                Round(taxableAmount + taxAmount)));
        }

        var taxTotal = Round(lineTotals.Sum(line => line.TaxAmount));
        var grandTotal = Round(subtotal - discountAmount + taxTotal);
        var amountPaid = Round(invoice.AmountPaid);

        return new InvoiceTotals(
            subtotal,
            discountAmount,
            taxTotal,
            grandTotal,
            amountPaid,
            Round(grandTotal - amountPaid),
            lineTotals);
    }

    public static decimal LineSubtotal(InvoiceLineItem line) =>
        Round(line.Quantity * line.UnitPrice);

    public static decimal Round(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}

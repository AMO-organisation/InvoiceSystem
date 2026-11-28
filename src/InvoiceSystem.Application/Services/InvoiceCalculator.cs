using InvoiceSystem.Domain.Interfaces;

namespace InvoiceSystem.Application.Services;

public class InvoiceCalculator : IInvoiceCalculator
{
    public InvoiceCalculationResult Calculate(IEnumerable<InvoiceItemInput> items, decimal discountAmount, decimal taxRate)
    {
        var calculatedItems = new List<InvoiceItemCalculated>();
        decimal subtotal = 0m;
        decimal taxableSum = 0m;

        foreach (var item in items)
        {
            var lineTotal = Math.Round(item.Quantity * item.UnitPrice, 2, MidpointRounding.AwayFromZero);
            subtotal += lineTotal;

            if (item.IsTaxable)
            {
                taxableSum += lineTotal;
            }

            calculatedItems.Add(new InvoiceItemCalculated(
                item.ProductId,
                item.ItemName,
                item.Description,
                item.Quantity,
                item.UnitPrice,
                item.IsTaxable,
                lineTotal
            ));
        }

        var taxableBase = Math.Max(0m, taxableSum - discountAmount);
        var taxAmount = Math.Round(taxableBase * (taxRate / 100m), 2, MidpointRounding.AwayFromZero);
        var grandTotal = Math.Round(subtotal - discountAmount + taxAmount, 2, MidpointRounding.AwayFromZero);

        return new InvoiceCalculationResult(
            Subtotal: subtotal,
            DiscountAmount: discountAmount,
            TaxRate: taxRate,
            TaxAmount: taxAmount,
            GrandTotal: grandTotal,
            Items: calculatedItems
        );
    }
}

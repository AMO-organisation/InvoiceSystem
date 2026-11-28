using InvoiceSystem.Application.Services;
using InvoiceSystem.Domain.Interfaces;
using Xunit;

namespace InvoiceSystem.Tests;

public class InvoiceCalculatorTests
{
    private readonly InvoiceCalculator _calculator = new();

    [Fact]
    public void Calculate_ShouldComputeCorrectSubtotalTaxAndGrandTotal()
    {
        var items = new List<InvoiceItemInput>
        {
            new(null, "Item 1", "Desc 1", 2, 50.00m, true),    // 100.00 taxable
            new(null, "Item 2", "Desc 2", 1, 30.00m, false)    // 30.00 non-taxable
        };

        var result = _calculator.Calculate(items, discountAmount: 10.00m, taxRate: 7.50m);

        Assert.Equal(130.00m, result.Subtotal);
        Assert.Equal(10.00m, result.DiscountAmount);
        Assert.Equal(6.75m, result.TaxAmount);
        Assert.Equal(126.75m, result.GrandTotal);
    }

    [Fact]
    public void Calculate_WithNoTaxableItems_ShouldHaveZeroTax()
    {
        var items = new List<InvoiceItemInput>
        {
            new(null, "Exempt Item", null, 5, 20.00m, false) // 100.00 non-taxable
        };

        var result = _calculator.Calculate(items, discountAmount: 0m, taxRate: 10.00m);

        Assert.Equal(100.00m, result.Subtotal);
        Assert.Equal(0.00m, result.TaxAmount);
        Assert.Equal(100.00m, result.GrandTotal);
    }
}

using FluentValidation.TestHelper;
using InvoiceSystem.Application.DTOs;
using InvoiceSystem.Application.Validators;
using InvoiceSystem.Domain.Enums;
using Xunit;

namespace InvoiceSystem.Tests;

public class ValidationTests
{
    [Fact]
    public void CustomerValidator_ShouldHaveError_WhenEmailIsInvalid()
    {
        var validator = new CreateCustomerDtoValidator();
        var dto = new CreateCustomerDto("John Doe", "Acme", "invalid-email", "1234567890", "123 St", "City");

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ProductValidator_ShouldHaveError_WhenUnitPriceIsNegative()
    {
        var validator = new CreateProductDtoValidator();
        var dto = new CreateProductDto("Product 1", "SKU1", "Desc", CatalogItemType.Product, -10.00m, true);

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.UnitPrice);
    }

    [Fact]
    public void InvoiceValidator_ShouldHaveError_WhenItemsAreEmpty()
    {
        var validator = new CreateInvoiceDtoValidator();
        var dto = new CreateInvoiceDto(1, DateTime.UtcNow, DateTime.UtcNow.AddDays(10), 0m, 7.50m, null, new());

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Fact]
    public void InvoiceValidator_ShouldHaveError_WhenDueDateBeforeIssueDate()
    {
        var validator = new CreateInvoiceDtoValidator();
        var dto = new CreateInvoiceDto(
            1,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(-1), // Due date before issue date
            0m,
            7.50m,
            null,
            new List<CreateInvoiceItemDto> { new(null, "Item", null, 1, 100m, true) }
        );

        var result = validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.DueDate);
    }
}

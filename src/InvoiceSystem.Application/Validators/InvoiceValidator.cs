using FluentValidation;
using InvoiceSystem.Application.DTOs;

namespace InvoiceSystem.Application.Validators;

public class CreateInvoiceItemDtoValidator : AbstractValidator<CreateInvoiceItemDto>
{
    public CreateInvoiceItemDtoValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(1).WithMessage("Quantity must be at least 1.");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).When(x => x.UnitPrice.HasValue)
            .WithMessage("Unit price must be greater than or equal to 0.");

        RuleFor(x => x.ItemName)
            .NotEmpty().When(x => !x.ProductId.HasValue)
            .WithMessage("Item name is required if product is not selected.");
    }
}

public class CreateInvoiceDtoValidator : AbstractValidator<CreateInvoiceDto>
{
    public CreateInvoiceDtoValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Customer ID is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one line item is required.");

        RuleForEach(x => x.Items).SetValidator(new CreateInvoiceItemDtoValidator());

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount amount cannot be negative.");

        RuleFor(x => x.TaxRate)
            .InclusiveBetween(0, 100).WithMessage("Tax rate must be between 0 and 100.");

        RuleFor(x => x.DueDate)
            .Must((dto, dueDate) => dueDate.Date >= dto.IssueDate.Date)
            .WithMessage("Due date must be on or after issue date.");
    }
}

public class UpdateInvoiceDtoValidator : AbstractValidator<UpdateInvoiceDto>
{
    public UpdateInvoiceDtoValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Customer ID is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one line item is required.");

        RuleForEach(x => x.Items).SetValidator(new CreateInvoiceItemDtoValidator());

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount amount cannot be negative.");

        RuleFor(x => x.TaxRate)
            .InclusiveBetween(0, 100).WithMessage("Tax rate must be between 0 and 100.");

        RuleFor(x => x.DueDate)
            .Must((dto, dueDate) => dueDate.Date >= dto.IssueDate.Date)
            .WithMessage("Due date must be on or after issue date.");
    }
}

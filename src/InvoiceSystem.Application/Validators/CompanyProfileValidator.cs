using FluentValidation;
using InvoiceSystem.Application.DTOs;

namespace InvoiceSystem.Application.Validators;

public class UpdateCompanyProfileRequestValidator : AbstractValidator<UpdateCompanyProfileRequest>
{
    public UpdateCompanyProfileRequestValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(150).WithMessage("Company name must not exceed 150 characters.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(300).WithMessage("Address must not exceed 300 characters.");

        RuleFor(x => x.City)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(150).WithMessage("Email must not exceed 150 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .MaximumLength(30).WithMessage("Phone must not exceed 30 characters.");

        RuleFor(x => x.CurrencySymbol)
            .NotEmpty().WithMessage("Currency symbol is required.")
            .MaximumLength(10).WithMessage("Currency symbol must not exceed 10 characters.");

        RuleFor(x => x.DefaultTaxRate)
            .InclusiveBetween(0, 100).WithMessage("Default tax rate must be between 0 and 100.");

        RuleFor(x => x.PaymentInstructions)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.PaymentInstructions));
    }
}

namespace InvoiceSystem.Domain.Interfaces;

public interface IInvoiceNumberGenerator
{
    Task<string> GenerateNextInvoiceNumberAsync();
}

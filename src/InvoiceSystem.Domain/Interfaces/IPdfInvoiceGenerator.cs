using InvoiceSystem.Domain.Entities;

namespace InvoiceSystem.Domain.Interfaces;

public interface IPdfInvoiceGenerator
{
    Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice);
}

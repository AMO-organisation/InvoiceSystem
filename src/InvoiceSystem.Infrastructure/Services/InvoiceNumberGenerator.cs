using Microsoft.EntityFrameworkCore;
using InvoiceSystem.Domain.Interfaces;
using InvoiceSystem.Infrastructure.Data;

namespace InvoiceSystem.Infrastructure.Services;

public class InvoiceNumberGenerator : IInvoiceNumberGenerator
{
    private readonly AppDbContext _context;

    public InvoiceNumberGenerator(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateNextInvoiceNumberAsync()
    {
        int maxRetries = 5;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var year = DateTime.UtcNow.Year;
                var highestSeq = await GetHighestSequenceNumberForYearAsync(year);
                var nextSeq = highestSeq + 1;
                var invoiceNumber = $"INV-{year}-{nextSeq:D4}";

                await transaction.CommitAsync();
                return invoiceNumber;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                if (attempt == maxRetries - 1) throw;
                await Task.Delay(50 * (attempt + 1));
            }
        }

        throw new InvalidOperationException("Failed to generate unique invoice number after multiple retries.");
    }

    private async Task<int> GetHighestSequenceNumberForYearAsync(int year)
    {
        var prefix = $"INV-{year}-";
        var matchingInvoices = await _context.Invoices
            .Where(i => i.InvoiceNumber.StartsWith(prefix))
            .Select(i => i.InvoiceNumber)
            .ToListAsync();

        int maxSeq = 0;
        foreach (var invNum in matchingInvoices)
        {
            var parts = invNum.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out var seq))
            {
                if (seq > maxSeq) maxSeq = seq;
            }
        }

        return maxSeq;
    }
}

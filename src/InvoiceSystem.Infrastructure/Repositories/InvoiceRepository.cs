using Microsoft.EntityFrameworkCore;
using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Enums;
using InvoiceSystem.Domain.Interfaces;
using InvoiceSystem.Infrastructure.Data;

namespace InvoiceSystem.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly AppDbContext _context;

    public InvoiceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice?> GetByIdAsync(int id)
    {
        return await _context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Items)
            .ThenInclude(it => it.Product)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync(InvoiceStatus? status = null, string? search = null, int? customerId = null)
    {
        var query = _context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Items)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(i => i.CustomerId == customerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(i =>
                i.InvoiceNumber.ToLower().Contains(term) ||
                i.Customer.FullName.ToLower().Contains(term) ||
                (i.Customer.CompanyName != null && i.Customer.CompanyName.ToLower().Contains(term)) ||
                i.Customer.Email.ToLower().Contains(term)
            );
        }

        return await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
    }

    public async Task<Invoice> AddAsync(Invoice invoice)
    {
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task UpdateAsync(Invoice invoice)
    {
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetHighestSequenceNumberForYearAsync(int year)
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

using Leaflet.Frontend.Models;
using Leaflet.Frontend.Services.Abstractions;

namespace Leaflet.Frontend.Services.InMemory;

public sealed class InMemoryInvoiceService(
    ICustomerService customers,
    IInvoiceNumberService numbers) : IInvoiceService
{
    private readonly List<Invoice> _invoices = new();
    private readonly SemaphoreSlim _finalizeLock = new(1, 1);

    public Task<IReadOnlyList<Invoice>> GetAllAsync()
    {
        IReadOnlyList<Invoice> result = _invoices
            .OrderByDescending(invoice => invoice.UpdatedAt)
            .Select(invoice => invoice.Clone())
            .ToList();

        return Task.FromResult(result);
    }

    public Task<Invoice?> GetByIdAsync(Guid id)
    {
        var invoice = _invoices.FirstOrDefault(item => item.Id == id);
        return Task.FromResult(invoice?.Clone());
    }

    public Task<Invoice> AddAsync(Invoice invoice)
    {
        var copy = invoice.Clone();
        copy.Id = Guid.NewGuid();
        copy.Number = null;
        copy.Status = InvoiceStatus.Draft;
        copy.AmountPaid = 0m;
        copy.CustomerSnapshot = null;
        copy.CreatedAt = DateTime.UtcNow;
        copy.UpdatedAt = DateTime.UtcNow;

        foreach (var line in copy.LineItems)
        {
            if (line.Id == Guid.Empty)
            {
                line.Id = Guid.NewGuid();
            }
        }

        _invoices.Add(copy);
        return Task.FromResult(copy.Clone());
    }

    public Task UpdateAsync(Invoice invoice)
    {
        var index = _invoices.FindIndex(item => item.Id == invoice.Id);
        if (index < 0)
        {
            throw new KeyNotFoundException("Invoice not found.");
        }

        if (_invoices[index].Status != InvoiceStatus.Draft)
        {
            throw new InvalidOperationException("Only draft invoices can be edited.");
        }

        var copy = invoice.Clone();
        copy.Status = InvoiceStatus.Draft;
        copy.Number = null;
        copy.AmountPaid = 0m;
        copy.CustomerSnapshot = null;
        copy.CreatedAt = _invoices[index].CreatedAt;
        copy.UpdatedAt = DateTime.UtcNow;

        foreach (var line in copy.LineItems)
        {
            if (line.Id == Guid.Empty)
            {
                line.Id = Guid.NewGuid();
            }
        }

        _invoices[index] = copy;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        var invoice = _invoices.FirstOrDefault(item => item.Id == id);
        if (invoice is null)
        {
            throw new KeyNotFoundException("Invoice not found.");
        }

        if (invoice.Status != InvoiceStatus.Draft)
        {
            throw new InvalidOperationException("Only draft invoices can be deleted.");
        }

        _invoices.Remove(invoice);
        return Task.CompletedTask;
    }

    public async Task<Invoice> FinalizeAsync(Guid id)
    {
        await _finalizeLock.WaitAsync();
        try
        {
            var invoice = _invoices.FirstOrDefault(item => item.Id == id)
                ?? throw new KeyNotFoundException("Invoice not found.");

            if (invoice.Status != InvoiceStatus.Draft)
            {
                throw new InvalidOperationException("This invoice has already been finalized.");
            }

            ValidateForFinalize(invoice);

            var customer = await customers.GetByIdAsync(invoice.CustomerId)
                ?? throw new InvalidOperationException("The selected customer no longer exists.");

            invoice.Number = await numbers.NextNumberAsync(invoice.IssueDate.Year);
            invoice.CustomerName = customer.Name;
            invoice.CustomerSnapshot = CustomerSnapshot.FromCustomer(customer);
            invoice.Status = InvoiceStatus.Issued;
            invoice.UpdatedAt = DateTime.UtcNow;

            return invoice.Clone();
        }
        finally
        {
            _finalizeLock.Release();
        }
    }

    public Task MarkPaidAsync(Guid id)
    {
        var invoice = _invoices.FirstOrDefault(item => item.Id == id)
            ?? throw new KeyNotFoundException("Invoice not found.");

        if (invoice.Status is InvoiceStatus.Draft or InvoiceStatus.Void)
        {
            throw new InvalidOperationException("Only issued invoices can be marked as paid.");
        }

        var totals = InvoiceCalculator.Calculate(invoice);
        invoice.AmountPaid = totals.GrandTotal;
        invoice.Status = InvoiceStatus.Paid;
        invoice.UpdatedAt = DateTime.UtcNow;

        return Task.CompletedTask;
    }

    public Task VoidAsync(Guid id)
    {
        var invoice = _invoices.FirstOrDefault(item => item.Id == id)
            ?? throw new KeyNotFoundException("Invoice not found.");

        if (invoice.Status == InvoiceStatus.Void)
        {
            return Task.CompletedTask;
        }

        invoice.Status = InvoiceStatus.Void;
        invoice.UpdatedAt = DateTime.UtcNow;

        return Task.CompletedTask;
    }

    public Task<Invoice> DuplicateAsync(Guid id)
    {
        var source = _invoices.FirstOrDefault(item => item.Id == id)
            ?? throw new KeyNotFoundException("Invoice not found.");

        var today = DateOnly.FromDateTime(DateTime.Today);
        var originalTerm = source.DueDate.DayNumber - source.IssueDate.DayNumber;

        var copy = new Invoice
        {
            Id = Guid.NewGuid(),
            Number = null,
            CustomerId = source.CustomerId,
            CustomerName = source.CustomerName,
            CustomerSnapshot = null,
            IssueDate = today,
            DueDate = today.AddDays(Math.Max(originalTerm, 0)),
            Status = InvoiceStatus.Draft,
            DiscountPercent = source.DiscountPercent,
            Notes = source.Notes,
            PaymentTerms = source.PaymentTerms,
            AmountPaid = 0m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LineItems = source.LineItems.Select(line =>
            {
                var clone = line.Clone();
                clone.Id = Guid.NewGuid();
                return clone;
            }).ToList()
        };

        _invoices.Add(copy);
        return Task.FromResult(copy.Clone());
    }

    public Task<int> CountForCustomerAsync(Guid customerId)
    {
        var count = _invoices.Count(invoice => invoice.CustomerId == customerId);
        return Task.FromResult(count);
    }

    private static void ValidateForFinalize(Invoice invoice)
    {
        if (invoice.CustomerId == Guid.Empty)
        {
            throw new InvalidOperationException("Select a customer before finalizing.");
        }

        if (invoice.DueDate < invoice.IssueDate)
        {
            throw new InvalidOperationException("The due date cannot be before the issue date.");
        }

        if (invoice.LineItems.Count == 0)
        {
            throw new InvalidOperationException("Add at least one line item before finalizing.");
        }

        foreach (var line in invoice.LineItems)
        {
            if (string.IsNullOrWhiteSpace(line.Description))
            {
                throw new InvalidOperationException("Every line item needs a description.");
            }

            if (line.Quantity <= 0)
            {
                throw new InvalidOperationException("Every line item needs a quantity greater than zero.");
            }

            if (line.UnitPrice < 0)
            {
                throw new InvalidOperationException("Unit price cannot be negative.");
            }

            if (line.TaxRate is < 0 or > 100)
            {
                throw new InvalidOperationException("Tax rates must be between 0 and 100.");
            }
        }

        if (invoice.DiscountPercent is < 0 or > 100)
        {
            throw new InvalidOperationException("Discount must be between 0 and 100.");
        }
    }
}

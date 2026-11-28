using Leaflet.Frontend.Models;

namespace Leaflet.Frontend.Services.Abstractions;

public interface IInvoiceService
{
    Task<IReadOnlyList<Invoice>> GetAllAsync();
    Task<Invoice?> GetByIdAsync(Guid id);
    Task<Invoice> AddAsync(Invoice invoice);
    Task UpdateAsync(Invoice invoice);
    Task DeleteAsync(Guid id);
    Task<Invoice> FinalizeAsync(Guid id);
    Task MarkPaidAsync(Guid id);
    Task VoidAsync(Guid id);
    Task<Invoice> DuplicateAsync(Guid id);
    Task<int> CountForCustomerAsync(Guid customerId);
}

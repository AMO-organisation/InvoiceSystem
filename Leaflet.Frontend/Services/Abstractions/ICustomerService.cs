using Leaflet.Frontend.Models;

namespace Leaflet.Frontend.Services.Abstractions;

public interface ICustomerService
{
    Task<IReadOnlyList<Customer>> GetAllAsync();
    Task<Customer?> GetByIdAsync(Guid id);
    Task<Customer> AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task<bool> DeleteAsync(Guid id);
}

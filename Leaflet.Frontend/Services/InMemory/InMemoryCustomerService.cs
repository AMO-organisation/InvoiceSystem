using Leaflet.Frontend.Models;
using Leaflet.Frontend.Services.Abstractions;

namespace Leaflet.Frontend.Services.InMemory;

public sealed class InMemoryCustomerService : ICustomerService
{
    private readonly List<Customer> _customers = new();

    public Task<IReadOnlyList<Customer>> GetAllAsync()
    {
        IReadOnlyList<Customer> result = _customers
            .OrderBy(customer => customer.Name, StringComparer.OrdinalIgnoreCase)
            .Select(customer => customer.Clone())
            .ToList();

        return Task.FromResult(result);
    }

    public Task<Customer?> GetByIdAsync(Guid id)
    {
        var customer = _customers.FirstOrDefault(item => item.Id == id);
        return Task.FromResult(customer?.Clone());
    }

    public Task<Customer> AddAsync(Customer customer)
    {
        var copy = customer.Clone();
        if (copy.Id == Guid.Empty)
        {
            copy.Id = Guid.NewGuid();
        }

        copy.CreatedAt = DateTime.UtcNow;
        _customers.Add(copy);

        return Task.FromResult(copy.Clone());
    }

    public Task UpdateAsync(Customer customer)
    {
        var index = _customers.FindIndex(item => item.Id == customer.Id);
        if (index < 0)
        {
            throw new KeyNotFoundException("Customer not found.");
        }

        var copy = customer.Clone();
        copy.CreatedAt = _customers[index].CreatedAt;
        _customers[index] = copy;

        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        var removed = _customers.RemoveAll(customer => customer.Id == id) > 0;
        return Task.FromResult(removed);
    }
}

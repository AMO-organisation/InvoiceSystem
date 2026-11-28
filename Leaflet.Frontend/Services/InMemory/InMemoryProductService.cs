using Leaflet.Frontend.Models;
using Leaflet.Frontend.Services.Abstractions;

namespace Leaflet.Frontend.Services.InMemory;

public sealed class InMemoryProductService : IProductService
{
    private readonly List<Product> _products = new();

    public Task<IReadOnlyList<Product>> GetAllAsync()
    {
        IReadOnlyList<Product> result = _products
            .OrderBy(product => product.Name, StringComparer.OrdinalIgnoreCase)
            .Select(product => product.Clone())
            .ToList();

        return Task.FromResult(result);
    }

    public Task<Product?> GetByIdAsync(Guid id)
    {
        var product = _products.FirstOrDefault(item => item.Id == id);
        return Task.FromResult(product?.Clone());
    }

    public Task<Product> AddAsync(Product product)
    {
        var copy = product.Clone();
        if (copy.Id == Guid.Empty)
        {
            copy.Id = Guid.NewGuid();
        }

        copy.CreatedAt = DateTime.UtcNow;
        _products.Add(copy);

        return Task.FromResult(copy.Clone());
    }

    public Task UpdateAsync(Product product)
    {
        var index = _products.FindIndex(item => item.Id == product.Id);
        if (index < 0)
        {
            throw new KeyNotFoundException("Product not found.");
        }

        var copy = product.Clone();
        copy.CreatedAt = _products[index].CreatedAt;
        _products[index] = copy;

        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        var removed = _products.RemoveAll(product => product.Id == id) > 0;
        return Task.FromResult(removed);
    }
}

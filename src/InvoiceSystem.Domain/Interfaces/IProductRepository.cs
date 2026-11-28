using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Enums;

namespace InvoiceSystem.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync(string? search = null, CatalogItemType? type = null);
    Task<Product> AddAsync(Product product);
    Task UpdateAsync(Product product);
}

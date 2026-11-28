using InvoiceSystem.Application.DTOs;
using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Enums;
using InvoiceSystem.Domain.Interfaces;

namespace InvoiceSystem.Application.Services;

public class ProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Sku = dto.Sku,
            Description = dto.Description,
            Type = dto.Type,
            UnitPrice = dto.UnitPrice,
            IsTaxable = dto.IsTaxable,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _productRepository.AddAsync(product);
        return MapToDto(created);
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync(string? search = null, CatalogItemType? type = null)
    {
        var products = await _productRepository.GetAllAsync(search, type);
        return products.Select(MapToDto);
    }

    public async Task<ProductDto> GetByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {id} was not found.");
        }
        return MapToDto(product);
    }

    public async Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {id} was not found.");
        }

        product.Name = dto.Name;
        product.Sku = dto.Sku;
        product.Description = dto.Description;
        product.Type = dto.Type;
        product.UnitPrice = dto.UnitPrice;
        product.IsTaxable = dto.IsTaxable;
        product.IsActive = dto.IsActive;

        await _productRepository.UpdateAsync(product);
        return MapToDto(product);
    }

    public async Task DeactivateAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {id} was not found.");
        }

        product.IsActive = false;
        await _productRepository.UpdateAsync(product);
    }

    private static ProductDto MapToDto(Product p) => new(
        p.Id,
        p.Name,
        p.Sku,
        p.Description,
        p.Type,
        p.UnitPrice,
        p.IsTaxable,
        p.IsActive,
        p.CreatedAt
    );
}

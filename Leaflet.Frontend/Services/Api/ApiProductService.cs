using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Leaflet.Frontend.Contracts;
using Leaflet.Frontend.Models;
using Leaflet.Frontend.Services.Abstractions;

namespace Leaflet.Frontend.Services.Api;

public sealed class ApiProductService : ApiServiceBase, IProductService
{
    private readonly Dictionary<Guid, int> _frontendToBackend = new();
    private readonly Dictionary<int, Guid> _backendToFrontend = new();

    public ApiProductService(HttpClient http, JsonSerializerOptions json) : base(http, json) { }

    private Guid ToFrontendId(int backendId)
    {
        if (_backendToFrontend.TryGetValue(backendId, out var frontendId))
        {
            return frontendId;
        }

        var id = Guid.NewGuid();
        _backendToFrontend[backendId] = id;
        _frontendToBackend[id] = backendId;
        return id;
    }

    private int ToBackendId(Guid frontendId)
    {
        if (_frontendToBackend.TryGetValue(frontendId, out var backendId))
        {
            return backendId;
        }

        throw new KeyNotFoundException("Product is not known to the backend.");
    }

    private static Product ToProduct(ProductDto dto)
    {
        return new Product
        {
            Id = Guid.Empty, // filled by caller after id mapping
            Name = dto.Name,
            Description = dto.Description,
            UnitPrice = dto.UnitPrice,
            Unit = dto.Sku ?? "each",
            TaxRate = dto.IsTaxable ? 0m : 0m,
            IsService = dto.Type == CatalogItemType.Service,
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt
        };
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync()
    {
        var dtos = await Http.GetFromJsonAsync<List<ProductDto>>("api/products", Json)
            ?? new();

        var result = new List<Product>(dtos.Count);
        foreach (var dto in dtos)
        {
            var product = ToProduct(dto);
            product.Id = ToFrontendId(dto.Id);
            result.Add(product);
        }

        return result.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        var backendId = ToBackendId(id);
        var dto = await Http.GetFromJsonAsync<ProductDto>($"api/products/{backendId}", Json);
        if (dto is null)
        {
            return null;
        }

        var product = ToProduct(dto);
        product.Id = ToFrontendId(dto.Id);
        return product;
    }

    public async Task<Product> AddAsync(Product product)
    {
        var dto = new CreateProductDto(
            product.Name,
            product.Unit,
            product.Description,
            product.IsService ? CatalogItemType.Service : CatalogItemType.Product,
            product.UnitPrice,
            product.TaxRate > 0m
        );

        var response = await Http.PostAsJsonAsync("api/products", dto, Json);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
        }

        var createdDto = await response.Content.ReadFromJsonAsync<ProductDto>(Json)
            ?? throw new InvalidOperationException("Failed to create product.");

        var result = ToProduct(createdDto);
        result.Id = ToFrontendId(createdDto.Id);
        return result;
    }

    public async Task UpdateAsync(Product product)
    {
        var backendId = ToBackendId(product.Id);
        var dto = new CreateProductDto(
            product.Name,
            product.Unit,
            product.Description,
            product.IsService ? CatalogItemType.Service : CatalogItemType.Product,
            product.UnitPrice,
            product.TaxRate > 0m
        );

        var response = await Http.PutAsJsonAsync($"api/products/{backendId}", dto, Json);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
        }

        var updatedDto = await response.Content.ReadFromJsonAsync<ProductDto>(Json)
            ?? throw new InvalidOperationException("Failed to update product.");

        _frontendToBackend[product.Id] = updatedDto.Id;
        _backendToFrontend[updatedDto.Id] = product.Id;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var backendId = ToBackendId(id);
        var response = await Http.DeleteAsync($"api/products/{backendId}");
        return response.IsSuccessStatusCode;
    }
}

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Leaflet.Frontend.Contracts;
using Leaflet.Frontend.Models;
using Leaflet.Frontend.Services.Abstractions;

namespace Leaflet.Frontend.Services.Api;

public sealed class ApiCustomerService : ApiServiceBase, ICustomerService
{
    private readonly Dictionary<Guid, int> _frontendToBackend = new();
    private readonly Dictionary<int, Guid> _backendToFrontend = new();

    public ApiCustomerService(HttpClient http, JsonSerializerOptions json) : base(http, json) { }

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

        throw new KeyNotFoundException("Customer is not known to the backend.");
    }

    private static Customer ToCustomer(CustomerDto dto)
    {
        return new Customer
        {
            Id = Guid.Empty, // filled by caller after id mapping
            Name = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            BillingAddress = new Address
            {
                Street = dto.Address ?? string.Empty,
                City = dto.City ?? string.Empty
            },
            CreatedAt = dto.CreatedAt
        };
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync()
    {
        var dtos = await Http.GetFromJsonAsync<List<CustomerDto>>("api/customers", Json)
            ?? new();

        var result = new List<Customer>(dtos.Count);
        foreach (var dto in dtos)
        {
            var customer = ToCustomer(dto);
            customer.Id = ToFrontendId(dto.Id);
            result.Add(customer);
        }

        return result.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        var backendId = ToBackendId(id);
        var dto = await Http.GetFromJsonAsync<CustomerDto>($"api/customers/{backendId}", Json);
        if (dto is null)
        {
            return null;
        }

        var customer = ToCustomer(dto);
        customer.Id = ToFrontendId(dto.Id);
        return customer;
    }

    public async Task<Customer> AddAsync(Customer customer)
    {
        var dto = new CreateCustomerDto(
            customer.Name,
            null,
            customer.Email ?? string.Empty,
            customer.Phone ?? string.Empty,
            customer.BillingAddress.Street,
            customer.BillingAddress.City
        );

        var response = await Http.PostAsJsonAsync("api/customers", dto, Json);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
        }

        var createdDto = await response.Content.ReadFromJsonAsync<CustomerDto>(Json)
            ?? throw new InvalidOperationException("Failed to create customer.");

        var result = ToCustomer(createdDto);
        result.Id = ToFrontendId(createdDto.Id);
        return result;
    }

    public async Task UpdateAsync(Customer customer)
    {
        var backendId = ToBackendId(customer.Id);
        var dto = new CreateCustomerDto(
            customer.Name,
            null,
            customer.Email ?? string.Empty,
            customer.Phone ?? string.Empty,
            customer.BillingAddress.Street,
            customer.BillingAddress.City
        );

        var response = await Http.PutAsJsonAsync($"api/customers/{backendId}", dto, Json);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
        }

        var updatedDto = await response.Content.ReadFromJsonAsync<CustomerDto>(Json)
            ?? throw new InvalidOperationException("Failed to update customer.");

        // Re-point the frontend id to the backend id in case the mapping is missing.
        _frontendToBackend[customer.Id] = updatedDto.Id;
        _backendToFrontend[updatedDto.Id] = customer.Id;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var backendId = ToBackendId(id);
        var response = await Http.DeleteAsync($"api/customers/{backendId}");
        return response.IsSuccessStatusCode;
    }
}

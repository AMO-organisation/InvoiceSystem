using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Leaflet.Frontend.Contracts;
using Leaflet.Frontend.Models;
using Leaflet.Frontend.Services.Abstractions;

namespace Leaflet.Frontend.Services.Api;

public sealed class ApiSettingsService : ApiServiceBase, ISettingsService
{
    private CompanySettings _current = new();

    public CompanySettings Current => _current;

    public ApiSettingsService(HttpClient http, JsonSerializerOptions json) : base(http, json) { }

    public async Task LoadAsync()
    {
        try
        {
            var dto = await Http.GetFromJsonAsync<CompanyProfileDto>("api/company-profile", Json);
            if (dto is not null)
            {
                _current = FromDto(dto);
            }
        }
        catch
        {
            // Keep the current (default) settings if the API is unavailable.
        }
    }

    public async Task UpdateAsync(CompanySettings settings)
    {
        var request = new UpdateCompanyProfileRequest(
            settings.CompanyName,
            settings.Address.Street,
            settings.Address.City,
            settings.Email ?? string.Empty,
            settings.Phone ?? string.Empty,
            settings.CurrencySymbol ?? string.Empty,
            settings.DefaultTaxRate,
            settings.PaymentInstructions
        );

        var response = await Http.PutAsJsonAsync("api/company-profile", request, Json);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
        }

        var dto = await response.Content.ReadFromJsonAsync<CompanyProfileDto>(Json)
            ?? throw new InvalidOperationException("Failed to update company profile.");

        _current = FromDto(dto);
    }

    private static CompanySettings FromDto(CompanyProfileDto d) => new()
    {
        CompanyName = d.CompanyName,
        Address = new Address
        {
            Street = d.Address,
            City = d.City ?? string.Empty
        },
        Email = d.Email,
        Phone = d.Phone,
        CurrencySymbol = d.CurrencySymbol,
        DefaultTaxRate = d.DefaultTaxRate,
        PaymentInstructions = d.PaymentInstructions ?? string.Empty
    };
}

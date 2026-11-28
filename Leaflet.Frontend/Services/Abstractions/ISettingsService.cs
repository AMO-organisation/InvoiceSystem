using Leaflet.Frontend.Models;

namespace Leaflet.Frontend.Services.Abstractions;

public interface ISettingsService
{
    CompanySettings Current { get; }
    Task LoadAsync();
    Task UpdateAsync(CompanySettings settings);
}

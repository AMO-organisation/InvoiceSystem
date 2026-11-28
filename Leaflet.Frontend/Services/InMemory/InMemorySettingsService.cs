using Leaflet.Frontend.Models;
using Leaflet.Frontend.Services.Abstractions;

namespace Leaflet.Frontend.Services.InMemory;

public sealed class InMemorySettingsService : ISettingsService
{
    private CompanySettings _settings = new();

    public CompanySettings Current => _settings.Clone();

    public void Update(CompanySettings settings) => _settings = settings.Clone();
}

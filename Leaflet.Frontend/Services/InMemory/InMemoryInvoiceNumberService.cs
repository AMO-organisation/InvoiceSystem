using Leaflet.Frontend.Services.Abstractions;

namespace Leaflet.Frontend.Services.InMemory;

public sealed class InMemoryInvoiceNumberService(ISettingsService settings) : IInvoiceNumberService
{
    private readonly Dictionary<int, int> _sequences = new();

    public Task<string> NextNumberAsync(int year)
    {
        var prefix = settings.Current.InvoicePrefix;
        if (string.IsNullOrWhiteSpace(prefix))
        {
            prefix = "INV";
        }

        _sequences.TryGetValue(year, out var sequence);
        sequence++;
        _sequences[year] = sequence;

        return Task.FromResult($"{prefix.Trim()}-{year}-{sequence:0000}");
    }
}

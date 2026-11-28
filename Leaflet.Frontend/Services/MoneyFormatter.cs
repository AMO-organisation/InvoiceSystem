using System.Globalization;
using Leaflet.Frontend.Services.Abstractions;

namespace Leaflet.Frontend.Services;

public sealed class MoneyFormatter(ISettingsService settings)
{
    public string Format(decimal amount)
    {
        var current = settings.Current;
        var symbol = string.IsNullOrWhiteSpace(current.CurrencySymbol)
            ? current.CurrencyCode
            : current.CurrencySymbol;
        var sign = amount < 0 ? "-" : string.Empty;
        var value = Math.Abs(amount).ToString("N2", CultureInfo.InvariantCulture);

        return $"{sign}{symbol}{value}";
    }

    public string CurrencyCode => settings.Current.CurrencyCode;
}

namespace Leaflet.Frontend.Models;

public sealed class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(Street) &&
        string.IsNullOrWhiteSpace(City) &&
        string.IsNullOrWhiteSpace(State) &&
        string.IsNullOrWhiteSpace(PostalCode) &&
        string.IsNullOrWhiteSpace(Country);

    public string ToSingleLine()
    {
        var parts = new[] { Street, City, State, PostalCode, Country }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToArray();

        return parts.Length == 0 ? string.Empty : string.Join(", ", parts);
    }

    public Address Clone() => (Address)MemberwiseClone();
}

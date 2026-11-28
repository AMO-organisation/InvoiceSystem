using System.Text.Json.Serialization;

namespace InvoiceSystem.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CatalogItemType
{
    Product = 1,
    Service = 2
}

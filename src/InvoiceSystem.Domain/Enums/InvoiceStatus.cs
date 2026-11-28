using System.Text.Json.Serialization;

namespace InvoiceSystem.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InvoiceStatus
{
    Draft = 0,
    Unpaid = 1,
    Paid = 2,
    Overdue = 3,
    Cancelled = 4
}

namespace Leaflet.Frontend.Models;

public enum InvoiceStatus
{
    Draft,
    Issued,
    Paid,
    Void
}

public static class InvoiceStatusInfo
{
    public static bool IsOverdue(this Invoice invoice) =>
        invoice.Status == InvoiceStatus.Issued &&
        invoice.DueDate < DateOnly.FromDateTime(DateTime.Today);

    public static string Label(this Invoice invoice) => invoice.Status switch
    {
        InvoiceStatus.Draft => "Draft",
        InvoiceStatus.Paid => "Paid",
        InvoiceStatus.Void => "Void",
        InvoiceStatus.Issued when invoice.IsOverdue() => "Overdue",
        InvoiceStatus.Issued => "Issued",
        _ => invoice.Status.ToString()
    };

    public static string CssClass(this Invoice invoice) => invoice.Status switch
    {
        InvoiceStatus.Draft => "status-draft",
        InvoiceStatus.Paid => "status-paid",
        InvoiceStatus.Void => "status-void",
        InvoiceStatus.Issued when invoice.IsOverdue() => "status-overdue",
        InvoiceStatus.Issued => "status-issued",
        _ => "status-draft"
    };
}

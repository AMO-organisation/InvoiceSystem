namespace Leaflet.Frontend.Services.Abstractions;

public interface IInvoiceNumberService
{
    Task<string> NextNumberAsync(int year);
}

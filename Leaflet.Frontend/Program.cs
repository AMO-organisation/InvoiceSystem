using System.Text.Json;
using System.Text.Json.Serialization;
using Leaflet.Frontend;
using Leaflet.Frontend.Services;
using Leaflet.Frontend.Services.Abstractions;
using Leaflet.Frontend.Services.Api;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Shared JSON options that match the ASP.NET Core backend contract:
// camelCase property names, case-insensitive deserialization, and enum
// member names as strings (e.g. "Product", "Unpaid").
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    Converters = { new JsonStringEnumConverter() }
};
builder.Services.AddSingleton(jsonOptions);

// The HttpClient used by all API services. Its BaseAddress comes from
// wwwroot/appsettings.json ("ApiBaseUrl"), e.g. http://localhost:5094.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]?.TrimEnd('/') ?? "http://localhost:5094";
builder.Services.AddSingleton(sp => new HttpClient
{
    BaseAddress = new Uri($"{apiBaseUrl}/")
});

builder.Services.AddSingleton<ISettingsService, ApiSettingsService>();
builder.Services.AddSingleton<ICustomerService, ApiCustomerService>();
builder.Services.AddSingleton<IProductService, ApiProductService>();
builder.Services.AddSingleton<IInvoiceService, ApiInvoiceService>();
builder.Services.AddSingleton<MoneyFormatter>();
builder.Services.AddSingleton<DialogService>();

var host = builder.Build();

// Load the live company profile from the backend before the UI renders.
await host.Services.GetRequiredService<ISettingsService>().LoadAsync();

await host.RunAsync();

using Leaflet.Frontend;
using Leaflet.Frontend.Services;
using Leaflet.Frontend.Services.Abstractions;
using Leaflet.Frontend.Services.InMemory;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddSingleton<ISettingsService, InMemorySettingsService>();
builder.Services.AddSingleton<ICustomerService, InMemoryCustomerService>();
builder.Services.AddSingleton<IProductService, InMemoryProductService>();
builder.Services.AddSingleton<IInvoiceNumberService, InMemoryInvoiceNumberService>();
builder.Services.AddSingleton<IInvoiceService, InMemoryInvoiceService>();
builder.Services.AddSingleton<MoneyFormatter>();
builder.Services.AddSingleton<DialogService>();
builder.Services.AddSingleton<DemoDataSeeder>();

var host = builder.Build();
host.Services.GetRequiredService<DemoDataSeeder>().SeedAsync().GetAwaiter().GetResult();

await host.RunAsync();

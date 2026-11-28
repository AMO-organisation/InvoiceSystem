using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using InvoiceSystem.Application.DTOs;
using InvoiceSystem.Domain.Enums;
using Xunit;
using Xunit.Abstractions;

namespace InvoiceSystem.Tests;

public class InvoiceApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public InvoiceApiIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Fact]
    public async Task FullInvoiceWorkflow_ShouldSucceed()
    {
        var client = _factory.CreateClient();

        // 1. Create Customer
        var customerDto = new CreateCustomerDto(
            FullName: "Jane Doe",
            CompanyName: "Doe Enterprises",
            Email: $"jane.doe.{Guid.NewGuid()}@example.com",
            Phone: "555-0199",
            Address: "100 Main St",
            City: "Metropolis"
        );

        var custResponse = await client.PostAsJsonAsync("/api/customers", customerDto);
        Assert.Equal(HttpStatusCode.Created, custResponse.StatusCode);
        var createdCustomer = await custResponse.Content.ReadFromJsonAsync<CustomerDto>();
        Assert.NotNull(createdCustomer);

        // 2. Create Product
        var productDto = new CreateProductDto(
            Name: "Consulting Service",
            Sku: "SRV-01",
            Description: "Hourly advisory",
            Type: CatalogItemType.Service,
            UnitPrice: 150.00m,
            IsTaxable: true
        );

        var prodResponse = await client.PostAsJsonAsync("/api/products", productDto);
        Assert.Equal(HttpStatusCode.Created, prodResponse.StatusCode);
        var createdProduct = await prodResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(createdProduct);

        // 3. Create Invoice
        var invoiceDto = new CreateInvoiceDto(
            CustomerId: createdCustomer.Id,
            IssueDate: DateTime.UtcNow,
            DueDate: DateTime.UtcNow.AddDays(15),
            DiscountAmount: 20.00m,
            TaxRate: 7.50m,
            Notes: "Thank you for your business.",
            Items: new List<CreateInvoiceItemDto>
            {
                new(ProductId: createdProduct.Id, ItemName: null, Description: null, Quantity: 2, UnitPrice: null, IsTaxable: null)
            }
        );

        var invResponse = await client.PostAsJsonAsync("/api/invoices", invoiceDto);
        Assert.Equal(HttpStatusCode.Created, invResponse.StatusCode);
        var createdInvoice = await invResponse.Content.ReadFromJsonAsync<InvoiceDetailDto>();
        Assert.NotNull(createdInvoice);
        Assert.StartsWith("INV-", createdInvoice.InvoiceNumber);

        Assert.Equal(300.00m, createdInvoice.Subtotal);
        Assert.Equal(20.00m, createdInvoice.DiscountAmount);
        Assert.Equal(21.00m, createdInvoice.TaxAmount);
        Assert.Equal(301.00m, createdInvoice.GrandTotal);

        // 4. Download PDF
        var pdfResponse = await client.GetAsync($"/api/invoices/{createdInvoice.Id}/pdf");
        if (pdfResponse.StatusCode != HttpStatusCode.OK)
        {
            var errContent = await pdfResponse.Content.ReadAsStringAsync();
            _output.WriteLine($"PDF Error Response: {errContent}");
        }
        Assert.Equal(HttpStatusCode.OK, pdfResponse.StatusCode);
        Assert.Equal("application/pdf", pdfResponse.Content.Headers.ContentType?.MediaType);

        var pdfBytes = await pdfResponse.Content.ReadAsByteArrayAsync();
        Assert.True(pdfBytes.Length > 0);

        var headerString = System.Text.Encoding.ASCII.GetString(pdfBytes, 0, 4);
        Assert.Equal("%PDF", headerString);
    }
}

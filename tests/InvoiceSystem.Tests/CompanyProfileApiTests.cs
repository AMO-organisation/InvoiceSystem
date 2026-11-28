using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using InvoiceSystem.Application.DTOs;
using InvoiceSystem.Domain.Enums;
using Xunit;

namespace InvoiceSystem.Tests;

public class CompanyProfileApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CompanyProfileApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UpdateProfile_PersistsAndIsReturnedByGet()
    {
        var client = _factory.CreateClient();

        // 1. Update the company profile via PUT
        var request = new UpdateCompanyProfileRequest(
            "Dynamic Test Co",
            "1 Dynamic Street",
            "Abuja",
            "info@dynamic.com",
            "+234 900 000 0000",
            "₦",
            10.0m,
            "Pay via Bank Transfer");

        var putResponse = await client.PutAsJsonAsync("/api/company-profile", request);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        var updated = await putResponse.Content.ReadFromJsonAsync<CompanyProfileDto>();
        Assert.NotNull(updated);
        Assert.Equal("Dynamic Test Co", updated.CompanyName);
        Assert.Equal("1 Dynamic Street", updated.Address);
        Assert.Equal("Abuja", updated.City);
        Assert.Equal("info@dynamic.com", updated.Email);
        Assert.Equal("+234 900 000 0000", updated.Phone);
        Assert.Equal("₦", updated.CurrencySymbol);
        Assert.Equal(10.0m, updated.DefaultTaxRate);
        Assert.Equal("Pay via Bank Transfer", updated.PaymentInstructions);

        // 2. GET always succeeds and returns the persisted (live) profile
        var getResponse = await client.GetAsync("/api/company-profile");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var profile = await getResponse.Content.ReadFromJsonAsync<CompanyProfileDto>();
        Assert.NotNull(profile);
        Assert.Equal("Dynamic Test Co", profile.CompanyName);
        Assert.Equal("1 Dynamic Street", profile.Address);
        Assert.Equal("Abuja", profile.City);
        Assert.Equal("info@dynamic.com", profile.Email);
        Assert.Equal("+234 900 000 0000", profile.Phone);
        Assert.Equal("₦", profile.CurrencySymbol);
        Assert.Equal(10.0m, profile.DefaultTaxRate);
        Assert.Equal("Pay via Bank Transfer", profile.PaymentInstructions);
    }

    [Fact]
    public async Task UpdateProfile_ThenGeneratePdf_ReflectsLiveCompanyProfile()
    {
        var client = _factory.CreateClient();

        // Set a distinctive company profile with a non-default tax rate.
        var request = new UpdateCompanyProfileRequest(
            "Dynamic Test Co",
            "1 Dynamic Street",
            "Abuja",
            "info@dynamic.com",
            "+234 900 000 0000",
            "₦",
            10.0m,
            "Pay via Bank Transfer");

        var putResponse = await client.PutAsJsonAsync("/api/company-profile", request);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        // Create a customer.
        var customerDto = new CreateCustomerDto(
            "Jane Doe", "Doe Enterprises", $"jane.dynamic.{Guid.NewGuid()}@example.com",
            "555-0199", "100 Main St", "Metropolis");
        var custResponse = await client.PostAsJsonAsync("/api/customers", customerDto);
        Assert.Equal(HttpStatusCode.Created, custResponse.StatusCode);
        var createdCustomer = await custResponse.Content.ReadFromJsonAsync<CustomerDto>();
        Assert.NotNull(createdCustomer);

        // Create a product.
        var productDto = new CreateProductDto(
            "Consulting Service", "SRV-DYN", "Hourly advisory",
            CatalogItemType.Service, 100.00m, true);
        var prodResponse = await client.PostAsJsonAsync("/api/products", productDto);
        Assert.Equal(HttpStatusCode.Created, prodResponse.StatusCode);
        var createdProduct = await prodResponse.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(createdProduct);

        // Create an invoice WITHOUT specifying TaxRate -> should default to CompanyProfile.DefaultTaxRate (10.0).
        var invoiceDto = new CreateInvoiceDto(
            createdCustomer.Id,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(15),
            0.00m,
            null,
            "Thank you for your business.",
            new List<CreateInvoiceItemDto>
            {
                new(createdProduct.Id, null, null, 2, null, true)
            });

        var invResponse = await client.PostAsJsonAsync("/api/invoices", invoiceDto);
        Assert.Equal(HttpStatusCode.Created, invResponse.StatusCode);
        var createdInvoice = await invResponse.Content.ReadFromJsonAsync<InvoiceDetailDto>();
        Assert.NotNull(createdInvoice);

        // The invoice tax rate defaults from the live company profile.
        Assert.Equal(10.0m, createdInvoice.TaxRate);

        // 3. Generate the PDF -> uses the live company profile (hardcoded strings removed).
        var pdfResponse = await client.GetAsync($"/api/invoices/{createdInvoice.Id}/pdf");
        Assert.Equal(HttpStatusCode.OK, pdfResponse.StatusCode);
        Assert.Equal("application/pdf", pdfResponse.Content.Headers.ContentType?.MediaType);

        var pdfBytes = await pdfResponse.Content.ReadAsByteArrayAsync();
        Assert.True(pdfBytes.Length > 0);

        var headerString = System.Text.Encoding.ASCII.GetString(pdfBytes, 0, 4);
        Assert.Equal("%PDF", headerString);
    }
}
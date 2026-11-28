using Microsoft.EntityFrameworkCore;
using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Enums;

namespace InvoiceSystem.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Customers.AnyAsync())
        {
            return; // Already seeded
        }

        // 1. Customers
        var customers = new List<Customer>
        {
            new() { FullName = "Acme Corp", CompanyName = "Acme Corporation", Email = "billing@acme.com", Phone = "+1-555-0100", Address = "123 Industrial Pkwy", City = "Chicago" },
            new() { FullName = "Global Tech", CompanyName = "Global Technologies", Email = "finance@globaltech.io", Phone = "+1-555-0200", Address = "456 Silicon Ave", City = "San Francisco" },
            new() { FullName = "Sarah Connor", CompanyName = null, Email = "sconnor@resistance.org", Phone = "+1-555-0300", Address = "789 Cyberdyne Ln", City = "Los Angeles" }
        };

        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();

        // 2. Products / Services
        var products = new List<Product>
        {
            new() { Name = "Web Development", Sku = "SRV-WEB", Description = "Custom full-stack web application development", Type = CatalogItemType.Service, UnitPrice = 1200.00m, IsTaxable = true },
            new() { Name = "Cloud Hosting (Monthly)", Sku = "SRV-HOST", Description = "Managed cloud infrastructure and CDN", Type = CatalogItemType.Service, UnitPrice = 150.00m, IsTaxable = true },
            new() { Name = "Consulting Hour", Sku = "SRV-CONS", Description = "Technical advisory and architecture review", Type = CatalogItemType.Service, UnitPrice = 150.00m, IsTaxable = true },
            new() { Name = "Ergonomic Office Chair", Sku = "PRD-CHAIR", Description = "Adjustable mesh office chair", Type = CatalogItemType.Product, UnitPrice = 350.00m, IsTaxable = true },
            new() { Name = "Wireless Mechanical Keyboard", Sku = "PRD-KEY", Description = "RGB backlit mechanical keyboard", Type = CatalogItemType.Product, UnitPrice = 120.00m, IsTaxable = true }
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        // 3. Sample Invoices
        var year = DateTime.UtcNow.Year;

        var inv1 = new Invoice
        {
            InvoiceNumber = $"INV-{year}-0001",
            CustomerId = customers[0].Id,
            IssueDate = DateTime.UtcNow.AddDays(-10),
            DueDate = DateTime.UtcNow.AddDays(20),
            Status = InvoiceStatus.Unpaid,
            Subtotal = 1350.00m,
            DiscountAmount = 50.00m,
            TaxRate = 7.50m,
            TaxAmount = 97.50m, // (1350 - 50) * 0.075 = 1300 * 0.075 = 97.50
            GrandTotal = 1397.50m,
            Notes = "Net 30 payment terms.",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            Items = new List<InvoiceItem>
            {
                new() { ProductId = products[0].Id, ItemName = products[0].Name, Description = products[0].Description, Quantity = 1, UnitPrice = products[0].UnitPrice, IsTaxable = products[0].IsTaxable, LineTotal = 1200.00m },
                new() { ProductId = products[1].Id, ItemName = products[1].Name, Description = products[1].Description, Quantity = 1, UnitPrice = products[1].UnitPrice, IsTaxable = products[1].IsTaxable, LineTotal = 150.00m }
            }
        };

        var inv2 = new Invoice
        {
            InvoiceNumber = $"INV-{year}-0002",
            CustomerId = customers[1].Id,
            IssueDate = DateTime.UtcNow.AddDays(-20),
            DueDate = DateTime.UtcNow.AddDays(-5),
            Status = InvoiceStatus.Paid,
            Subtotal = 300.00m,
            DiscountAmount = 0.00m,
            TaxRate = 7.50m,
            TaxAmount = 22.50m, // 300 * 0.075 = 22.50
            GrandTotal = 322.50m,
            Notes = "Thank you for prompt payment!",
            CreatedAt = DateTime.UtcNow.AddDays(-20),
            PaidAt = DateTime.UtcNow.AddDays(-6),
            Items = new List<InvoiceItem>
            {
                new() { ProductId = products[2].Id, ItemName = products[2].Name, Description = products[2].Description, Quantity = 2, UnitPrice = products[2].UnitPrice, IsTaxable = products[2].IsTaxable, LineTotal = 300.00m }
            }
        };

        context.Invoices.AddRange(inv1, inv2);
        await context.SaveChangesAsync();
    }
}

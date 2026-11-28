using Microsoft.EntityFrameworkCore;
using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Enums;
using InvoiceSystem.Infrastructure.Data;
using InvoiceSystem.Infrastructure.Services;
using Xunit;

namespace InvoiceSystem.Tests;

public class InvoiceNumberGeneratorTests
{
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"DataSource=:memory:")
            .Options;
        var context = new AppDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task GenerateNextInvoiceNumberAsync_ShouldIncrementSequentially()
    {
        using var context = GetInMemoryDbContext();
        var generator = new InvoiceNumberGenerator(context);

        var num1 = await generator.GenerateNextInvoiceNumberAsync();
        var year = DateTime.UtcNow.Year;
        Assert.Equal($"INV-{year}-0001", num1);

        var customer = new Customer
        {
            FullName = "Test Customer",
            Email = "test@example.com",
            Phone = "1234567890"
        };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        // Save an invoice with this number to DB
        context.Invoices.Add(new Invoice
        {
            InvoiceNumber = num1,
            CustomerId = customer.Id,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            Status = InvoiceStatus.Unpaid,
            Subtotal = 100m,
            GrandTotal = 100m
        });
        await context.SaveChangesAsync();

        var num2 = await generator.GenerateNextInvoiceNumberAsync();
        Assert.Equal($"INV-{year}-0002", num2);
    }
}

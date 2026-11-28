using Leaflet.Frontend.Models;
using Leaflet.Frontend.Services.Abstractions;

namespace Leaflet.Frontend.Services;

public sealed class DemoDataSeeder(
    ISettingsService settings,
    ICustomerService customers,
    IProductService products,
    IInvoiceService invoices)
{
    public async Task SeedAsync()
    {
        settings.Update(new CompanySettings
        {
            CompanyName = "Leaflet Studio",
            Address = new Address
            {
                Street = "14 Lantern Lane",
                City = "Portland",
                State = "OR",
                PostalCode = "97205",
                Country = "USA"
            },
            Email = "hello@leaflet.studio",
            Phone = "+234 (503) 555-0148",
            TaxId = "EIN 93-4821007",
            CurrencyCode = "NGN",
            CurrencySymbol = "₦",
            DefaultTaxRate = 8.25m,
            InvoicePrefix = "INV",
            PaymentTermsDays = 14,
            FooterText = "Payment by bank transfer within the terms above. Thank you for your business."
        });

        var existingCustomers = await customers.GetAllAsync();
        if (existingCustomers.Count > 0)
        {
            return;
        }

        var acme = await customers.AddAsync(new Customer
        {
            Name = "Acme Coffee Roasters",
            Email = "accounts@acmeroasters.co",
            Phone = "+234 (503) 555-0119",
            BillingAddress = new Address
            {
                Street = "88 Alder Street",
                City = "Portland",
                State = "OR",
                PostalCode = "97204",
                Country = "USA"
            },
            Notes = "Preferred contact is the accounts inbox."
        });

        var northwind = await customers.AddAsync(new Customer
        {
            Name = "Northwind Atelier",
            Email = "hello@northwindatelier.com",
            Phone = "+234 (206) 555-0182",
            BillingAddress = new Address
            {
                Street = "212 Clay Avenue",
                City = "Seattle",
                State = "WA",
                PostalCode = "98104",
                Country = "USA"
            },
            TaxId = "WA 604-221-889"
        });

        var cedar = await customers.AddAsync(new Customer
        {
            Name = "Cedar & Co.",
            Email = "studio@cedarandco.com",
            Phone = "+234 (541) 555-0134",
            BillingAddress = new Address
            {
                Street = "44 Juniper Way",
                City = "Bend",
                State = "OR",
                PostalCode = "97701",
                Country = "USA"
            }
        });

        var brandIdentity = await products.AddAsync(new Product
        {
            Name = "Brand Identity System",
            Description = "Strategy, identity design, and a complete brand guideline document.",
            UnitPrice = 4800m,
            Unit = "project",
            TaxRate = 0m,
            IsService = true
        });

        var websiteSprint = await products.AddAsync(new Product
        {
            Name = "Website Design Sprint",
            Description = "A two-week design sprint covering structure, UI, and handoff.",
            UnitPrice = 6500m,
            Unit = "project",
            TaxRate = 0m,
            IsService = true
        });

        var artDirection = await products.AddAsync(new Product
        {
            Name = "Art Direction Retainer",
            Description = "Monthly art direction and production oversight.",
            UnitPrice = 1800m,
            Unit = "month",
            TaxRate = 0m,
            IsService = true
        });

        var consultation = await products.AddAsync(new Product
        {
            Name = "Studio Consultation",
            Description = "Focused working session with the studio team.",
            UnitPrice = 175m,
            Unit = "hour",
            TaxRate = 0m,
            IsService = true
        });

        var businessCards = await products.AddAsync(new Product
        {
            Name = "Letterpress Business Cards",
            Description = "Two-color letterpress on 600gsm cotton stock. Set of 250.",
            UnitPrice = 145m,
            Unit = "set",
            TaxRate = 8.25m,
            IsService = false
        });

        var packagingInserts = await products.AddAsync(new Product
        {
            Name = "Packaging Insert Print",
            Description = "Custom printed insert cards, set of 500.",
            UnitPrice = 320m,
            Unit = "set",
            TaxRate = 8.25m,
            IsService = false
        });

        var today = DateOnly.FromDateTime(DateTime.Today);

        await invoices.AddAsync(new Invoice
        {
            CustomerId = acme.Id,
            CustomerName = acme.Name,
            IssueDate = today,
            DueDate = today.AddDays(14),
            DiscountPercent = 5m,
            PaymentTerms = "Net 14",
            Notes = "Phase one of the brand rollout. Includes the strategy workshop and two consultation sessions.",
            LineItems =
            {
                new InvoiceLineItem
                {
                    ProductId = brandIdentity.Id,
                    Description = brandIdentity.Name,
                    Unit = brandIdentity.Unit,
                    Quantity = 1,
                    UnitPrice = brandIdentity.UnitPrice,
                    TaxRate = brandIdentity.TaxRate
                },
                new InvoiceLineItem
                {
                    ProductId = consultation.Id,
                    Description = consultation.Name,
                    Unit = consultation.Unit,
                    Quantity = 3,
                    UnitPrice = consultation.UnitPrice,
                    TaxRate = consultation.TaxRate
                }
            }
        });

        var issued = await invoices.AddAsync(new Invoice
        {
            CustomerId = northwind.Id,
            CustomerName = northwind.Name,
            IssueDate = today.AddDays(-10),
            DueDate = today.AddDays(4),
            PaymentTerms = "Net 14",
            Notes = "Website design sprint and printed packaging inserts for the autumn release.",
            LineItems =
            {
                new InvoiceLineItem
                {
                    ProductId = websiteSprint.Id,
                    Description = websiteSprint.Name,
                    Unit = websiteSprint.Unit,
                    Quantity = 1,
                    UnitPrice = websiteSprint.UnitPrice,
                    TaxRate = websiteSprint.TaxRate
                },
                new InvoiceLineItem
                {
                    ProductId = packagingInserts.Id,
                    Description = packagingInserts.Name,
                    Unit = packagingInserts.Unit,
                    Quantity = 2,
                    UnitPrice = packagingInserts.UnitPrice,
                    TaxRate = packagingInserts.TaxRate
                }
            }
        });
        await invoices.FinalizeAsync(issued.Id);

        var paid = await invoices.AddAsync(new Invoice
        {
            CustomerId = cedar.Id,
            CustomerName = cedar.Name,
            IssueDate = today.AddDays(-24),
            DueDate = today.AddDays(-10),
            PaymentTerms = "Net 14",
            Notes = "Monthly art direction plus a fresh run of letterpress cards.",
            LineItems =
            {
                new InvoiceLineItem
                {
                    ProductId = artDirection.Id,
                    Description = artDirection.Name,
                    Unit = artDirection.Unit,
                    Quantity = 1,
                    UnitPrice = artDirection.UnitPrice,
                    TaxRate = artDirection.TaxRate
                },
                new InvoiceLineItem
                {
                    ProductId = businessCards.Id,
                    Description = businessCards.Name,
                    Unit = businessCards.Unit,
                    Quantity = 1,
                    UnitPrice = businessCards.UnitPrice,
                    TaxRate = businessCards.TaxRate
                }
            }
        });
        await invoices.FinalizeAsync(paid.Id);
        await invoices.MarkPaidAsync(paid.Id);

        var overdue = await invoices.AddAsync(new Invoice
        {
            CustomerId = cedar.Id,
            CustomerName = cedar.Name,
            IssueDate = today.AddDays(-25),
            DueDate = today.AddDays(-8),
            PaymentTerms = "Net 17",
            Notes = "Consultation block for the winter catalogue and a short print run.",
            LineItems =
            {
                new InvoiceLineItem
                {
                    ProductId = consultation.Id,
                    Description = consultation.Name,
                    Unit = consultation.Unit,
                    Quantity = 6,
                    UnitPrice = consultation.UnitPrice,
                    TaxRate = consultation.TaxRate
                },
                new InvoiceLineItem
                {
                    ProductId = packagingInserts.Id,
                    Description = packagingInserts.Name,
                    Unit = packagingInserts.Unit,
                    Quantity = 1,
                    UnitPrice = packagingInserts.UnitPrice,
                    TaxRate = packagingInserts.TaxRate
                }
            }
        });
        await invoices.FinalizeAsync(overdue.Id);
    }
}

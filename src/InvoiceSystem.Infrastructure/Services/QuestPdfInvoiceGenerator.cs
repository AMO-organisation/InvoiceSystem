using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;
using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Enums;
using InvoiceSystem.Domain.Interfaces;
using InvoiceSystem.Infrastructure.Data;

namespace InvoiceSystem.Infrastructure.Services;

public class QuestPdfInvoiceGenerator : IPdfInvoiceGenerator
{
    static QuestPdfInvoiceGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private readonly AppDbContext _context;

    public QuestPdfInvoiceGenerator(AppDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice)
    {
        var profile = await _context.CompanyProfiles.FirstOrDefaultAsync();
        if (profile == null)
        {
            profile = new CompanyProfile
            {
                CompanyName = "My Business Name",
                Address = "123 Main Street",
                City = "Lagos",
                Email = "info@mybusiness.com",
                Phone = "+234 000 000 0000",
                CurrencySymbol = "₦",
                DefaultTaxRate = 7.50m,
                PaymentInstructions = "Bank Transfer: Zenith Bank - 1234567890",
                UpdatedAt = DateTime.UtcNow
            };
            _context.CompanyProfiles.Add(profile);
            await _context.SaveChangesAsync();
        }

        var companyAddress = string.IsNullOrWhiteSpace(profile.City)
            ? profile.Address
            : $"{profile.Address}, {profile.City}";
        var companyContact = $"{profile.Email} | {profile.Phone}";

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial").FontColor(Colors.Grey.Darken3));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(profile.CompanyName).Bold().FontSize(20).FontColor(Colors.Blue.Medium);
                        col.Item().Text(companyAddress).FontSize(9).FontColor(Colors.Grey.Medium);
                        col.Item().Text(companyContact).FontSize(9).FontColor(Colors.Grey.Medium);
                        if (!string.IsNullOrWhiteSpace(profile.PaymentInstructions))
                        {
                            col.Item().Text($"Payment: {profile.PaymentInstructions}").FontSize(8).FontColor(Colors.Grey.Darken1);
                        }
                    });

                    row.ConstantItem(150).AlignRight().Column(col =>
                    {
                        col.Item().Text("INVOICE").Bold().FontSize(24).FontColor(Colors.Blue.Darken2);
                        col.Item().Text($"# {invoice.InvoiceNumber}").Bold().FontSize(12);
                        col.Item().Text($"Issue Date: {invoice.IssueDate:yyyy-MM-dd}").FontSize(9);
                        col.Item().Text($"Due Date: {invoice.DueDate:yyyy-MM-dd}").FontSize(9);
                        col.Item().Text($"Status: {invoice.Status}").Bold().FontSize(9).FontColor(invoice.Status == InvoiceStatus.Paid ? Colors.Green.Darken2 : Colors.Orange.Darken2);
                    });
                });

                page.Content().Column(col =>
                {
                    col.Spacing(20);
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    // Bill To
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("BILL TO:").Bold().FontSize(11).FontColor(Colors.Blue.Medium);
                            c.Item().Text(invoice.Customer?.FullName ?? "N/A").Bold();
                            if (!string.IsNullOrEmpty(invoice.Customer?.CompanyName))
                            {
                                c.Item().Text(invoice.Customer.CompanyName);
                            }
                            if (!string.IsNullOrEmpty(invoice.Customer?.Address))
                            {
                                c.Item().Text(invoice.Customer.Address);
                            }
                            if (!string.IsNullOrEmpty(invoice.Customer?.City))
                            {
                                c.Item().Text(invoice.Customer.City);
                            }
                            c.Item().Text(invoice.Customer?.Email ?? string.Empty);
                            c.Item().Text(invoice.Customer?.Phone ?? string.Empty);
                        });
                    });

                    // Items Table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Item & Description
                            columns.RelativeColumn(1); // Qty
                            columns.RelativeColumn(1); // Unit Price
                            columns.RelativeColumn(1); // Taxable
                            columns.RelativeColumn(1); // Line Total
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Lighten4).Padding(5).Text("Item / Description").Bold();
                            header.Cell().Background(Colors.Blue.Lighten4).Padding(5).AlignRight().Text("Qty").Bold();
                            header.Cell().Background(Colors.Blue.Lighten4).Padding(5).AlignRight().Text("Unit Price").Bold();
                            header.Cell().Background(Colors.Blue.Lighten4).Padding(5).AlignCenter().Text("Taxable").Bold();
                            header.Cell().Background(Colors.Blue.Lighten4).Padding(5).AlignRight().Text("Total").Bold();
                        });

                        foreach (var item in invoice.Items)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Column(c =>
                            {
                                c.Item().Text(item.ItemName).Bold();
                                if (!string.IsNullOrEmpty(item.Description))
                                {
                                    c.Item().Text(item.Description).FontSize(8).FontColor(Colors.Grey.Medium);
                                }
                            });
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(item.Quantity.ToString());
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{profile.CurrencySymbol} {item.UnitPrice:N2}");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text(item.IsTaxable ? "Yes" : "No");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{profile.CurrencySymbol} {item.LineTotal:N2}");
                        }
                    });

                    // Summary & Totals
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            if (!string.IsNullOrEmpty(invoice.Notes))
                            {
                                c.Item().Text("Notes:").Bold().FontSize(10);
                                c.Item().Text(invoice.Notes).FontSize(9).FontColor(Colors.Grey.Darken1);
                            }
                        });

                        row.ConstantItem(200).Column(c =>
                        {
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Subtotal:");
                                r.ConstantItem(80).AlignRight().Text($"{profile.CurrencySymbol} {invoice.Subtotal:N2}");
                            });
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Discount:");
                                r.ConstantItem(80).AlignRight().Text($"-{profile.CurrencySymbol} {invoice.DiscountAmount:N2}");
                            });
                            c.Item().Row(r =>
                            {
                                r.RelativeItem().Text($"Tax ({invoice.TaxRate:N2}%):");
                                r.ConstantItem(80).AlignRight().Text($"{profile.CurrencySymbol} {invoice.TaxAmount:N2}");
                            });
                            c.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                            c.Item().PaddingTop(5).Row(r =>
                            {
                                r.RelativeItem().Text("Grand Total:").Bold().FontSize(12);
                                r.ConstantItem(80).AlignRight().Text($"{profile.CurrencySymbol} {invoice.GrandTotal:N2}").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                            });
                        });
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }
}

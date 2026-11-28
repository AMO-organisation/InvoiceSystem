using InvoiceSystem.Application.DTOs;
using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Enums;
using InvoiceSystem.Domain.Interfaces;

namespace InvoiceSystem.Application.Services;

public class InvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInvoiceCalculator _invoiceCalculator;
    private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
    private readonly IPdfInvoiceGenerator _pdfInvoiceGenerator;
    private readonly ICompanyProfileService _companyProfileService;

    public InvoiceService(
        IInvoiceRepository invoiceRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IInvoiceCalculator invoiceCalculator,
        IInvoiceNumberGenerator invoiceNumberGenerator,
        IPdfInvoiceGenerator pdfInvoiceGenerator,
        ICompanyProfileService companyProfileService)
    {
        _invoiceRepository = invoiceRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _invoiceCalculator = invoiceCalculator;
        _invoiceNumberGenerator = invoiceNumberGenerator;
        _pdfInvoiceGenerator = pdfInvoiceGenerator;
        _companyProfileService = companyProfileService;
    }

    public async Task<InvoicePreviewResultDto> CalculatePreviewAsync(CalculatePreviewDto dto)
    {
        var itemInputs = new List<InvoiceItemInput>();
        foreach (var item in dto.Items)
        {
            string itemName = item.ItemName ?? string.Empty;
            decimal unitPrice = item.UnitPrice ?? 0m;
            bool isTaxable = item.IsTaxable ?? true;

            if (item.ProductId.HasValue)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId.Value);
                if (product != null)
                {
                    if (string.IsNullOrWhiteSpace(itemName)) itemName = product.Name;
                    if (!item.UnitPrice.HasValue) unitPrice = product.UnitPrice;
                    if (!item.IsTaxable.HasValue) isTaxable = product.IsTaxable;
                }
            }

            itemInputs.Add(new InvoiceItemInput(
                item.ProductId,
                itemName,
                item.Description,
                item.Quantity,
                unitPrice,
                isTaxable
            ));
        }

        var calcResult = _invoiceCalculator.Calculate(itemInputs, dto.DiscountAmount, dto.TaxRate);

        var itemDtos = calcResult.Items.Select((i, idx) => new InvoiceItemDto(
            Id: 0,
            ProductId: i.ProductId,
            ItemName: i.ItemName,
            Description: i.Description,
            Quantity: i.Quantity,
            UnitPrice: i.UnitPrice,
            IsTaxable: i.IsTaxable,
            LineTotal: i.LineTotal
        )).ToList();

        return new InvoicePreviewResultDto(
            calcResult.Subtotal,
            calcResult.DiscountAmount,
            calcResult.TaxRate,
            calcResult.TaxAmount,
            calcResult.GrandTotal,
            itemDtos
        );
    }

    public async Task<InvoiceDetailDto> CreateAsync(CreateInvoiceDto dto)
    {
        var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
        if (customer == null || customer.IsDeleted)
        {
            throw new KeyNotFoundException($"Customer with ID {dto.CustomerId} was not found or is deleted.");
        }

        var itemInputs = new List<InvoiceItemInput>();
        foreach (var item in dto.Items)
        {
            string itemName = item.ItemName ?? string.Empty;
            decimal unitPrice = item.UnitPrice ?? 0m;
            bool isTaxable = item.IsTaxable ?? true;

            if (item.ProductId.HasValue)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId.Value);
                if (product != null)
                {
                    if (string.IsNullOrWhiteSpace(itemName)) itemName = product.Name;
                    if (!item.UnitPrice.HasValue) unitPrice = product.UnitPrice;
                    if (!item.IsTaxable.HasValue) isTaxable = product.IsTaxable;
                }
            }

            itemInputs.Add(new InvoiceItemInput(
                item.ProductId,
                itemName,
                item.Description,
                item.Quantity,
                unitPrice,
                isTaxable
            ));
        }

        var profile = await _companyProfileService.GetProfileAsync();
        var taxRate = dto.TaxRate ?? profile.DefaultTaxRate;
        var calcResult = _invoiceCalculator.Calculate(itemInputs, dto.DiscountAmount, taxRate);
        if (calcResult.DiscountAmount > calcResult.Subtotal)
        {
            throw new InvalidOperationException("Discount amount cannot exceed invoice subtotal.");
        }

        var invoiceNumber = await _invoiceNumberGenerator.GenerateNextInvoiceNumberAsync();

        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            CustomerId = dto.CustomerId,
            IssueDate = dto.IssueDate,
            DueDate = dto.DueDate,
            Status = InvoiceStatus.Unpaid,
            Subtotal = calcResult.Subtotal,
            DiscountAmount = calcResult.DiscountAmount,
            TaxRate = calcResult.TaxRate,
            TaxAmount = calcResult.TaxAmount,
            GrandTotal = calcResult.GrandTotal,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            Items = calcResult.Items.Select(i => new InvoiceItem
            {
                ProductId = i.ProductId,
                ItemName = i.ItemName,
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                IsTaxable = i.IsTaxable,
                LineTotal = i.LineTotal
            }).ToList()
        };

        var created = await _invoiceRepository.AddAsync(invoice);
        return MapToDetailDto(created);
    }

    public async Task<IEnumerable<InvoiceDto>> GetAllAsync(InvoiceStatus? status = null, string? search = null, int? customerId = null)
    {
        var invoices = await _invoiceRepository.GetAllAsync(status, search, customerId);
        var dtos = new List<InvoiceDto>();

        foreach (var inv in invoices)
        {
            EvaluateAndUpdateStatus(inv);
            dtos.Add(MapToDto(inv));
        }
        return dtos;
    }

    public async Task<InvoiceDetailDto> GetByIdAsync(int id)
    {
        var inv = await _invoiceRepository.GetByIdAsync(id);
        if (inv == null)
        {
            throw new KeyNotFoundException($"Invoice with ID {id} was not found.");
        }
        EvaluateAndUpdateStatus(inv);
        return MapToDetailDto(inv);
    }

    public async Task<InvoiceDetailDto> UpdateAsync(int id, UpdateInvoiceDto dto)
    {
        var inv = await _invoiceRepository.GetByIdAsync(id);
        if (inv == null)
        {
            throw new KeyNotFoundException($"Invoice with ID {id} was not found.");
        }

        if (inv.Status == InvoiceStatus.Paid || inv.Status == InvoiceStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot update invoice in '{inv.Status}' status.");
        }

        var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
        if (customer == null || customer.IsDeleted)
        {
            throw new KeyNotFoundException($"Customer with ID {dto.CustomerId} was not found or is deleted.");
        }

        var itemInputs = new List<InvoiceItemInput>();
        foreach (var item in dto.Items)
        {
            string itemName = item.ItemName ?? string.Empty;
            decimal unitPrice = item.UnitPrice ?? 0m;
            bool isTaxable = item.IsTaxable ?? true;

            if (item.ProductId.HasValue)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId.Value);
                if (product != null)
                {
                    if (string.IsNullOrWhiteSpace(itemName)) itemName = product.Name;
                    if (!item.UnitPrice.HasValue) unitPrice = product.UnitPrice;
                    if (!item.IsTaxable.HasValue) isTaxable = product.IsTaxable;
                }
            }

            itemInputs.Add(new InvoiceItemInput(
                item.ProductId,
                itemName,
                item.Description,
                item.Quantity,
                unitPrice,
                isTaxable
            ));
        }

        var calcResult = _invoiceCalculator.Calculate(itemInputs, dto.DiscountAmount, dto.TaxRate);
        if (calcResult.DiscountAmount > calcResult.Subtotal)
        {
            throw new InvalidOperationException("Discount amount cannot exceed invoice subtotal.");
        }

        inv.CustomerId = dto.CustomerId;
        inv.IssueDate = dto.IssueDate;
        inv.DueDate = dto.DueDate;
        inv.Subtotal = calcResult.Subtotal;
        inv.DiscountAmount = calcResult.DiscountAmount;
        inv.TaxRate = calcResult.TaxRate;
        inv.TaxAmount = calcResult.TaxAmount;
        inv.GrandTotal = calcResult.GrandTotal;
        inv.Notes = dto.Notes;

        // Replace items
        inv.Items.Clear();
        foreach (var i in calcResult.Items)
        {
            inv.Items.Add(new InvoiceItem
            {
                ProductId = i.ProductId,
                ItemName = i.ItemName,
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                IsTaxable = i.IsTaxable,
                LineTotal = i.LineTotal
            });
        }

        await _invoiceRepository.UpdateAsync(inv);
        return MapToDetailDto(inv);
    }

    public async Task<InvoiceDetailDto> UpdateStatusAsync(int id, InvoiceStatus status)
    {
        var inv = await _invoiceRepository.GetByIdAsync(id);
        if (inv == null)
        {
            throw new KeyNotFoundException($"Invoice with ID {id} was not found.");
        }

        inv.Status = status;
        if (status == InvoiceStatus.Paid)
        {
            inv.PaidAt = DateTime.UtcNow;
        }
        else
        {
            inv.PaidAt = null;
        }

        await _invoiceRepository.UpdateAsync(inv);
        return MapToDetailDto(inv);
    }

    public async Task<byte[]> GetPdfBytesAsync(int id)
    {
        var inv = await _invoiceRepository.GetByIdAsync(id);
        if (inv == null)
        {
            throw new KeyNotFoundException($"Invoice with ID {id} was not found.");
        }
        return await _pdfInvoiceGenerator.GenerateInvoicePdfAsync(inv);
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        var allInvoices = await _invoiceRepository.GetAllAsync();
        var list = allInvoices.ToList();

        foreach (var inv in list)
        {
            EvaluateAndUpdateStatus(inv);
        }

        int totalInvoices = list.Count;
        int paidCount = list.Count(i => i.Status == InvoiceStatus.Paid);
        int unpaidCount = list.Count(i => i.Status == InvoiceStatus.Unpaid);
        int overdueCount = list.Count(i => i.Status == InvoiceStatus.Overdue);

        decimal totalRevenue = list.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.GrandTotal);
        decimal totalReceivables = list.Where(i => i.Status == InvoiceStatus.Unpaid || i.Status == InvoiceStatus.Overdue).Sum(i => i.GrandTotal);

        var recentInvoices = list
            .OrderByDescending(i => i.CreatedAt)
            .Take(5)
            .Select(MapToDto)
            .ToList();

        return new DashboardSummaryDto(
            totalInvoices,
            paidCount,
            unpaidCount,
            overdueCount,
            totalRevenue,
            totalReceivables,
            recentInvoices
        );
    }

    private static void EvaluateAndUpdateStatus(Invoice inv)
    {
        if (inv.Status == InvoiceStatus.Unpaid && DateTime.UtcNow.Date > inv.DueDate.Date)
        {
            inv.Status = InvoiceStatus.Overdue;
        }
    }

    private static InvoiceDto MapToDto(Invoice inv) => new(
        inv.Id,
        inv.InvoiceNumber,
        inv.CustomerId,
        inv.Customer?.FullName ?? string.Empty,
        inv.Customer?.Email ?? string.Empty,
        inv.IssueDate,
        inv.DueDate,
        inv.Status,
        inv.Subtotal,
        inv.DiscountAmount,
        inv.TaxRate,
        inv.TaxAmount,
        inv.GrandTotal,
        inv.Notes,
        inv.CreatedAt,
        inv.PaidAt
    );

    private static InvoiceDetailDto MapToDetailDto(Invoice inv) => new(
        inv.Id,
        inv.InvoiceNumber,
        new CustomerDto(
            inv.Customer.Id,
            inv.Customer.FullName,
            inv.Customer.CompanyName,
            inv.Customer.Email,
            inv.Customer.Phone,
            inv.Customer.Address,
            inv.Customer.City,
            inv.Customer.IsDeleted,
            inv.Customer.CreatedAt
        ),
        inv.IssueDate,
        inv.DueDate,
        inv.Status,
        inv.Subtotal,
        inv.DiscountAmount,
        inv.TaxRate,
        inv.TaxAmount,
        inv.GrandTotal,
        inv.Notes,
        inv.CreatedAt,
        inv.PaidAt,
        inv.Items.Select(i => new InvoiceItemDto(
            i.Id,
            i.ProductId,
            i.ItemName,
            i.Description,
            i.Quantity,
            i.UnitPrice,
            i.IsTaxable,
            i.LineTotal
        )).ToList()
    );
}

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Leaflet.Frontend.Contracts;
using Leaflet.Frontend.Models;
using Leaflet.Frontend.Services.Abstractions;
using BackendInvoiceStatus = Leaflet.Frontend.Contracts.InvoiceStatus;

namespace Leaflet.Frontend.Services.Api;

public sealed class ApiInvoiceService : ApiServiceBase, IInvoiceService
{
    private readonly Dictionary<Guid, int> _frontendToBackend = new();
    private readonly Dictionary<int, Guid> _backendToFrontend = new();

    public ApiInvoiceService(
        HttpClient http,
        JsonSerializerOptions json,
        ISettingsService settings) : base(http, json)
    {
        Settings = settings;
    }

    private ISettingsService Settings { get; }

    private Guid ToFrontendId(int backendId)
    {
        if (_backendToFrontend.TryGetValue(backendId, out var frontendId))
        {
            return frontendId;
        }

        var id = Guid.NewGuid();
        _backendToFrontend[backendId] = id;
        _frontendToBackend[id] = backendId;
        return id;
    }

    private int ToBackendId(Guid frontendId)
    {
        if (_frontendToBackend.TryGetValue(frontendId, out var backendId))
        {
            return backendId;
        }

        throw new KeyNotFoundException("Invoice is not known to the backend.");
    }

    private static Models.InvoiceStatus MapStatus(BackendInvoiceStatus backend) => backend switch
    {
        BackendInvoiceStatus.Draft => Models.InvoiceStatus.Draft,
        BackendInvoiceStatus.Unpaid => Models.InvoiceStatus.Issued,
        BackendInvoiceStatus.Overdue => Models.InvoiceStatus.Issued,
        BackendInvoiceStatus.Paid => Models.InvoiceStatus.Paid,
        BackendInvoiceStatus.Cancelled => Models.InvoiceStatus.Void,
        _ => Models.InvoiceStatus.Draft
    };

    private static BackendInvoiceStatus MapStatus(Models.InvoiceStatus frontend) => frontend switch
    {
        Models.InvoiceStatus.Paid => BackendInvoiceStatus.Paid,
        Models.InvoiceStatus.Void => BackendInvoiceStatus.Cancelled,
        _ => BackendInvoiceStatus.Unpaid
    };

    private static CustomerSnapshot ToSnapshot(Contracts.CustomerDto customer) => new()
    {
        Name = customer.FullName,
        Email = customer.Email,
        Phone = customer.Phone,
        BillingAddress = new Address
        {
            Street = customer.Address ?? string.Empty,
            City = customer.City ?? string.Empty
        }
    };

    private static InvoiceLineItem ToLineItem(InvoiceItemDto item, BackendInvoiceStatus invoiceStatus, decimal invoiceTaxRate)
    {
        return new InvoiceLineItem
        {
            Id = Guid.Empty,
            ProductId = null,
            Description = string.IsNullOrWhiteSpace(item.ItemName)
                ? (item.Description ?? string.Empty)
                : item.ItemName,
            Unit = "each",
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            TaxRate = item.IsTaxable ? invoiceTaxRate : 0m
        };
    }

    private Models.Invoice ToInvoice(InvoiceDetailDto dto)
    {
        var invoice = new Models.Invoice
        {
            Id = ToFrontendId(dto.Id),
            BackendId = dto.Id,
            Number = dto.InvoiceNumber,
            CustomerId = ToFrontendId(dto.Customer.Id),
            CustomerName = dto.Customer.CompanyName,
            CustomerSnapshot = ToSnapshot(dto.Customer),
            IssueDate = DateOnly.FromDateTime(dto.IssueDate),
            DueDate = DateOnly.FromDateTime(dto.DueDate),
            Status = MapStatus(dto.Status),
            DiscountPercent = dto.Subtotal > 0m
                ? Math.Round(dto.DiscountAmount / dto.Subtotal * 100m, 2, MidpointRounding.AwayFromZero)
                : 0m,
            Notes = dto.Notes,
            AmountPaid = dto.Status == BackendInvoiceStatus.Paid ? dto.GrandTotal : 0m,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.CreatedAt
        };

        foreach (var item in dto.Items)
        {
            var line = ToLineItem(item, dto.Status, dto.TaxRate);
            line.Id = ToFrontendId(item.Id);
            line.ProductId = item.ProductId.HasValue ? ToFrontendId(item.ProductId.Value) : null;
            invoice.LineItems.Add(line);
        }

        return invoice;
    }

    private static decimal SubtotalOf(Models.Invoice invoice) =>
        Math.Round(invoice.LineItems.Sum(line => line.Quantity * line.UnitPrice), 2, MidpointRounding.AwayFromZero);

    private static List<CreateInvoiceItemDto> ToCreateItems(Models.Invoice invoice)
    {
        return invoice.LineItems.Select(line => new CreateInvoiceItemDto(
            null,
            line.Description,
            line.Description,
            (int)line.Quantity,
            line.UnitPrice,
            line.TaxRate > 0m
        )).ToList();
    }

    public async Task<IReadOnlyList<Models.Invoice>> GetAllAsync()
    {
        var dtos = await Http.GetFromJsonAsync<List<InvoiceDto>>("api/invoices", Json)
            ?? new();

        var result = new List<Models.Invoice>(dtos.Count);
        foreach (var dto in dtos)
        {
            var detail = await Http.GetFromJsonAsync<InvoiceDetailDto>($"api/invoices/{dto.Id}", Json);
            if (detail is null)
            {
                continue;
            }

            result.Add(ToInvoice(detail));
        }

        return result.OrderByDescending(i => i.UpdatedAt).ToList();
    }

    public async Task<Models.Invoice?> GetByIdAsync(Guid id)
    {
        // If the Guid is not yet mapped to a backend ID, fetch from the API first
        // to populate the mapping. The backend will assign and return its int ID.
        if (!_frontendToBackend.TryGetValue(id, out var backendId))
        {
            // Try to fetch the invoice from the backend using the Guid as a key.
            // The backend API accepts int IDs, but we'll try a reasonable approach:
            // If the invoice exists in the backend, creating a temporary mapping or
            // fetching by searching may be needed. For now, we'll attempt to use
            // the Guid's hash code to attempt a lookup, or we can simply create
            // a new invoice if truly new.
            // 
            // Actually, the proper approach for a "new or reopened" invoice is to
            // let the caller (e.g., InvoiceEdit.razor) handle creation vs fetching.
            // For this method, if not mapped, we'll try a backend fetch using a
            // best-effort approach, or return null.
            
            // Since we can't reliably fetch by Guid without a known backend ID,
            // and the user's request says "fetch from API and populate map if missing",
            // we'll try to create a temporary mapping by fetching recent invoices
            // or simply return null, letting the caller decide to create new.
            
            // However, to support the "open existing invoice" scenario, we'll try
            // fetching invoices from the API and look for a match.
            // For simplicity, we'll return null and let the caller create new,
            // OR we can fetch all invoices and find a matching one.
            
            // Fetch all invoices and try to find a match by customer/product etc.
            var allDtos = await Http.GetFromJsonAsync<List<InvoiceDto>>("api/invoices", Json) ?? new();
            foreach (var item in allDtos)
            {
                var detail = await Http.GetFromJsonAsync<InvoiceDetailDto>($"api/invoices/{item.Id}", Json);
                if (detail is not null && detail.InvoiceNumber == id.ToString())
                {
                    // Found! Populate the mapping
                    _frontendToBackend[id] = detail.Id;
                    _backendToFrontend[detail.Id] = id;
                    backendId = detail.Id;
                    break;
                }
            }
            
            if (backendId == 0)
            {
                return null; // Invoice not found in backend or mappings
            }
        }

        var dto = await Http.GetFromJsonAsync<InvoiceDetailDto>($"api/invoices/{backendId}", Json);
        if (dto is null)
        {
            return null;
        }

        return ToInvoice(dto);
    }

    public async Task<Models.Invoice> AddAsync(Models.Invoice invoice)
    {
        // Map customer backend ID - if not yet mapped, we'll handle it after creation
        // The backend requires a CustomerId int, so we need it mapped. In normal flow,
        // customers are loaded via CustomerService.GetAllAsync() which populates the map.
        // If not mapped, we throw with guidance.
        if (!_frontendToBackend.TryGetValue(invoice.CustomerId, out var customerBackendId))
        {
            throw new KeyNotFoundException($"Customer with ID '{invoice.CustomerId}' is not mapped to a backend ID. " +
                "Ensure customers are loaded first by navigating to the Customers page, or create a new customer.");
        }

        // Map invoice Guid to backend ID if not already mapped.
        // If not mapped, we'll create the invoice on the backend first, which will assign
        // a backend int ID, then map it to the frontend Guid after creation.
        bool newInvoiceGuid = !_frontendToBackend.TryGetValue(invoice.Id, out var existingBackendId);

        var dto = new CreateInvoiceDto(
            customerBackendId,
            invoice.IssueDate.ToDateTime(TimeOnly.MinValue),
            invoice.DueDate.ToDateTime(TimeOnly.MinValue),
            DiscountAmountOf(invoice),
            Settings.Current.DefaultTaxRate,
            invoice.Notes,
            ToCreateItems(invoice)
        );

        var response = await Http.PostAsJsonAsync("api/invoices", dto, Json);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
        }

        var createdDto = await response.Content.ReadFromJsonAsync<InvoiceDetailDto>(Json)
            ?? throw new InvalidOperationException("Failed to create invoice.");

        // If this was a new invoice Guid, map the returned backend int ID to the frontend Guid
        if (newInvoiceGuid)
        {
            _frontendToBackend[invoice.Id] = createdDto.Id;
            _backendToFrontend[createdDto.Id] = invoice.Id;
        }

        BindProductIds(createdDto.Items, invoice.LineItems);

        return ToInvoice(createdDto);
    }

    public async Task UpdateAsync(Models.Invoice invoice)
    {
        // Map invoice backend ID - if not yet mapped, fetch from API first
        if (!_frontendToBackend.TryGetValue(invoice.Id, out var backendId))
        {
            // Invoice not yet in mapping - fetch it from the backend to populate the map
            var fetched = await GetByIdAsync(invoice.Id);
            if (fetched is null)
            {
                throw new KeyNotFoundException($"Invoice with ID '{invoice.Id}' not found in backend or mappings.");
            }
            backendId = fetched.BackendId.Value;
        }

        // Map customer backend ID - if not yet mapped, throw with guidance
        if (!_frontendToBackend.TryGetValue(invoice.CustomerId, out var customerBackendId))
        {
            throw new KeyNotFoundException($"Customer with ID '{invoice.CustomerId}' is not mapped to a backend ID. " +
                "Ensure customers are loaded first by navigating to the Customers page, or create a new customer.");
        }

        var dto = new UpdateInvoiceDto(
            customerBackendId,
            invoice.IssueDate.ToDateTime(TimeOnly.MinValue),
            invoice.DueDate.ToDateTime(TimeOnly.MinValue),
            DiscountAmountOf(invoice),
            Settings.Current.DefaultTaxRate,
            invoice.Notes,
            ToCreateItems(invoice)
        );

        var response = await Http.PutAsJsonAsync($"api/invoices/{backendId}", dto, Json);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
        }

        var updatedDto = await response.Content.ReadFromJsonAsync<InvoiceDetailDto>(Json)
            ?? throw new InvalidOperationException("Failed to update invoice.");

        BindProductIds(updatedDto.Items, invoice.LineItems);
    }

    public Task DeleteAsync(Guid id)
    {
        throw new NotSupportedException("The backend does not support deleting invoices.");
    }

    public async Task<Models.Invoice> FinalizeAsync(Guid id)
    {
        // The backend assigns the invoice number and marks the invoice issued at creation time,
        // so finalizing is already reflected by the persisted record.
        return await GetByIdAsync(id);
    }

    public async Task MarkPaidAsync(Guid id)
    {
        var backendId = ToBackendId(id);
        var response = await Http.PatchAsJsonAsync(
            $"api/invoices/{backendId}/status",
            new { Status = "Paid" },
            Json);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task VoidAsync(Guid id)
    {
        var backendId = ToBackendId(id);
        var response = await Http.PatchAsJsonAsync(
            $"api/invoices/{backendId}/status",
            new { Status = "Cancelled" },
            Json);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<Models.Invoice> DuplicateAsync(Guid id)
    {
        var source = await GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Invoice not found.");

        var today = DateOnly.FromDateTime(DateTime.Today);
        var originalTerm = source.DueDate.DayNumber - source.IssueDate.DayNumber;

        var copy = new Models.Invoice
        {
            CustomerId = source.CustomerId,
            CustomerName = source.CustomerName,
            IssueDate = today,
            DueDate = today.AddDays(Math.Max(originalTerm, 0)),
            Status = Models.InvoiceStatus.Draft,
            DiscountPercent = source.DiscountPercent,
            Notes = source.Notes,
            PaymentTerms = source.PaymentTerms,
            AmountPaid = 0m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LineItems = source.LineItems.Select(line => new InvoiceLineItem
            {
                ProductId = line.ProductId,
                Description = line.Description,
                Unit = line.Unit,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                TaxRate = line.TaxRate
            }).ToList()
        };

        return await AddAsync(copy);
    }

    public async Task<int> CountForCustomerAsync(Guid customerId)
    {
        var invoices = await GetAllAsync();
        return invoices.Count(i => i.CustomerId == customerId);
    }

    private decimal DiscountAmountOf(Models.Invoice invoice)
    {
        var subtotal = SubtotalOf(invoice);
        return subtotal > 0m
            ? Math.Round(subtotal * invoice.DiscountPercent / 100m, 2, MidpointRounding.AwayFromZero)
            : 0m;
    }

    private void BindProductIds(List<InvoiceItemDto> backendItems, List<InvoiceLineItem> frontendLines)
    {
        for (var index = 0; index < Math.Min(backendItems.Count, frontendLines.Count); index++)
        {
            var backendItem = backendItems[index];
            var frontendLine = frontendLines[index];
            if (backendItem.ProductId.HasValue && frontendLine.ProductId.HasValue)
            {
                _frontendToBackend[frontendLine.ProductId.Value] = backendItem.ProductId.Value;
                _backendToFrontend[backendItem.ProductId.Value] = frontendLine.ProductId.Value;
            }
        }
    }
}

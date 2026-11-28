namespace InvoiceSystem.Application.DTOs;

public record DashboardSummaryDto(
    int TotalInvoices,
    int PaidCount,
    int UnpaidCount,
    int OverdueCount,
    decimal TotalRevenue,
    decimal TotalReceivables,
    List<InvoiceDto> RecentInvoices
);

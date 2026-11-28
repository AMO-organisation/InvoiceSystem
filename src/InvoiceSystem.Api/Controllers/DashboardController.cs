using Microsoft.AspNetCore.Mvc;
using InvoiceSystem.Application.DTOs;
using InvoiceSystem.Application.Services;

namespace InvoiceSystem.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly InvoiceService _invoiceService;

    public DashboardController(InvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var summary = await _invoiceService.GetDashboardSummaryAsync();
        return Ok(summary);
    }
}

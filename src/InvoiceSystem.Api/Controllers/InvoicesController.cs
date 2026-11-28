using Microsoft.AspNetCore.Mvc;
using InvoiceSystem.Application.DTOs;
using InvoiceSystem.Application.Services;
using InvoiceSystem.Domain.Enums;

namespace InvoiceSystem.Api.Controllers;

[ApiController]
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly InvoiceService _invoiceService;

    public InvoicesController(InvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpPost("calculate-preview")]
    public async Task<ActionResult<InvoicePreviewResultDto>> CalculatePreview([FromBody] CalculatePreviewDto dto)
    {
        var result = await _invoiceService.CalculatePreviewAsync(dto);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceDetailDto>> Create([FromBody] CreateInvoiceDto dto)
    {
        var invoice = await _invoiceService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, invoice);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetAll(
        [FromQuery] InvoiceStatus? status,
        [FromQuery] string? search,
        [FromQuery] int? customerId)
    {
        var invoices = await _invoiceService.GetAllAsync(status, search, customerId);
        return Ok(invoices);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InvoiceDetailDto>> GetById(int id)
    {
        var invoice = await _invoiceService.GetByIdAsync(id);
        return Ok(invoice);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<InvoiceDetailDto>> Update(int id, [FromBody] UpdateInvoiceDto dto)
    {
        var invoice = await _invoiceService.UpdateAsync(id, dto);
        return Ok(invoice);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<InvoiceDetailDto>> UpdateStatus(int id, [FromBody] UpdateInvoiceStatusDto dto)
    {
        var invoice = await _invoiceService.UpdateStatusAsync(id, dto.Status);
        return Ok(invoice);
    }

    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        var pdfBytes = await _invoiceService.GetPdfBytesAsync(id);
        var invoice = await _invoiceService.GetByIdAsync(id);
        return File(pdfBytes, "application/pdf", $"{invoice.InvoiceNumber}.pdf");
    }
}

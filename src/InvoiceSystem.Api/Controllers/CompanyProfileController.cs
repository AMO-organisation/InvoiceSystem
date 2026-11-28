using Microsoft.AspNetCore.Mvc;
using InvoiceSystem.Application.DTOs;
using InvoiceSystem.Application.Services;

namespace InvoiceSystem.Api.Controllers;

[ApiController]
[Route("api/company-profile")]
public class CompanyProfileController : ControllerBase
{
    private readonly ICompanyProfileService _service;

    public CompanyProfileController(ICompanyProfileService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<CompanyProfileDto>> Get()
    {
        var profile = await _service.GetProfileAsync();
        var dto = new CompanyProfileDto(
            profile.Id,
            profile.CompanyName,
            profile.Address,
            profile.City,
            profile.Email,
            profile.Phone,
            profile.CurrencySymbol,
            profile.DefaultTaxRate,
            profile.PaymentInstructions,
            profile.UpdatedAt);
        return Ok(dto);
    }

    [HttpPut]
    public async Task<ActionResult<CompanyProfileDto>> Put([FromBody] UpdateCompanyProfileRequest request)
    {
        var profile = await _service.UpdateProfileAsync(request);
        var dto = new CompanyProfileDto(
            profile.Id,
            profile.CompanyName,
            profile.Address,
            profile.City,
            profile.Email,
            profile.Phone,
            profile.CurrencySymbol,
            profile.DefaultTaxRate,
            profile.PaymentInstructions,
            profile.UpdatedAt);
        return Ok(dto);
    }
}
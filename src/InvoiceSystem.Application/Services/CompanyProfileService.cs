using Microsoft.EntityFrameworkCore;
using InvoiceSystem.Application.DTOs;
using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Infrastructure.Data;

namespace InvoiceSystem.Application.Services;

public class CompanyProfileService : ICompanyProfileService
{
    private readonly AppDbContext _context;

    public CompanyProfileService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyProfile> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        var profile = await _context.CompanyProfiles.FirstOrDefaultAsync(cancellationToken);
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
            await _context.SaveChangesAsync(cancellationToken);
        }

        return profile;
    }

    public async Task<CompanyProfile> UpdateProfileAsync(
        UpdateCompanyProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var profile = await _context.CompanyProfiles.FirstOrDefaultAsync(cancellationToken);
        if (profile == null)
        {
            profile = new CompanyProfile();
            _context.CompanyProfiles.Add(profile);
        }

        profile.CompanyName = request.CompanyName;
        profile.Address = request.Address;
        profile.City = request.City;
        profile.Email = request.Email;
        profile.Phone = request.Phone;
        profile.CurrencySymbol = request.CurrencySymbol;
        profile.DefaultTaxRate = request.DefaultTaxRate;
        profile.PaymentInstructions = request.PaymentInstructions;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return profile;
    }
}

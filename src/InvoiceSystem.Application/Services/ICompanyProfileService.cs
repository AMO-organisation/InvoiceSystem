using InvoiceSystem.Application.DTOs;
using InvoiceSystem.Domain.Entities;

namespace InvoiceSystem.Application.Services;

public interface ICompanyProfileService
{
    Task<CompanyProfile> GetProfileAsync(CancellationToken cancellationToken = default);
    Task<CompanyProfile> UpdateProfileAsync(UpdateCompanyProfileRequest request, CancellationToken cancellationToken = default);
}
using System.ComponentModel.DataAnnotations;

namespace Leaflet.Frontend.Models;

public sealed class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Customer name is required.")]
    [StringLength(120, ErrorMessage = "Customer name must be 120 characters or fewer.")]
    public string Name { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(160)]
    public string? Email { get; set; }

    [StringLength(40)]
    public string? Phone { get; set; }

    public Address BillingAddress { get; set; } = new();

    [StringLength(40)]
    public string? TaxId { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Customer Clone() => new()
    {
        Id = Id,
        Name = Name,
        Email = Email,
        Phone = Phone,
        BillingAddress = BillingAddress.Clone(),
        TaxId = TaxId,
        Notes = Notes,
        CreatedAt = CreatedAt
    };
}

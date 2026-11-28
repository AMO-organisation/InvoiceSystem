using System.ComponentModel.DataAnnotations;

namespace Leaflet.Frontend.Models;

public sealed class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Product or service name is required.")]
    [StringLength(120, ErrorMessage = "Name must be 120 characters or fewer.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(0, 1_000_000, ErrorMessage = "Unit price must be between 0 and 1,000,000.")]
    public decimal UnitPrice { get; set; }

    [Required(ErrorMessage = "Unit is required.")]
    [StringLength(24)]
    public string Unit { get; set; } = "each";

    [Range(0, 100, ErrorMessage = "Tax rate must be between 0 and 100.")]
    public decimal TaxRate { get; set; }

    public bool IsService { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Product Clone() => (Product)MemberwiseClone();
}

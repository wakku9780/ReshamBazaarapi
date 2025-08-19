using System.ComponentModel.DataAnnotations;

namespace ReshamBazaar.Api.Models;

public enum DiscountType
{
    Percent = 1,
    Fixed = 2
}

public class Coupon
{
    public int Id { get; set; }

    [Required]
    [MaxLength(32)]
    public string Code { get; set; } = string.Empty; // uppercase code

    public DiscountType Type { get; set; }

    [Range(0, 100000)]
    public decimal Amount { get; set; } // percent (0-100) for Percent type; currency for Fixed type

    public DateTime? ExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;

    public decimal? MinOrderAmount { get; set; }

    public decimal? MaxDiscount { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReshamBazaar.Api.Models;

public class Review
{
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    public string UserId { get; set; } = default!;

    [Range(1,5)]
    public int Rating { get; set; }

    [MaxLength(2000)]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }
}

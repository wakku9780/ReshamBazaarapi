using System.ComponentModel.DataAnnotations.Schema;

namespace ReshamBazaar.Api.Models;

public class CartItem
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int ProductId { get; set; }
    public int Quantity { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }
}

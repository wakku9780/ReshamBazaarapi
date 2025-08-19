namespace ReshamBazaar.Api.Models;

public class Order
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }
    public decimal Discount { get; set; }
    public decimal FinalTotal { get; set; }
    public string? CouponCode { get; set; }
    public string? ShippingAddress { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}

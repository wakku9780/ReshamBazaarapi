namespace ReshamBazaar.Api.DTOs;

public record OrderItemReadDto(int ProductId, string ProductName, decimal UnitPrice, int Quantity);
public record OrderReadDto(int Id, DateTime CreatedAt, decimal Total, decimal Discount, decimal FinalTotal, string? CouponCode, List<OrderItemReadDto> Items);
public record CheckoutRequestDto(string? CouponCode, string? ShippingAddress);

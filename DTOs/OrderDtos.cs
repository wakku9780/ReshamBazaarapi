namespace ReshamBazaar.Api.DTOs;

using ReshamBazaar.Api.Models;

public record OrderItemReadDto(int ProductId, string ProductName, decimal UnitPrice, int Quantity);
public record OrderReadDto(int Id, DateTime CreatedAt, decimal Total, decimal Discount, decimal FinalTotal, string? CouponCode, OrderStatus Status, List<OrderItemReadDto> Items);
public record CheckoutRequestDto(string? CouponCode, AddressDto? Address);
public record AddressDto(
    string FullName,
    string Phone,
    string Line1,
    string? Line2,
    string City,
    string Pincode
);
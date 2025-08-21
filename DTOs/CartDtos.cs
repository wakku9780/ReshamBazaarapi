namespace ReshamBazaar.Api.DTOs;

public record CartItemRequestDto(int ProductId, int Quantity);
public record CartItemReadDto(int Id, int ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);
public record CartSummaryDto(decimal Subtotal, decimal Discount, decimal FinalTotal, IReadOnlyList<CartItemReadDto> Items);

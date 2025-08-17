namespace ReshamBazaar.Api.DTOs;

public record CartItemRequestDto(int ProductId, int Quantity);
public record CartItemReadDto(int Id, int ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);

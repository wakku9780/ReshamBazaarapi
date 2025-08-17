namespace ReshamBazaar.Api.DTOs;

public record OrderItemReadDto(int ProductId, string ProductName, decimal UnitPrice, int Quantity);
public record OrderReadDto(int Id, DateTime CreatedAt, decimal Total, List<OrderItemReadDto> Items);

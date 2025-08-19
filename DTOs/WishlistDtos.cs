namespace ReshamBazaar.Api.DTOs;

public record WishlistItemReadDto(int Id, int ProductId, string ProductName, decimal Price, string ImageUrl, DateTime CreatedAt);

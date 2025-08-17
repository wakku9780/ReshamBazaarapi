namespace ReshamBazaar.Api.DTOs;

public record ProductCreateDto(string Name, string Description, string Color, decimal Price, int Stock, string ImageUrl);
public record ProductUpdateDto(string Name, string Description, string Color, decimal Price, int Stock, string ImageUrl);
public record ProductReadDto(int Id, string Name, string Description, string Color, decimal Price, int Stock, string ImageUrl);

namespace ReshamBazaar.Api.DTOs;

public record ReviewCreateDto(int Rating, string? Comment);
public record ReviewReadDto(int Id, int ProductId, string UserId, int Rating, string? Comment, DateTime CreatedAt);

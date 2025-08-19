using System.ComponentModel.DataAnnotations;

namespace ReshamBazaar.Api.DTOs;

public record ProductCreateUpdateDto(
    [param: Required, MaxLength(200)] string Name,
    [param: MaxLength(100)] string? Category,
    [param: MaxLength(50)] string? Color,
    [param: MaxLength(1000)] string? Description,
    [param: Range(0, 100000000)] decimal Price,
    [param: Range(0, int.MaxValue)] int Stock
);

public record ProductListQuery(string? Search, string? Category, decimal? MinPrice, decimal? MaxPrice, int Page = 1, int PageSize = 20);

public record UserSummaryDto(string Id, string Email, string? FullName, bool EmailConfirmed, bool IsBlocked, IEnumerable<string> Roles);

public record OrderStatusUpdateDto([param: Required] string Status);

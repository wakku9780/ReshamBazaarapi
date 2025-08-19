namespace ReshamBazaar.Api.DTOs;

public class ProductQuery
{
    public string? Q { get; set; }
    public string? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Price { get; set; } // friendly token e.g., under-1499, above-5000, 1500-4999
    public string? SortBy { get; set; } = "latest"; // price|name|latest
    public string? SortDir { get; set; } = "desc";  // asc|desc
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

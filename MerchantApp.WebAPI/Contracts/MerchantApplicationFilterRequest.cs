namespace MerchantApp.WebAPI.Contracts;

public sealed class MerchantApplicationFilterRequest
{
    public string? TaxNumber { get; set; }
    public string? WorkplaceSignboardName { get; set; }
    public string? Status { get; set; }
    public string? City { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

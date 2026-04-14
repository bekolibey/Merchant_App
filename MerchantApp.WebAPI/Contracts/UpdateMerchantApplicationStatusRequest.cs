namespace MerchantApp.WebAPI.Contracts;

public sealed class UpdateMerchantApplicationStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UserCode { get; set; } = string.Empty;
    public string HistoryDescription { get; set; } = string.Empty;
}

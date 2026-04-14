namespace merchant.domain.Entities;

public sealed class MerchantApplicationHistory
{
    public Guid Id { get; set; }
    public Guid MerchantApplicationId { get; set; }
    public DateTime ProcessDate { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string UserCode { get; set; } = string.Empty;
    public string HistoryDescription { get; set; } = string.Empty;
    public son_calisma_merchantapp.Entities.Merchantdetails? MerchantApplication { get; set; }
}

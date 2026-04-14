namespace merchant.domain.Entities;

public sealed class MerchantDocument
{
    public Guid Id { get; set; }
    public Guid MerchantApplicationId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public son_calisma_merchantapp.Entities.Merchantdetails? MerchantApplication { get; set; }
}

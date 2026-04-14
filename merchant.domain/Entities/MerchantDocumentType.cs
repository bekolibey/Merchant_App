namespace merchant.domain.Entities;

public sealed class MerchantDocumentType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsRequired { get; set; }
}

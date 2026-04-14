namespace MerchantApp.WebAPI.Contracts;

public sealed class CreateMerchantApplicationRequest
{
    public string ApplicationNumber { get; set; } = string.Empty;
    public string WorkflowReferenceNumber { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string CustomerNumber { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = string.Empty;
    public string CompanyType { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public bool IsSpecialRulingRequired { get; set; }
    public string DemandDepositAccountNumber { get; set; } = string.Empty;
    public string WorkplaceSignboardName { get; set; } = string.Empty;
    public string ContractedUserCode { get; set; } = string.Empty;
    public string TradeRegistryNumber { get; set; } = string.Empty;
    public DateTime? TradeRegistryRegistrationDate { get; set; }
    public string ManagerFullName { get; set; } = string.Empty;
    public string TradeRegistryRegistrationName { get; set; } = string.Empty;
    public string TaxOffice { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public string PosInstallationAddress { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string GsmNumber { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? WebAddress { get; set; }
    public string OwnerIdentityNumber { get; set; } = string.Empty;
    public string OwnerFullName { get; set; } = string.Empty;
    public string OwnerMobilePhone { get; set; } = string.Empty;
    public List<CreateMerchantApplicationDocumentRequest> Documents { get; set; } = [];
}

public sealed class CreateMerchantApplicationDocumentRequest
{
    public string DocumentType { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
}

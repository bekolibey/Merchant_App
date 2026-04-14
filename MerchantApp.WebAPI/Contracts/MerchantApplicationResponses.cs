namespace MerchantApp.WebAPI.Contracts;

public sealed class MerchantApplicationListItemResponse
{
    public Guid Id { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public string WorkplaceSignboardName { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class MerchantApplicationHistoryResponse
{
    public Guid Id { get; set; }
    public DateTime ProcessDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string UserCode { get; set; } = string.Empty;
    public string HistoryDescription { get; set; } = string.Empty;
}

public sealed class MerchantDocumentResponse
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
}

public sealed class MerchantApplicationDetailResponse
{
    public Guid Id { get; set; }
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
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ExchangeRateSnapshotResponse ExchangeRates { get; set; } = new();
    public List<MerchantDocumentResponse> Documents { get; set; } = [];
    public List<MerchantApplicationHistoryResponse> Histories { get; set; } = [];
}

public sealed class PagedResponse<T>
{
    public IReadOnlyCollection<T> Items { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

public sealed class MerchantApplicationReportResponse
{
    public int TotalApplications { get; set; }
    public Dictionary<string, int> StatusCounts { get; set; } = [];
    public Dictionary<string, int> CityCounts { get; set; } = [];
}

public sealed class ExchangeRateSnapshotResponse
{
    public decimal? UsdToTryRate { get; set; }
    public decimal? EurToTryRate { get; set; }
    public DateTime? FetchedAtUtc { get; set; }
    public string Source { get; set; } = string.Empty;
}

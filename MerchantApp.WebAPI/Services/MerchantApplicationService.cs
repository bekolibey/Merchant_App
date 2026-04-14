using System.Text.RegularExpressions;
using System.Globalization;
using System.Data;
using MerchantApp.WebAPI.Contracts;
using merchant.domain.Constants;
using merchant.domain.Entities;
using merchantapp.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using son_calisma_merchantapp.Entities;

namespace MerchantApp.WebAPI.Services;

public interface IMerchantApplicationService
{
    Task<MerchantApplicationDetailResponse> CreateAsync(CreateMerchantApplicationRequest request, CancellationToken cancellationToken);
    Task<PagedResponse<MerchantApplicationListItemResponse>> GetPagedAsync(MerchantApplicationFilterRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<MerchantApplicationDetailResponse>> GetExportItemsAsync(MerchantApplicationFilterRequest request, CancellationToken cancellationToken);
    Task<MerchantApplicationDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> DeleteByApplicationNumberAsync(string applicationNumber, CancellationToken cancellationToken);
    Task<bool> TradeRegistryNumberExistsAsync(string tradeRegistryNumber, CancellationToken cancellationToken);
    Task<MerchantApplicationDetailResponse?> UpdateStatusAsync(Guid id, UpdateMerchantApplicationStatusRequest request, CancellationToken cancellationToken);
    Task<MerchantApplicationReportResponse> GetReportAsync(CancellationToken cancellationToken);
    Task<int> AutoRejectPendingApplicationsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<string>> GetDocumentTypesAsync(CancellationToken cancellationToken);
    Task<ExchangeRateSnapshotResponse> GetExchangeRatesAsync(CancellationToken cancellationToken);
}

public sealed class MerchantApplicationService(
    ApplicationDbContext context,
    IOptions<MerchantApplicationOptions> options) : IMerchantApplicationService
{
    private static readonly Regex TaxNumberRegex = new(@"^\d{10}$", RegexOptions.Compiled);
    private static readonly Regex IdentityRegex = new(@"^\d{11}$", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"^[0-9+\-\s()]{10,15}$", RegexOptions.Compiled);
    private static readonly Regex PostalCodeRegex = new(@"^\d{5}$", RegexOptions.Compiled);
    private readonly MerchantApplicationOptions _options = options.Value;

    public async Task<MerchantApplicationDetailResponse> CreateAsync(CreateMerchantApplicationRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        request.ApplicationNumber = await GenerateNextApplicationNumberAsync(cancellationToken);

        Dictionary<string, string[]> errors = await ValidateCreateRequestAsync(request, cancellationToken);
        if (errors.Count > 0)
        {
            throw new MerchantApplicationValidationException(errors);
        }

        Guid applicationId = Guid.NewGuid();

        Merchantdetails entity = new()
        {
            Id = applicationId,
            ApplicationNumber = request.ApplicationNumber.Trim(),
            WorkflowReferenceNumber = request.WorkflowReferenceNumber.Trim(),
            BranchCode = request.BranchCode.Trim(),
            BranchName = request.BranchName.Trim(),
            CustomerNumber = request.CustomerNumber.Trim(),
            IdentityNumber = request.IdentityNumber.Trim(),
            CompanyType = request.CompanyType.Trim(),
            TaxNumber = request.TaxNumber.Trim(),
            IsSpecialRulingRequired = request.IsSpecialRulingRequired,
            DemandDepositAccountNumber = request.DemandDepositAccountNumber.Trim(),
            WorkplaceSignboardName = request.WorkplaceSignboardName.Trim(),
            ContractedUserCode = request.ContractedUserCode.Trim(),
            TradeRegistryNumber = request.TradeRegistryNumber.Trim(),
            TradeRegistryRegistrationDate = request.TradeRegistryRegistrationDate,
            ManagerFullName = request.ManagerFullName.Trim(),
            TradeRegistryRegistrationName = request.TradeRegistryRegistrationName.Trim(),
            TaxOffice = request.TaxOffice.Trim(),
            CustomerAddress = request.CustomerAddress.Trim(),
            PosInstallationAddress = request.PosInstallationAddress.Trim(),
            Email = request.Email.Trim(),
            City = request.City.Trim(),
            District = request.District.Trim(),
            PostalCode = request.PostalCode.Trim(),
            GsmNumber = request.GsmNumber.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            WebAddress = NormalizeOptional(request.WebAddress),
            OwnerIdentityNumber = request.OwnerIdentityNumber.Trim(),
            OwnerFullName = request.OwnerFullName.Trim(),
            OwnerMobilePhone = request.OwnerMobilePhone.Trim(),
            ApplicationStatus = MerchantApplicationStatuses.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        entity.Documents = request.Documents
            .Where(document => !string.IsNullOrWhiteSpace(document.DocumentType))
            .Select(document => new MerchantDocument
            {
                Id = Guid.NewGuid(),
                MerchantApplicationId = applicationId,
                DocumentType = document.DocumentType.Trim(),
                OriginalFileName = string.IsNullOrWhiteSpace(document.OriginalFileName) ? "unknown" : document.OriginalFileName.Trim(),
                ContentType = string.IsNullOrWhiteSpace(document.ContentType) ? "application/octet-stream" : document.ContentType.Trim(),
                FileSize = Math.Max(0, document.FileSize),
                UploadedAt = DateTime.UtcNow
            })
            .ToList();

        entity.Histories.Add(new MerchantApplicationHistory
        {
            Id = Guid.NewGuid(),
            MerchantApplicationId = applicationId,
            ProcessDate = DateTime.UtcNow,
            Description = "Basvuru girisi",
            Status = MerchantApplicationStatuses.Pending,
            UserCode = entity.ContractedUserCode,
            HistoryDescription = "Basvuru islemi baslatildi"
        });

        context.MerchantApplications.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        ExchangeRateSnapshotResponse exchangeRateSnapshot = await GetExchangeRatesAsync(cancellationToken);
        return MapDetail(entity, exchangeRateSnapshot);
    }

    public async Task<PagedResponse<MerchantApplicationListItemResponse>> GetPagedAsync(MerchantApplicationFilterRequest request, CancellationToken cancellationToken)
    {
        IQueryable<Merchantdetails> query = ApplyFilters(context.MerchantApplications.AsNoTracking(), request);

        int pageNumber = Math.Max(1, request.PageNumber);
        int pageSize = Math.Clamp(request.PageSize, 1, 100);
        int totalCount = await query.CountAsync(cancellationToken);

        List<MerchantApplicationListItemResponse> items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new MerchantApplicationListItemResponse
            {
                Id = x.Id,
                ApplicationNumber = x.ApplicationNumber,
                WorkplaceSignboardName = x.WorkplaceSignboardName,
                TaxNumber = x.TaxNumber,
                City = x.City,
                District = x.District,
                Status = x.ApplicationStatus,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<MerchantApplicationListItemResponse>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<IReadOnlyCollection<MerchantApplicationDetailResponse>> GetExportItemsAsync(MerchantApplicationFilterRequest request, CancellationToken cancellationToken)
    {
        List<Merchantdetails> entities = await ApplyFilters(context.MerchantApplications.AsNoTracking(), request)
            .Include(x => x.Histories)
            .Include(x => x.Documents)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        ExchangeRateSnapshotResponse exchangeRateSnapshot = await GetExchangeRatesAsync(cancellationToken);
        return entities
            .Select(entity => MapDetail(entity, exchangeRateSnapshot))
            .ToList();
    }

    public async Task<MerchantApplicationDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        Merchantdetails? entity = await context.MerchantApplications
            .AsNoTracking()
            .Include(x => x.Histories.OrderByDescending(h => h.ProcessDate))
            .Include(x => x.Documents.OrderByDescending(d => d.UploadedAt))
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        ExchangeRateSnapshotResponse exchangeRateSnapshot = await GetExchangeRatesAsync(cancellationToken);
        return MapDetail(entity, exchangeRateSnapshot);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        Merchantdetails? entity = await context.MerchantApplications.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        context.MerchantApplications.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteByApplicationNumberAsync(string applicationNumber, CancellationToken cancellationToken)
    {
        string normalized = applicationNumber.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        Merchantdetails? entity = await context.MerchantApplications
            .FirstOrDefaultAsync(x => x.ApplicationNumber == normalized, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        context.MerchantApplications.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> TradeRegistryNumberExistsAsync(string tradeRegistryNumber, CancellationToken cancellationToken)
    {
        string normalized = tradeRegistryNumber.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        return await context.MerchantApplications
            .AsNoTracking()
            .AnyAsync(x => x.TradeRegistryNumber == normalized, cancellationToken);
    }

    public async Task<MerchantApplicationDetailResponse?> UpdateStatusAsync(Guid id, UpdateMerchantApplicationStatusRequest request, CancellationToken cancellationToken)
    {
        Merchantdetails? entity = await context.MerchantApplications
            .Include(x => x.Histories)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (!MerchantApplicationStatuses.All.Contains(request.Status))
        {
            throw new MerchantApplicationValidationException(new Dictionary<string, string[]>
            {
                ["status"] = ["Gecersiz basvuru durumu."]
            });
        }

        entity.ApplicationStatus = request.Status;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.Histories.Add(new MerchantApplicationHistory
        {
            Id = Guid.NewGuid(),
            MerchantApplicationId = entity.Id,
            ProcessDate = DateTime.UtcNow,
            Description = string.IsNullOrWhiteSpace(request.Description) ? "Durum guncellendi" : request.Description.Trim(),
            Status = request.Status,
            UserCode = string.IsNullOrWhiteSpace(request.UserCode) ? entity.ContractedUserCode : request.UserCode.Trim(),
            HistoryDescription = string.IsNullOrWhiteSpace(request.HistoryDescription) ? "Durum bilgisi guncellendi" : request.HistoryDescription.Trim()
        });

        await context.SaveChangesAsync(cancellationToken);
        ExchangeRateSnapshotResponse exchangeRateSnapshot = await GetExchangeRatesAsync(cancellationToken);
        return MapDetail(entity, exchangeRateSnapshot);
    }

    public async Task<MerchantApplicationReportResponse> GetReportAsync(CancellationToken cancellationToken)
    {
        int totalApplications = await context.MerchantApplications.CountAsync(cancellationToken);

        Dictionary<string, int> statusCounts = await context.MerchantApplications
            .GroupBy(x => x.ApplicationStatus)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

        Dictionary<string, int> cityCounts = await context.MerchantApplications
            .GroupBy(x => x.City)
            .Select(group => new { group.Key, Count = group.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

        return new MerchantApplicationReportResponse
        {
            TotalApplications = totalApplications,
            StatusCounts = statusCounts,
            CityCounts = cityCounts
        };
    }

    public async Task<int> AutoRejectPendingApplicationsAsync(CancellationToken cancellationToken)
    {
        DateTime threshold = DateTime.UtcNow.AddHours(-_options.PendingTimeoutHours);

        List<Merchantdetails> applications = await context.MerchantApplications
            .Where(x => x.ApplicationStatus == MerchantApplicationStatuses.Pending && x.CreatedAt <= threshold)
            .ToListAsync(cancellationToken);

        foreach (Merchantdetails application in applications)
        {
            application.ApplicationStatus = MerchantApplicationStatuses.Rejected;
            application.UpdatedAt = DateTime.UtcNow;
            application.Histories.Add(new MerchantApplicationHistory
            {
                Id = Guid.NewGuid(),
                MerchantApplicationId = application.Id,
                ProcessDate = DateTime.UtcNow,
                Description = "Zaman asimi",
                Status = MerchantApplicationStatuses.Rejected,
                UserCode = "SYSTEM",
                HistoryDescription = "Basvuru 2 gunden fazla bekledigi icin sistem tarafindan otomatik reddedildi."
            });
        }

        if (applications.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        return applications.Count;
    }

    public async Task<IReadOnlyCollection<string>> GetDocumentTypesAsync(CancellationToken cancellationToken)
    {
        return await context.MerchantDocumentTypes
            .AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<ExchangeRateSnapshotResponse> GetExchangeRatesAsync(CancellationToken cancellationToken)
    {
        ExchangeRateSnapshot? snapshot = await context.ExchangeRateSnapshots
            .AsNoTracking()
            .OrderByDescending(x => x.FetchedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (snapshot is null)
        {
            return new ExchangeRateSnapshotResponse
            {
                UsdToTryRate = null,
                EurToTryRate = null,
                FetchedAtUtc = null,
                Source = "No snapshot"
            };
        }

        return new ExchangeRateSnapshotResponse
        {
            UsdToTryRate = snapshot.UsdToTryRate,
            EurToTryRate = snapshot.EurToTryRate,
            FetchedAtUtc = snapshot.FetchedAtUtc,
            Source = snapshot.Source
        };
    }

    private async Task<Dictionary<string, string[]>> ValidateCreateRequestAsync(CreateMerchantApplicationRequest request, CancellationToken cancellationToken)
    {
        Dictionary<string, List<string>> errors = new(StringComparer.OrdinalIgnoreCase);

        AddIf(string.IsNullOrWhiteSpace(request.WorkflowReferenceNumber), "workflowReferenceNumber", "Is akis referans no zorunludur.");
        AddIf(string.IsNullOrWhiteSpace(request.BranchCode), "branchCode", "Sube kodu zorunludur.");
        AddIf(string.IsNullOrWhiteSpace(request.BranchName), "branchName", "Sube adi zorunludur.");
        AddIf(string.IsNullOrWhiteSpace(request.CustomerNumber), "customerNumber", "Musteri no zorunludur.");
        AddIf(!IdentityRegex.IsMatch(request.IdentityNumber ?? string.Empty), "identityNumber", "TC kimlik no 11 haneli olmalidir.");
        AddIf(string.IsNullOrWhiteSpace(request.CompanyType), "companyType", "Sirket tipi zorunludur.");
        AddIf(!TaxNumberRegex.IsMatch(request.TaxNumber ?? string.Empty), "taxNumber", "Vergi no 10 haneli olmalidir.");
        AddIf(string.IsNullOrWhiteSpace(request.DemandDepositAccountNumber), "demandDepositAccountNumber", "Vadesiz hesap no zorunludur.");
        AddIf(string.IsNullOrWhiteSpace(request.WorkplaceSignboardName), "workplaceSignboardName", "Isyeri tabela adi zorunludur.");
        AddIf(string.IsNullOrWhiteSpace(request.ContractedUserCode), "contractedUserCode", "Sozlesme alan kullanici zorunludur.");
        AddIf(string.IsNullOrWhiteSpace(request.TradeRegistryNumber), "tradeRegistryNumber", "Ticari sicil no zorunludur.");
        AddIf(string.IsNullOrWhiteSpace(request.ManagerFullName), "managerFullName", "Yonetici adi soyadi zorunludur.");
        AddIf(string.IsNullOrWhiteSpace(request.TradeRegistryRegistrationName), "tradeRegistryRegistrationName", "Ticari sicil kayit adi zorunludur.");
        AddIf(string.IsNullOrWhiteSpace(request.TaxOffice), "taxOffice", "Vergi dairesi zorunludur.");
        AddIf(string.IsNullOrWhiteSpace(request.CustomerAddress), "customerAddress", "Musteri adres bilgisi zorunludur.");
        AddIf(string.IsNullOrWhiteSpace(request.PosInstallationAddress), "posInstallationAddress", "POS kurulum adresi zorunludur.");
        AddIf(string.IsNullOrWhiteSpace(request.Email) || !IsValidEmail(request.Email), "email", "Email formatı gecersiz.");
        AddIf(string.IsNullOrWhiteSpace(request.City), "city", "Il zorunludur.");
        AddIf(string.IsNullOrWhiteSpace(request.District), "district", "Ilce zorunludur.");
        AddIf(!PostalCodeRegex.IsMatch(request.PostalCode ?? string.Empty), "postalCode", "Posta kodu 5 haneli olmalidir.");
        AddIf(string.IsNullOrWhiteSpace(request.GsmNumber) || !PhoneRegex.IsMatch(request.GsmNumber), "gsmNumber", "GSM no gecerli formatta olmalidir.");
        AddIf(request.Latitude is < -90 or > 90, "latitude", "Latitude -90 ile 90 arasinda olmalidir.");
        AddIf(request.Longitude is < -180 or > 180, "longitude", "Longitude -180 ile 180 arasinda olmalidir.");
        AddIf(!IdentityRegex.IsMatch(request.OwnerIdentityNumber ?? string.Empty), "ownerIdentityNumber", "Isyeri sahibi TC kimlik no 11 haneli olmalidir.");
        AddIf(string.IsNullOrWhiteSpace(request.OwnerFullName), "ownerFullName", "Isyeri sahibi adi soyadi zorunludur.");
        AddIf(string.IsNullOrWhiteSpace(request.OwnerMobilePhone) || !PhoneRegex.IsMatch(request.OwnerMobilePhone), "ownerMobilePhone", "Isyeri sahibi cep telefonu gecerli formatta olmalidir.");

        bool duplicateTaxNumber = await context.MerchantApplications.AnyAsync(x => x.TaxNumber == request.TaxNumber, cancellationToken);
        AddIf(duplicateTaxNumber, "taxNumber", "Bu vergi numarasi ile daha once basvuru yapilmis.");
        string normalizedTradeRegistryNumber = (request.TradeRegistryNumber ?? string.Empty).Trim();
        bool duplicateTradeRegistryNumber = await context.MerchantApplications
            .AnyAsync(x => x.TradeRegistryNumber == normalizedTradeRegistryNumber, cancellationToken);
        AddIf(duplicateTradeRegistryNumber, "tradeRegistryNumber", "Bu ticari sicil no ile daha once basvuru yapilmis.");

        return errors.ToDictionary(x => x.Key, x => x.Value.Distinct().ToArray(), StringComparer.OrdinalIgnoreCase);

        void AddIf(bool condition, string key, string message)
        {
            if (!condition)
            {
                return;
            }

            if (!errors.TryGetValue(key, out List<string>? errorList))
            {
                errorList = [];
                errors[key] = errorList;
            }

            errorList.Add(message);
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            _ = new System.Net.Mail.MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private async Task<string> GenerateNextApplicationNumberAsync(CancellationToken cancellationToken)
    {
        List<string> applicationNumbers = await context.MerchantApplications
            .AsNoTracking()
            .Select(x => x.ApplicationNumber)
            .ToListAsync(cancellationToken);

        long maxNumber = 0;
        foreach (string applicationNumber in applicationNumbers)
        {
            if (long.TryParse(applicationNumber, NumberStyles.None, CultureInfo.InvariantCulture, out long parsed) && parsed > maxNumber)
            {
                maxNumber = parsed;
            }
        }

        return (maxNumber + 1).ToString(CultureInfo.InvariantCulture);
    }

    private static IQueryable<Merchantdetails> ApplyFilters(IQueryable<Merchantdetails> query, MerchantApplicationFilterRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.TaxNumber))
        {
            query = query.Where(x => x.TaxNumber.Contains(request.TaxNumber));
        }

        if (!string.IsNullOrWhiteSpace(request.WorkplaceSignboardName))
        {
            string workplaceSignboardName = request.WorkplaceSignboardName.Trim();
            query = query.Where(x => x.WorkplaceSignboardName.Contains(workplaceSignboardName));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(x => x.ApplicationStatus == request.Status);
        }

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            query = query.Where(x => x.City == request.City);
        }

        if (request.StartDate.HasValue)
        {
            DateTime start = request.StartDate.Value.Date;
            query = query.Where(x => x.CreatedAt >= start);
        }

        if (request.EndDate.HasValue)
        {
            DateTime end = request.EndDate.Value.Date.AddDays(1);
            query = query.Where(x => x.CreatedAt < end);
        }

        return query;
    }

    private static MerchantApplicationDetailResponse MapDetail(Merchantdetails entity, ExchangeRateSnapshotResponse exchangeRateSnapshot)
    {
        return new MerchantApplicationDetailResponse
        {
            Id = entity.Id,
            ApplicationNumber = entity.ApplicationNumber,
            WorkflowReferenceNumber = entity.WorkflowReferenceNumber,
            BranchCode = entity.BranchCode,
            BranchName = entity.BranchName,
            CustomerNumber = entity.CustomerNumber,
            IdentityNumber = entity.IdentityNumber,
            CompanyType = entity.CompanyType,
            TaxNumber = entity.TaxNumber,
            IsSpecialRulingRequired = entity.IsSpecialRulingRequired,
            DemandDepositAccountNumber = entity.DemandDepositAccountNumber,
            WorkplaceSignboardName = entity.WorkplaceSignboardName,
            ContractedUserCode = entity.ContractedUserCode,
            TradeRegistryNumber = entity.TradeRegistryNumber,
            TradeRegistryRegistrationDate = entity.TradeRegistryRegistrationDate,
            ManagerFullName = entity.ManagerFullName,
            TradeRegistryRegistrationName = entity.TradeRegistryRegistrationName,
            TaxOffice = entity.TaxOffice,
            CustomerAddress = entity.CustomerAddress,
            PosInstallationAddress = entity.PosInstallationAddress,
            Email = entity.Email,
            City = entity.City,
            District = entity.District,
            PostalCode = entity.PostalCode,
            GsmNumber = entity.GsmNumber,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            WebAddress = entity.WebAddress,
            OwnerIdentityNumber = entity.OwnerIdentityNumber,
            OwnerFullName = entity.OwnerFullName,
            OwnerMobilePhone = entity.OwnerMobilePhone,
            Status = entity.ApplicationStatus,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            ExchangeRates = exchangeRateSnapshot,
            Documents = entity.Documents
                .OrderByDescending(document => document.UploadedAt)
                .Select(document => new MerchantDocumentResponse
                {
                    Id = document.Id,
                    DocumentType = document.DocumentType,
                    OriginalFileName = document.OriginalFileName,
                    ContentType = document.ContentType,
                    FileSize = document.FileSize,
                    UploadedAt = document.UploadedAt
                })
                .ToList(),
            Histories = entity.Histories
                .OrderByDescending(history => history.ProcessDate)
                .Select(history => new MerchantApplicationHistoryResponse
            {
                Id = history.Id,
                ProcessDate = history.ProcessDate,
                Description = history.Description,
                Status = history.Status,
                UserCode = history.UserCode,
                HistoryDescription = history.HistoryDescription
            })
                .ToList()
        };
    }
}

using MerchantApp.WebAPI.Contracts;
using MerchantApp.WebAPI.Services;
using merchant.domain.Constants;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.AspNetCore.Authorization;


namespace MerchantApp.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/merchant-applications")]
public sealed class MerchantApplicationsController(IMerchantApplicationService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(MerchantApplicationDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMerchantApplicationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            MerchantApplicationDetailResponse response = await service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (MerchantApplicationValidationException exception)
        {
            return ValidationProblem(new ValidationProblemDetails(exception.Errors));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<MerchantApplicationListItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] MerchantApplicationFilterRequest request, CancellationToken cancellationToken)
    {
        return Ok(await service.GetPagedAsync(request, cancellationToken));
    }

    [HttpGet("export/excel")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportExcel([FromQuery] MerchantApplicationFilterRequest request, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<MerchantApplicationDetailResponse> records = await service.GetExportItemsAsync(request, cancellationToken);
        byte[] csvBytes = BuildCsv(records);
        string fileName = $"basvuru_raporu_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        return File(csvBytes, "text/csv; charset=utf-8", fileName);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MerchantApplicationDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        MerchantApplicationDetailResponse? response = await service.GetByIdAsync(id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpGet("exists/trade-registry/{tradeRegistryNumber}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckTradeRegistryNumberExists(string tradeRegistryNumber, CancellationToken cancellationToken)
    {
        bool exists = await service.TradeRegistryNumberExistsAsync(tradeRegistryNumber, cancellationToken);
        return Ok(new { exists });
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        bool deleted = await service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpDelete("by-application-number/{applicationNumber}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteByApplicationNumber(string applicationNumber, CancellationToken cancellationToken)
    {
        bool deleted = await service.DeleteByApplicationNumberAsync(applicationNumber, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(MerchantApplicationDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateMerchantApplicationStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            MerchantApplicationDetailResponse? response = await service.UpdateStatusAsync(id, request, cancellationToken);
            return response is null ? NotFound() : Ok(response);
        }
        catch (MerchantApplicationValidationException exception)
        {
            return ValidationProblem(new ValidationProblemDetails(exception.Errors));
        }
    }

    [HttpGet("report/summary")]
    [ProducesResponseType(typeof(MerchantApplicationReportResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReport(CancellationToken cancellationToken)
    {
        return Ok(await service.GetReportAsync(cancellationToken));
    }

    [HttpGet("reference/statuses")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    public IActionResult GetStatuses()
    {
        return Ok(MerchantApplicationStatuses.All);
    }

    [HttpGet("reference/document-types")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDocumentTypes(CancellationToken cancellationToken)
    {
        return Ok(await service.GetDocumentTypesAsync(cancellationToken));
    }

    [HttpGet("reference/exchange-rates")]
    [ProducesResponseType(typeof(ExchangeRateSnapshotResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExchangeRates(CancellationToken cancellationToken)
    {
        return Ok(await service.GetExchangeRatesAsync(cancellationToken));
    }

    private static byte[] BuildCsv(IReadOnlyCollection<MerchantApplicationDetailResponse> records)
    {
        StringBuilder builder = new();
        builder.AppendLine(string.Join(",",
            "BasvuruId",
            "BasvuruNo",
            "IsAkisReferansNo",
            "SubeKodu",
            "SubeAdi",
            "MusteriNo",
            "KimlikNo",
            "SirketTuru",
            "VergiNo",
            "OzelHukumGerekiyorMu",
            "VadesizHesapNo",
            "IsyeriTabelaAdi",
            "SozlesmeKullaniciKodu",
            "TicaretSicilNo",
            "TicaretSicilTarihi",
            "YoneticiAdSoyad",
            "TicaretSicilKayitAdi",
            "VergiDairesi",
            "MusteriAdresi",
            "PosKurulumAdresi",
            "Eposta",
            "Il",
            "Ilce",
            "PostaKodu",
            "GsmNo",
            "Enlem",
            "Boylam",
            "WebAdresi",
            "SahipKimlikNo",
            "SahipAdSoyad",
            "SahipCepNo",
            "Durum",
            "OlusturmaTarihi",
            "GuncellemeTarihi",
            "DokumanSayisi",
            "GecmisSayisi",
            "DokumanDetayi",
            "DurumGecmisi"
        ));

        foreach (MerchantApplicationDetailResponse item in records)
        {
            string documentDetail = string.Join(" | ",
                item.Documents.Select(document => $"{document.DocumentType}:{document.OriginalFileName}({document.FileSize})"));

            string historyDetail = string.Join(" | ",
                item.Histories.Select(history => $"{history.ProcessDate:yyyy-MM-dd HH:mm}:{history.Status}:{history.Description}"));

            builder.AppendLine(string.Join(",",
                Escape(item.Id.ToString()),
                Escape(item.ApplicationNumber),
                Escape(item.WorkflowReferenceNumber),
                Escape(item.BranchCode),
                Escape(item.BranchName),
                Escape(item.CustomerNumber),
                Escape(item.IdentityNumber),
                Escape(item.CompanyType),
                Escape(item.TaxNumber),
                Escape(item.IsSpecialRulingRequired ? "Evet" : "Hayir"),
                Escape(item.DemandDepositAccountNumber),
                Escape(item.WorkplaceSignboardName),
                Escape(item.ContractedUserCode),
                Escape(item.TradeRegistryNumber),
                Escape(item.TradeRegistryRegistrationDate?.ToString("yyyy-MM-dd") ?? string.Empty),
                Escape(item.ManagerFullName),
                Escape(item.TradeRegistryRegistrationName),
                Escape(item.TaxOffice),
                Escape(item.CustomerAddress),
                Escape(item.PosInstallationAddress),
                Escape(item.Email),
                Escape(item.City),
                Escape(item.District),
                Escape(item.PostalCode),
                Escape(item.GsmNumber),
                Escape(item.Latitude.ToString("0.######")),
                Escape(item.Longitude.ToString("0.######")),
                Escape(item.WebAddress ?? string.Empty),
                Escape(item.OwnerIdentityNumber),
                Escape(item.OwnerFullName),
                Escape(item.OwnerMobilePhone),
                Escape(item.Status),
                Escape(item.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")),
                Escape(item.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty),
                Escape(item.Documents.Count.ToString()),
                Escape(item.Histories.Count.ToString()),
                Escape(documentDetail),
                Escape(historyDetail)
            ));
        }

        byte[] utf8WithoutBom = Encoding.UTF8.GetBytes(builder.ToString());
        byte[] bom = [0xEF, 0xBB, 0xBF];
        byte[] result = new byte[bom.Length + utf8WithoutBom.Length];
        Buffer.BlockCopy(bom, 0, result, 0, bom.Length);
        Buffer.BlockCopy(utf8WithoutBom, 0, result, bom.Length, utf8WithoutBom.Length);
        return result;
    }

    private static string Escape(string value)
    {
        string sanitized = value.Replace("\"", "\"\"");
        return $"\"{sanitized}\"";
    }
}

namespace merchant.domain.Entities;

public sealed class ExchangeRateSnapshot
{
    public Guid Id { get; set; }
    public decimal? UsdToTryRate { get; set; }
    public decimal? EurToTryRate { get; set; }
    public DateTime? FetchedAtUtc { get; set; }
    public string Source { get; set; } = string.Empty;
}

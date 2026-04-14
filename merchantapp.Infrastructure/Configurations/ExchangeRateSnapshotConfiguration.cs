using merchant.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace merchantapp.Infrastructure.Configurations;

internal sealed class ExchangeRateSnapshotConfiguration : IEntityTypeConfiguration<ExchangeRateSnapshot>
{
    public void Configure(EntityTypeBuilder<ExchangeRateSnapshot> builder)
    {
        builder.ToTable("ExchangeRateSnapshots");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UsdToTryRate).HasColumnType("decimal(18,4)");
        builder.Property(x => x.EurToTryRate).HasColumnType("decimal(18,4)");
        builder.Property(x => x.FetchedAtUtc).HasColumnType("datetime2");
        builder.Property(x => x.Source).HasColumnType("varchar(100)").IsRequired();
        builder.HasData(new ExchangeRateSnapshot
        {
            Id = Guid.Parse("b3e8a998-1cc0-448f-a0d1-61fbfcf88d57"),
            UsdToTryRate = 38.6500m,
            EurToTryRate = 41.9500m,
            FetchedAtUtc = new DateTime(2026, 3, 31, 9, 0, 0, DateTimeKind.Utc),
            Source = "Static seed"
        });
    }
}

using merchant.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace merchantapp.Infrastructure.Configurations;

internal sealed class MerchantApplicationHistoryConfiguration : IEntityTypeConfiguration<MerchantApplicationHistory>
{
    public void Configure(EntityTypeBuilder<MerchantApplicationHistory> builder)
    {
        builder.ToTable("MerchantApplicationHistories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Description).HasColumnType("varchar(200)").IsRequired();
        builder.Property(x => x.Status).HasColumnType("varchar(30)").IsRequired();
        builder.Property(x => x.UserCode).HasColumnType("varchar(30)").IsRequired();
        builder.Property(x => x.HistoryDescription).HasColumnType("varchar(500)").IsRequired();
        builder.Property(x => x.ProcessDate).IsRequired();

        builder.HasOne(x => x.MerchantApplication)
            .WithMany(x => x.Histories)
            .HasForeignKey(x => x.MerchantApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

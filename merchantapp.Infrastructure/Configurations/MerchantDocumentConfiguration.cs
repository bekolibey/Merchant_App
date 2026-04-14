using merchant.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace merchantapp.Infrastructure.Configurations;

internal sealed class MerchantDocumentConfiguration : IEntityTypeConfiguration<MerchantDocument>
{
    public void Configure(EntityTypeBuilder<MerchantDocument> builder)
    {
        builder.ToTable("MerchantDocuments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DocumentType).HasColumnType("varchar(100)").IsRequired();
        builder.Property(x => x.OriginalFileName).HasColumnType("varchar(260)").IsRequired();
        builder.Property(x => x.ContentType).HasColumnType("varchar(100)").IsRequired();
        builder.Property(x => x.FileSize).IsRequired();
        builder.Property(x => x.UploadedAt).IsRequired();

        builder.HasOne(x => x.MerchantApplication)
            .WithMany(x => x.Documents)
            .HasForeignKey(x => x.MerchantApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

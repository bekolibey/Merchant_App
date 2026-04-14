using merchant.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace merchantapp.Infrastructure.Configurations;

internal sealed class MerchantDocumentTypeConfiguration : IEntityTypeConfiguration<MerchantDocumentType>
{
    public void Configure(EntityTypeBuilder<MerchantDocumentType> builder)
    {
        builder.ToTable("MerchantDocumentTypes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Name).HasColumnType("varchar(100)").IsRequired();
        builder.Property(x => x.DisplayOrder).IsRequired();
        builder.Property(x => x.IsRequired).IsRequired();

        builder.HasData(
            new MerchantDocumentType { Id = 1, Name = "Vergi Levhasi", DisplayOrder = 1, IsRequired = true },
            new MerchantDocumentType { Id = 2, Name = "Imza Sirkuleri", DisplayOrder = 2, IsRequired = true },
            new MerchantDocumentType { Id = 3, Name = "Ticaret Sicil Gazetesi", DisplayOrder = 3, IsRequired = true },
            new MerchantDocumentType { Id = 4, Name = "Kimlik", DisplayOrder = 4, IsRequired = true },
            new MerchantDocumentType { Id = 5, Name = "Uye Isyeri Sozlesmesi", DisplayOrder = 5, IsRequired = true }
        );
    }
}

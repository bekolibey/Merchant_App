using merchant.domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace merchantapp.Infrastructure.Configurations;

internal sealed class PortalUserConfiguration : IEntityTypeConfiguration<PortalUser>
{
    public void Configure(EntityTypeBuilder<PortalUser> builder)
    {
        builder.ToTable("PortalUsers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .HasColumnType("varchar(150)")
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.PasswordHash)
            .HasColumnType("varchar(128)")
            .IsRequired();

        builder.Property(x => x.AccessToken)
            .HasColumnType("nvarchar(4000)");

        builder.Property(x => x.AccessTokenExpiresAtUtc);

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}

using merchant.domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using son_calisma_merchantapp.Entities;

namespace merchantapp.Infrastructure.Configurations;

internal sealed class MerchantdetailsConfigurations : IEntityTypeConfiguration<Merchantdetails>
{
    public void Configure(EntityTypeBuilder<Merchantdetails> builder)
    {
        builder.ToTable("MerchantApplications");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.ApplicationNumber).HasColumnType("varchar(30)").IsRequired();
        builder.HasIndex(p => p.ApplicationNumber).IsUnique();
        builder.Property(p => p.WorkflowReferenceNumber).HasColumnType("varchar(50)").IsRequired();
        builder.Property(p => p.BranchCode).HasColumnType("varchar(20)").IsRequired();
        builder.Property(p => p.BranchName).HasColumnType("varchar(150)").IsRequired();
        builder.Property(p => p.CustomerNumber).HasColumnType("varchar(30)").IsRequired();
        builder.Property(p => p.IdentityNumber).HasColumnType("varchar(11)").IsRequired();
        builder.Property(p => p.CompanyType).HasColumnType("varchar(50)").IsRequired();
        builder.Property(p => p.TaxNumber).HasColumnType("varchar(10)").IsRequired();
        builder.HasIndex(p => p.TaxNumber).IsUnique();
        builder.Property(p => p.IsSpecialRulingRequired).IsRequired();
        builder.Property(p => p.DemandDepositAccountNumber).HasColumnType("varchar(34)").IsRequired();
        builder.Property(p => p.WorkplaceSignboardName).HasColumnType("varchar(150)").IsRequired();
        builder.Property(p => p.ContractedUserCode).HasColumnType("varchar(30)").IsRequired();
        builder.Property(p => p.TradeRegistryNumber).HasColumnType("varchar(30)").IsRequired();
        builder.Property(p => p.ManagerFullName).HasColumnType("varchar(150)").IsRequired();
        builder.Property(p => p.TradeRegistryRegistrationName).HasColumnType("varchar(150)").IsRequired();
        builder.Property(p => p.TaxOffice).HasColumnType("varchar(50)").IsRequired();
        builder.Property(p => p.CustomerAddress).HasColumnType("varchar(500)").IsRequired();
        builder.Property(p => p.PosInstallationAddress).HasColumnType("varchar(500)").IsRequired();
        builder.Property(p => p.Email).HasColumnType("varchar(150)").IsRequired();
        builder.Property(p => p.City).HasColumnType("varchar(50)").IsRequired();
        builder.Property(p => p.District).HasColumnType("varchar(50)").IsRequired();
        builder.Property(p => p.PostalCode).HasColumnType("varchar(10)").IsRequired();
        builder.Property(p => p.GsmNumber).HasColumnType("varchar(15)").IsRequired();
        builder.Property(p => p.Latitude).HasColumnType("decimal(9,6)").IsRequired();
        builder.Property(p => p.Longitude).HasColumnType("decimal(9,6)").IsRequired();
        builder.Property(p => p.WebAddress).HasColumnType("varchar(100)");
        builder.Property(p => p.ApplicationStatus).HasColumnType("varchar(20)").HasDefaultValue(MerchantApplicationStatuses.Pending);
        builder.Property(p => p.OwnerIdentityNumber).HasColumnType("varchar(11)").IsRequired();
        builder.Property(p => p.OwnerFullName).HasColumnType("varchar(150)").IsRequired();
        builder.Property(p => p.OwnerMobilePhone).HasColumnType("varchar(15)").IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt);
        builder.HasMany(p => p.Histories)
            .WithOne(p => p.MerchantApplication)
            .HasForeignKey(p => p.MerchantApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Documents)
            .WithOne(p => p.MerchantApplication)
            .HasForeignKey(p => p.MerchantApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

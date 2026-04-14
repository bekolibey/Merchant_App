using Microsoft.EntityFrameworkCore;
using merchant.domain.Entities;
using son_calisma_merchantapp.Domain.Repositories;
using son_calisma_merchantapp.Entities;

namespace merchantapp.Infrastructure.Context;

public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Merchantdetails> MerchantApplications { get; set; }
    public DbSet<MerchantApplicationHistory> MerchantApplicationHistories { get; set; }
    public DbSet<MerchantDocument> MerchantDocuments { get; set; }
    public DbSet<ExchangeRateSnapshot> ExchangeRateSnapshots { get; set; }
    public DbSet<MerchantDocumentType> MerchantDocumentTypes { get; set; }
    public DbSet<PortalUser> PortalUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}

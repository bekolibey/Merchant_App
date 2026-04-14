using merchantapp.Infrastructure.Context;
using son_calisma_merchantapp.Domain.Repositories; 
using son_calisma_merchantapp.Entities;

namespace son_calisma_merchantapp.Infrastructure.Repositories;

internal sealed class MerchantRepository : Repository<Merchantdetails>,IMerchantRepository
{
    public MerchantRepository(ApplicationDbContext context) : base(context)
    {
    }
}

internal interface IMerchantRepository
{
}
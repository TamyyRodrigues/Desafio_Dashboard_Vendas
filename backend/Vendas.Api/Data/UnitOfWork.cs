using Vendas.Api.Repositories;

namespace Vendas.Api.Data;

public class UnitOfWork(VendasDbContext db, IVendaRepository vendas) : IUnitOfWork
{
    public IVendaRepository Vendas { get; } = vendas;

    public Task<int> CommitAsync() => db.SaveChangesAsync();
}

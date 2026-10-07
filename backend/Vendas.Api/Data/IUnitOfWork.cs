using Vendas.Api.Repositories;

namespace Vendas.Api.Data;

public interface IUnitOfWork
{
    IVendaRepository Vendas { get; }

    /// <summary>Confirma todas as alterações pendentes em uma única transação.</summary>
    Task<int> CommitAsync();
}

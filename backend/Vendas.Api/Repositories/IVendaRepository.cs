using Vendas.Api.Models;

namespace Vendas.Api.Repositories;

public interface IVendaRepository
{
    Task<IReadOnlyList<Venda>> ListarAsync(VendaFiltro filtro);
    Task<Venda?> ObterAsync(int idVenda);
    Task<bool> ExisteAsync(int idVenda);
    Task<IReadOnlyList<int>> IdsExistentesAsync(IEnumerable<int> ids);
    Task AdicionarAsync(Venda venda);
    Task AdicionarVariosAsync(IEnumerable<Venda> vendas);
    void Atualizar(Venda venda);
    void Remover(Venda venda);
}

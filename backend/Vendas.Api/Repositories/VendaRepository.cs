using Microsoft.EntityFrameworkCore;
using Vendas.Api.Data;
using Vendas.Api.Models;

namespace Vendas.Api.Repositories;

public class VendaRepository(VendasDbContext db) : IVendaRepository
{
    public async Task<IReadOnlyList<Venda>> ListarAsync(VendaFiltro filtro)
    {
        var query = db.Vendas.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro.Produto))
        {
            var termo = $"%{filtro.Produto.Trim()}%";
            query = query.Where(v => EF.Functions.Like(v.Produto, termo));
        }
        if (filtro.QuantidadeMinima is int min)
            query = query.Where(v => v.Quantidade >= min);
        if (filtro.QuantidadeMaxima is int max)
            query = query.Where(v => v.Quantidade <= max);
        if (filtro.DataInicio is DateOnly inicio)
            query = query.Where(v => v.DataVenda >= inicio);
        if (filtro.DataFim is DateOnly fim)
            query = query.Where(v => v.DataVenda <= fim);

        return await query.OrderBy(v => v.IdVenda).ToListAsync();
    }

    public async Task<Venda?> ObterAsync(int idVenda) => await db.Vendas.FindAsync(idVenda);

    public Task<bool> ExisteAsync(int idVenda) => db.Vendas.AnyAsync(v => v.IdVenda == idVenda);

    public async Task<IReadOnlyList<int>> IdsExistentesAsync(IEnumerable<int> ids)
    {
        var lista = ids.ToList();
        return await db.Vendas.Where(v => lista.Contains(v.IdVenda)).Select(v => v.IdVenda).ToListAsync();
    }

    public async Task AdicionarAsync(Venda venda) => await db.Vendas.AddAsync(venda);

    public Task AdicionarVariosAsync(IEnumerable<Venda> vendas) => db.Vendas.AddRangeAsync(vendas);

    public void Atualizar(Venda venda) => db.Vendas.Update(venda);

    public void Remover(Venda venda) => db.Vendas.Remove(venda);
}

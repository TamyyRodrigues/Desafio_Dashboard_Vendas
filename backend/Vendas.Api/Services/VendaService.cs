using Vendas.Api.Data;
using Vendas.Api.Dtos;
using Vendas.Api.Exceptions;
using Vendas.Api.Models;

namespace Vendas.Api.Services;

public class VendaService(IUnitOfWork uow) : IVendaService
{
    public async Task<IReadOnlyList<VendaDto>> ListarAsync(VendaFiltro filtro)
    {
        if (filtro.DataInicio > filtro.DataFim)
            throw new BadRequestException("dataInicio não pode ser maior que dataFim.");
        if (filtro.QuantidadeMinima > filtro.QuantidadeMaxima)
            throw new BadRequestException("quantidadeMinima não pode ser maior que quantidadeMaxima.");

        var vendas = await uow.Vendas.ListarAsync(filtro);
        return vendas.Select(VendaDto.From).ToList();
    }

    public async Task<VendaDto> ObterAsync(int idVenda) =>
        VendaDto.From(await ObterEntidadeAsync(idVenda));

    public async Task<VendaDto> CriarAsync(VendaDto dto)
    {
        if (await uow.Vendas.ExisteAsync(dto.IdVenda))
            throw new ConflictException($"Já existe uma venda com id_venda {dto.IdVenda}.");

        await uow.Vendas.AdicionarAsync(dto.ToEntity());
        await uow.CommitAsync();
        return dto;
    }

    public async Task<int> ImportarAsync(IEnumerable<VendaDto> vendas)
    {
        var lista = vendas.ToList();
        if (lista.Count == 0)
            throw new BadRequestException("Nenhuma venda informada para importação.");

        var repetidos = lista.GroupBy(v => v.IdVenda).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (repetidos.Count > 0)
            throw new ConflictException($"IDs repetidos no arquivo: {string.Join(", ", repetidos)}.");

        var existentes = await uow.Vendas.IdsExistentesAsync(lista.Select(v => v.IdVenda));
        if (existentes.Count > 0)
            throw new ConflictException($"IDs já cadastrados: {string.Join(", ", existentes)}. Nada foi importado.");

        await uow.Vendas.AdicionarVariosAsync(lista.Select(v => v.ToEntity()));
        await uow.CommitAsync(); // uma única transação: tudo ou nada
        return lista.Count;
    }

    public async Task<VendaDto> AtualizarAsync(int idVenda, VendaDto dto)
    {
        if (dto.IdVenda != idVenda)
            throw new BadRequestException("O id_venda do corpo difere do id da rota.");

        var venda = await ObterEntidadeAsync(idVenda);
        venda.Produto = dto.Produto.Trim();
        venda.Quantidade = dto.Quantidade;
        venda.PrecoUnitario = dto.PrecoUnitario;
        venda.DataVenda = dto.DataVenda;

        uow.Vendas.Atualizar(venda);
        await uow.CommitAsync();
        return VendaDto.From(venda);
    }

    public async Task RemoverAsync(int idVenda)
    {
        var venda = await ObterEntidadeAsync(idVenda);
        uow.Vendas.Remover(venda);
        await uow.CommitAsync();
    }

    private async Task<Venda> ObterEntidadeAsync(int idVenda) =>
        await uow.Vendas.ObterAsync(idVenda)
        ?? throw new NotFoundException($"Venda {idVenda} não encontrada.");
}

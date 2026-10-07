using Vendas.Api.Dtos;
using Vendas.Api.Models;

namespace Vendas.Api.Services;

public interface IVendaService
{
    Task<IReadOnlyList<VendaDto>> ListarAsync(VendaFiltro filtro);
    Task<VendaDto> ObterAsync(int idVenda);
    Task<VendaDto> CriarAsync(VendaDto dto);
    Task<int> ImportarAsync(IEnumerable<VendaDto> vendas);
    Task<VendaDto> AtualizarAsync(int idVenda, VendaDto dto);
    Task RemoverAsync(int idVenda);
}

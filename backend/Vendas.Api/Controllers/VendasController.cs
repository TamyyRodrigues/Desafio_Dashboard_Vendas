using Microsoft.AspNetCore.Mvc;
using Vendas.Api.Dtos;
using Vendas.Api.Models;
using Vendas.Api.Services;

namespace Vendas.Api.Controllers;

[ApiController]
[Route("api/vendas")]
[Produces("application/json")]
public class VendasController(IVendaService service) : ControllerBase
{
    /// <summary>Lista vendas. Filtros: produto, quantidadeMinima/Maxima, dataInicio/dataFim (yyyy-MM-dd).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VendaDto>>> Listar([FromQuery] VendaFiltro filtro) =>
        Ok(await service.ListarAsync(filtro));

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VendaDto>> Obter(int id) => Ok(await service.ObterAsync(id));

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VendaDto>> Criar([FromBody] VendaDto dto)
    {
        var criada = await service.CriarAsync(dto);
        return CreatedAtAction(nameof(Obter), new { id = criada.IdVenda }, criada);
    }

    /// <summary>Importa em lote as vendas lidas do CSV (atômico: tudo ou nada).</summary>
    [HttpPost("importacao")]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ImportacaoResultado>> Importar([FromBody] List<VendaDto> vendas) =>
        Ok(new ImportacaoResultado(await service.ImportarAsync(vendas)));

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VendaDto>> Atualizar(int id, [FromBody] VendaDto dto) =>
        Ok(await service.AtualizarAsync(id, dto));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id)
    {
        await service.RemoverAsync(id);
        return NoContent();
    }
}

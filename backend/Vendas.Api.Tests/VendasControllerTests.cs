using Microsoft.AspNetCore.Mvc;
using Moq;
using Vendas.Api.Controllers;
using Vendas.Api.Dtos;
using Vendas.Api.Models;
using Vendas.Api.Services;

namespace Vendas.Api.Tests;

public class VendasControllerTests
{
    private readonly Mock<IVendaService> _service = new();
    private readonly VendasController _controller;

    public VendasControllerTests() => _controller = new VendasController(_service.Object);

    private static VendaDto Dto(int id) => new()
    {
        IdVenda = id, Produto = "Camiseta", Quantidade = 1, PrecoUnitario = 49.90m, DataVenda = new DateOnly(2026, 9, 6)
    };

    [Fact]
    public async Task Listar_RetornaOkComAsVendas()
    {
        _service.Setup(s => s.ListarAsync(It.IsAny<VendaFiltro>()))
                .ReturnsAsync((IReadOnlyList<VendaDto>)new List<VendaDto> { Dto(1) });

        var resultado = await _controller.Listar(new VendaFiltro());

        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<VendaDto>>(ok.Value));
    }

    [Fact]
    public async Task Criar_RetornaCreatedComLocalizacao()
    {
        _service.Setup(s => s.CriarAsync(It.IsAny<VendaDto>())).ReturnsAsync(Dto(7));

        var resultado = await _controller.Criar(Dto(7));

        var created = Assert.IsType<CreatedAtActionResult>(resultado.Result);
        Assert.Equal(nameof(VendasController.Obter), created.ActionName);
        Assert.Equal(7, created.RouteValues!["id"]);
    }

    [Fact]
    public async Task Importar_RetornaQuantidadeImportada()
    {
        _service.Setup(s => s.ImportarAsync(It.IsAny<IEnumerable<VendaDto>>())).ReturnsAsync(2);

        var resultado = await _controller.Importar(new List<VendaDto> { Dto(1), Dto(2) });

        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        Assert.Equal(2, Assert.IsType<ImportacaoResultado>(ok.Value).Importadas);
    }

    [Fact]
    public async Task Remover_RetornaNoContent()
    {
        var resultado = await _controller.Remover(1);

        Assert.IsType<NoContentResult>(resultado);
        _service.Verify(s => s.RemoverAsync(1), Times.Once);
    }
}

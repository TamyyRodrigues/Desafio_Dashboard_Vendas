using Moq;
using Vendas.Api.Data;
using Vendas.Api.Dtos;
using Vendas.Api.Exceptions;
using Vendas.Api.Models;
using Vendas.Api.Repositories;
using Vendas.Api.Services;

namespace Vendas.Api.Tests;

public class VendaServiceTests
{
    private readonly Mock<IVendaRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly VendaService _service;

    public VendaServiceTests()
    {
        _uow.Setup(u => u.Vendas).Returns(_repo.Object);
        _uow.Setup(u => u.CommitAsync()).ReturnsAsync(1);
        _service = new VendaService(_uow.Object);
    }

    private static VendaDto Dto(int id, string produto = "Camiseta") => new()
    {
        IdVenda = id, Produto = produto, Quantidade = 1, PrecoUnitario = 49.90m, DataVenda = new DateOnly(2026, 9, 6)
    };

    [Fact]
    public async Task CriarAsync_IdNovo_AdicionaEFazCommit()
    {
        _repo.Setup(r => r.ExisteAsync(10)).ReturnsAsync(false);

        await _service.CriarAsync(Dto(10));

        _repo.Verify(r => r.AdicionarAsync(It.Is<Venda>(v => v.IdVenda == 10)), Times.Once);
        _uow.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_IdExistente_LancaConflictSemCommit()
    {
        _repo.Setup(r => r.ExisteAsync(10)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.CriarAsync(Dto(10)));

        _uow.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task ObterAsync_Inexistente_LancaNotFound()
    {
        _repo.Setup(r => r.ObterAsync(99)).ReturnsAsync((Venda?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.ObterAsync(99));
    }

    [Fact]
    public async Task ImportarAsync_IdsRepetidosNoLote_LancaConflictSemCommit()
    {
        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.ImportarAsync(new[] { Dto(1), Dto(1) }));

        _uow.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task ImportarAsync_IdsJaCadastrados_LancaConflictSemCommit()
    {
        _repo.Setup(r => r.IdsExistentesAsync(It.IsAny<IEnumerable<int>>()))
             .ReturnsAsync((IReadOnlyList<int>)new List<int> { 2 });

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.ImportarAsync(new[] { Dto(1), Dto(2) }));

        _repo.Verify(r => r.AdicionarVariosAsync(It.IsAny<IEnumerable<Venda>>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task ImportarAsync_LoteValido_RetornaQuantidadeEFazUmCommit()
    {
        _repo.Setup(r => r.IdsExistentesAsync(It.IsAny<IEnumerable<int>>()))
             .ReturnsAsync((IReadOnlyList<int>)new List<int>());

        var total = await _service.ImportarAsync(new[] { Dto(1), Dto(2), Dto(3) });

        Assert.Equal(3, total);
        _uow.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task ImportarAsync_LoteVazio_LancaBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _service.ImportarAsync(Array.Empty<VendaDto>()));
    }

    [Fact]
    public async Task AtualizarAsync_IdDoCorpoDiferenteDaRota_LancaBadRequest()
    {
        await Assert.ThrowsAsync<BadRequestException>(() => _service.AtualizarAsync(1, Dto(2)));
    }

    [Fact]
    public async Task AtualizarAsync_Existente_AtualizaCamposECommita()
    {
        var entidade = new Venda { IdVenda = 1, Produto = "Calça", Quantidade = 2, PrecoUnitario = 99.90m, DataVenda = new DateOnly(2026, 9, 7) };
        _repo.Setup(r => r.ObterAsync(1)).ReturnsAsync(entidade);

        var resultado = await _service.AtualizarAsync(1, Dto(1, "Tênis"));

        Assert.Equal("Tênis", resultado.Produto);
        Assert.Equal("Tênis", entidade.Produto);
        _repo.Verify(r => r.Atualizar(entidade), Times.Once);
        _uow.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoverAsync_Existente_RemoveECommita()
    {
        var entidade = new Venda { IdVenda = 1, Produto = "Calça" };
        _repo.Setup(r => r.ObterAsync(1)).ReturnsAsync(entidade);

        await _service.RemoverAsync(1);

        _repo.Verify(r => r.Remover(entidade), Times.Once);
        _uow.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoverAsync_Inexistente_LancaNotFound()
    {
        _repo.Setup(r => r.ObterAsync(5)).ReturnsAsync((Venda?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.RemoverAsync(5));
    }

    [Fact]
    public async Task ListarAsync_IntervaloDeDatasInvertido_LancaBadRequest()
    {
        var filtro = new VendaFiltro { DataInicio = new DateOnly(2026, 9, 10), DataFim = new DateOnly(2026, 9, 1) };

        await Assert.ThrowsAsync<BadRequestException>(() => _service.ListarAsync(filtro));
    }

    [Fact]
    public async Task ListarAsync_IntervaloDeQuantidadeInvertido_LancaBadRequest()
    {
        var filtro = new VendaFiltro { QuantidadeMinima = 5, QuantidadeMaxima = 1 };

        await Assert.ThrowsAsync<BadRequestException>(() => _service.ListarAsync(filtro));
    }

    [Fact]
    public async Task ListarAsync_FiltroValido_MapeiaParaDto()
    {
        _repo.Setup(r => r.ListarAsync(It.IsAny<VendaFiltro>()))
             .ReturnsAsync((IReadOnlyList<Venda>)new List<Venda>
             {
                 new() { IdVenda = 1, Produto = "Camiseta", Quantidade = 3, PrecoUnitario = 49.90m, DataVenda = new DateOnly(2026, 9, 6) }
             });

        var lista = await _service.ListarAsync(new VendaFiltro());

        var item = Assert.Single(lista);
        Assert.Equal("Camiseta", item.Produto);
        Assert.Equal(3, item.Quantidade);
    }
}

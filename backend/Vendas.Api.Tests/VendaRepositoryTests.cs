using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Vendas.Api.Data;
using Vendas.Api.Models;
using Vendas.Api.Repositories;

namespace Vendas.Api.Tests;

/// <summary>Testa o repositório e o Unit of Work contra um SQLite em memória (mesmo provider da API).</summary>
public sealed class VendaRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly VendasDbContext _db;
    private readonly VendaRepository _repo;

    public VendaRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<VendasDbContext>().UseSqlite(_connection).Options;
        _db = new VendasDbContext(options);
        _db.Database.EnsureCreated();

        _db.Vendas.AddRange(
            Nova(1, "Camiseta", 3, 49.90m, new DateOnly(2026, 9, 6)),
            Nova(2, "Calça", 2, 99.90m, new DateOnly(2026, 9, 7)),
            Nova(3, "Camiseta", 1, 49.90m, new DateOnly(2026, 9, 6)),
            Nova(4, "Tênis", 1, 699.90m, new DateOnly(2026, 9, 10)));
        _db.SaveChanges();

        _repo = new VendaRepository(_db);
    }

    private static Venda Nova(int id, string produto, int qtd, decimal preco, DateOnly data) => new()
    {
        IdVenda = id, Produto = produto, Quantidade = qtd, PrecoUnitario = preco, DataVenda = data
    };

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task ListarAsync_SemFiltro_RetornaTodasOrdenadasPorId()
    {
        var lista = await _repo.ListarAsync(new VendaFiltro());

        Assert.Equal(new[] { 1, 2, 3, 4 }, lista.Select(v => v.IdVenda));
    }

    [Fact]
    public async Task ListarAsync_FiltraPorProdutoParcial()
    {
        var lista = await _repo.ListarAsync(new VendaFiltro { Produto = "camis" });

        Assert.Equal(new[] { 1, 3 }, lista.Select(v => v.IdVenda));
    }

    [Fact]
    public async Task ListarAsync_FiltraPorQuantidadeMinima()
    {
        var lista = await _repo.ListarAsync(new VendaFiltro { QuantidadeMinima = 2 });

        Assert.Equal(new[] { 1, 2 }, lista.Select(v => v.IdVenda));
    }

    [Fact]
    public async Task ListarAsync_FiltraPorQuantidadeMaxima()
    {
        var lista = await _repo.ListarAsync(new VendaFiltro { QuantidadeMaxima = 1 });

        Assert.Equal(new[] { 3, 4 }, lista.Select(v => v.IdVenda));
    }

    [Fact]
    public async Task ListarAsync_FiltraPorIntervaloDeDatas()
    {
        var filtro = new VendaFiltro { DataInicio = new DateOnly(2026, 9, 7), DataFim = new DateOnly(2026, 9, 10) };

        var lista = await _repo.ListarAsync(filtro);

        Assert.Equal(new[] { 2, 4 }, lista.Select(v => v.IdVenda));
    }

    [Fact]
    public async Task ListarAsync_CombinaFiltros()
    {
        var filtro = new VendaFiltro { Produto = "Camiseta", QuantidadeMinima = 2 };

        var lista = await _repo.ListarAsync(filtro);

        Assert.Equal(1, Assert.Single(lista).IdVenda);
    }

    [Fact]
    public async Task ExisteAsync_RetornaVerdadeiroSomenteParaIdCadastrado()
    {
        Assert.True(await _repo.ExisteAsync(1));
        Assert.False(await _repo.ExisteAsync(999));
    }

    [Fact]
    public async Task IdsExistentesAsync_RetornaApenasOsJaCadastrados()
    {
        var ids = await _repo.IdsExistentesAsync(new[] { 2, 4, 50 });

        Assert.Equal(new[] { 2, 4 }, ids.OrderBy(i => i));
    }

    [Fact]
    public async Task UnitOfWork_CommitPersisteAlteracoes()
    {
        var uow = new UnitOfWork(_db, _repo);

        await uow.Vendas.AdicionarAsync(Nova(10, "Boné", 5, 29.90m, new DateOnly(2026, 9, 11)));
        await uow.CommitAsync();

        Assert.True(await _repo.ExisteAsync(10));
    }

    [Fact]
    public async Task Remover_ApagaVendaAposCommit()
    {
        var uow = new UnitOfWork(_db, _repo);
        var venda = await _repo.ObterAsync(2);

        uow.Vendas.Remover(venda!);
        await uow.CommitAsync();

        Assert.False(await _repo.ExisteAsync(2));
    }
}

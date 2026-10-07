using System.ComponentModel.DataAnnotations;
using Vendas.Api.Models;

namespace Vendas.Api.Dtos;

public class VendaDto
{
    [Range(1, int.MaxValue)]
    public int IdVenda { get; set; }

    [Required, StringLength(100, MinimumLength = 1)]
    public string Produto { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantidade { get; set; }

    [Range(typeof(decimal), "0", "1000000000")]
    public decimal PrecoUnitario { get; set; }

    public DateOnly DataVenda { get; set; }

    public Venda ToEntity() => new()
    {
        IdVenda = IdVenda,
        Produto = Produto.Trim(),
        Quantidade = Quantidade,
        PrecoUnitario = PrecoUnitario,
        DataVenda = DataVenda
    };

    public static VendaDto From(Venda v) => new()
    {
        IdVenda = v.IdVenda,
        Produto = v.Produto,
        Quantidade = v.Quantidade,
        PrecoUnitario = v.PrecoUnitario,
        DataVenda = v.DataVenda
    };
}

public record ImportacaoResultado(int Importadas);

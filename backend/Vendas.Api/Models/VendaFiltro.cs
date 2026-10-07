namespace Vendas.Api.Models;

/// <summary>Filtros opcionais da listagem (quantidade e data_venda, além do produto).</summary>
public class VendaFiltro
{
    public string? Produto { get; set; }
    public int? QuantidadeMinima { get; set; }
    public int? QuantidadeMaxima { get; set; }
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
}

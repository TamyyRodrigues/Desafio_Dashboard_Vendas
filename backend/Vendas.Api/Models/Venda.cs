namespace Vendas.Api.Models;

public class Venda
{
    public int IdVenda { get; set; }
    public string Produto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public DateOnly DataVenda { get; set; }
}

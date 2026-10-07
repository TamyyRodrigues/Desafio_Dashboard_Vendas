using Microsoft.EntityFrameworkCore;
using Vendas.Api.Models;

namespace Vendas.Api.Data;

public class VendasDbContext(DbContextOptions<VendasDbContext> options) : DbContext(options)
{
    public DbSet<Venda> Vendas => Set<Venda>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Venda>(e =>
        {
            e.ToTable("vendas");
            e.HasKey(v => v.IdVenda);
            e.Property(v => v.IdVenda).ValueGeneratedNever(); // o id vem do CSV
            e.Property(v => v.Produto).IsRequired().HasMaxLength(100);
            e.Property(v => v.PrecoUnitario).HasPrecision(18, 2);
            e.HasIndex(v => v.DataVenda);
        });
    }
}

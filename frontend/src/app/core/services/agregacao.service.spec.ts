import { Venda } from '../models/venda.model';
import { AgregacaoService } from './agregacao.service';

describe('AgregacaoService', () => {
  const service = new AgregacaoService();

  const vendas: Venda[] = [
    { id_venda: 1, produto: 'Camiseta', quantidade: 3, preco_unitario: 49.9, data_venda: '2026-09-06' },
    { id_venda: 2, produto: 'Calça', quantidade: 2, preco_unitario: 99.9, data_venda: '2026-09-07' },
    { id_venda: 3, produto: 'Camiseta', quantidade: 1, preco_unitario: 49.9, data_venda: '2026-09-09' },
    { id_venda: 4, produto: 'Tênis', quantidade: 1, preco_unitario: 699.9, data_venda: '2026-09-10' }
  ];

  it('agrupa por produto somando quantidade e valor, ordenado por nome', () => {
    const r = service.agruparPorProduto(vendas);

    expect(r.map(p => p.produto)).toEqual(['Calça', 'Camiseta', 'Tênis']);
    const camiseta = r.find(p => p.produto === 'Camiseta')!;
    expect(camiseta.quantidadeTotal).toBe(4);
    expect(camiseta.valorTotal).toBe(199.6);
    expect(camiseta.vendas.length).toBe(2);
  });

  it('usa a data de venda mais recente do produto', () => {
    const camiseta = service.agruparPorProduto(vendas).find(p => p.produto === 'Camiseta')!;

    expect(camiseta.dataVenda).toBe('2026-09-09');
  });

  it('não acumula erro de ponto flutuante nos valores', () => {
    const r = service.agruparPorProduto([
      { id_venda: 1, produto: 'Bala', quantidade: 1, preco_unitario: 0.1, data_venda: '2026-09-01' },
      { id_venda: 2, produto: 'Bala', quantidade: 2, preco_unitario: 0.1, data_venda: '2026-09-01' }
    ]);

    expect(r[0].valorTotal).toBe(0.3);
  });

  it('agrupa nomes iguais ignorando maiúsculas/minúsculas', () => {
    const r = service.agruparPorProduto([
      { id_venda: 1, produto: 'Camiseta', quantidade: 1, preco_unitario: 10, data_venda: '2026-09-01' },
      { id_venda: 2, produto: 'camiseta', quantidade: 1, preco_unitario: 10, data_venda: '2026-09-02' }
    ]);

    expect(r.length).toBe(1);
    expect(r[0].quantidadeTotal).toBe(2);
  });

  it('retorna lista vazia para entrada vazia', () => {
    expect(service.agruparPorProduto([])).toEqual([]);
  });

  it('filtra por produto ignorando acentos e caixa', () => {
    const agregados = service.agruparPorProduto(vendas);

    expect(service.filtrarPorProduto(agregados, 'CALCA').map(p => p.produto)).toEqual(['Calça']);
    expect(service.filtrarPorProduto(agregados, 'ca').map(p => p.produto)).toEqual(['Calça', 'Camiseta']);
    expect(service.filtrarPorProduto(agregados, '  ').length).toBe(3);
    expect(service.filtrarPorProduto(agregados, 'xyz')).toEqual([]);
  });
});

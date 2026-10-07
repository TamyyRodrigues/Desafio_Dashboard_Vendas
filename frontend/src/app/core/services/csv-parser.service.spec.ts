import { CsvParserService } from './csv-parser.service';

describe('CsvParserService', () => {
  const service = new CsvParserService();
  const CAB = 'id_venda,produto,quantidade,preco_unitario,data_venda';

  it('converte linhas válidas com tipos corretos e data ISO', () => {
    const r = service.parse(`${CAB}\n1,Camiseta,3,49.90,06/09/2026\n4,Tênis,1,699.90,10/09/2026`);

    expect(r.erro).toBeUndefined();
    expect(r.avisos).toEqual([]);
    expect(r.vendas).toEqual([
      { id_venda: 1, produto: 'Camiseta', quantidade: 3, preco_unitario: 49.9, data_venda: '2026-09-06' },
      { id_venda: 4, produto: 'Tênis', quantidade: 1, preco_unitario: 699.9, data_venda: '2026-09-10' }
    ]);
  });

  it('aceita espaços após as vírgulas no cabeçalho e CRLF', () => {
    const r = service.parse('id_venda, produto, quantidade, preco_unitario, data_venda\r\n1,Calça,2,99.90,07/09/2026\r\n');

    expect(r.erro).toBeUndefined();
    expect(r.vendas.length).toBe(1);
  });

  it('remove o BOM e ignora linhas em branco', () => {
    const r = service.parse(`\uFEFF${CAB}\n\n1,Calça,2,99.90,07/09/2026\n\n`);

    expect(r.vendas.length).toBe(1);
    expect(r.avisos).toEqual([]);
  });

  it('retorna erro para arquivo vazio', () => {
    expect(service.parse('  \n ').erro).toContain('vazio');
  });

  it('retorna erro para cabeçalho inválido', () => {
    const r = service.parse('id,produto,qtd,preco,data\n1,Calça,2,99.90,07/09/2026');

    expect(r.erro).toContain('Cabeçalho inválido');
    expect(r.vendas).toEqual([]);
  });

  it('gera aviso para linha com número de colunas errado e segue com as demais', () => {
    const r = service.parse(`${CAB}\n1,Calça,2,99.90\n2,Camiseta,1,49.90,06/09/2026`);

    expect(r.avisos.length).toBe(1);
    expect(r.avisos[0]).toContain('Linha 2');
    expect(r.vendas.length).toBe(1);
  });

  it('gera aviso para quantidade, preço e data inválidos', () => {
    const r = service.parse(`${CAB}\n1,Calça,abc,99.90,07/09/2026\n2,Calça,1,xx,07/09/2026\n3,Calça,1,9.90,31/02/2026`);

    expect(r.vendas).toEqual([]);
    expect(r.avisos.length).toBe(3);
    expect(r.avisos[0]).toContain('quantidade');
    expect(r.avisos[1]).toContain('preco_unitario');
    expect(r.avisos[2]).toContain('data_venda');
  });

  it('gera aviso para id repetido no arquivo', () => {
    const r = service.parse(`${CAB}\n1,Calça,1,99.90,07/09/2026\n1,Camiseta,1,49.90,06/09/2026`);

    expect(r.vendas.length).toBe(1);
    expect(r.avisos[0]).toContain('repetido');
  });
});

import { CurrencyBrPipe } from './currency-br.pipe';

describe('CurrencyBrPipe', () => {
  const pipe = new CurrencyBrPipe();
  const normalizar = (s: string) => s.replace(/\u00a0/g, ' ');

  it('formata valores em reais no padrão pt-BR', () => {
    expect(normalizar(pipe.transform(99.8))).toBe('R$ 99,80');
    expect(normalizar(pipe.transform(1234.5))).toBe('R$ 1.234,50');
  });

  it('trata null/undefined como zero', () => {
    expect(normalizar(pipe.transform(null))).toBe('R$ 0,00');
    expect(normalizar(pipe.transform(undefined))).toBe('R$ 0,00');
  });
});

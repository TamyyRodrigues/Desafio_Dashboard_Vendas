/** Venda no formato da API (snake_case, igual ao CSV). data_venda em ISO yyyy-MM-dd. */
export interface Venda {
  id_venda: number;
  produto: string;
  quantidade: number;
  preco_unitario: number;
  data_venda: string;
}

export interface ProdutoAgregado {
  produto: string;
  quantidadeTotal: number;
  valorTotal: number;
  /** Data da venda mais recente do produto (ISO yyyy-MM-dd). */
  dataVenda: string;
  vendas: Venda[];
}

export interface ResultadoParse {
  vendas: Venda[];
  avisos: string[];
  /** Erro fatal (arquivo vazio ou cabeçalho inválido). */
  erro?: string;
}

export interface ResultadoImportacao {
  importadas: number;
}

export interface FiltroVendas {
  produto?: string;
  quantidadeMinima?: number;
  quantidadeMaxima?: number;
  dataInicio?: string;
  dataFim?: string;
}

export interface Alerta {
  tipo: 'success' | 'info' | 'warning' | 'danger';
  mensagem: string;
}

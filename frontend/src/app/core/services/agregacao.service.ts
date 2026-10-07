import { Injectable } from '@angular/core';

import { ProdutoAgregado, Venda } from '../models/venda.model';

@Injectable({ providedIn: 'root' })
export class AgregacaoService {
  /** Agrupa as vendas por produto (sem diferenciar maiúsculas/minúsculas), ordenado por nome. */
  agruparPorProduto(vendas: Venda[]): ProdutoAgregado[] {
    const grupos = new Map<string, { agregado: ProdutoAgregado; centavos: number }>();

    for (const venda of vendas) {
      const chave = venda.produto.trim().toLowerCase();
      let grupo = grupos.get(chave);
      if (!grupo) {
        grupo = {
          agregado: {
            produto: venda.produto.trim(),
            quantidadeTotal: 0,
            valorTotal: 0,
            dataVenda: venda.data_venda,
            vendas: []
          },
          centavos: 0
        };
        grupos.set(chave, grupo);
      }
      grupo.agregado.quantidadeTotal += venda.quantidade;
      grupo.centavos += Math.round(venda.quantidade * venda.preco_unitario * 100);
      if (venda.data_venda > grupo.agregado.dataVenda) {
        grupo.agregado.dataVenda = venda.data_venda;
      }
      grupo.agregado.vendas.push(venda);
    }

    return Array.from(grupos.values())
      .map(({ agregado, centavos }) => ({ ...agregado, valorTotal: centavos / 100 }))
      .sort((a, b) => a.produto.localeCompare(b.produto, 'pt-BR'));
  }

  /** Filtro por nome de produto, ignorando acentos e maiúsculas. */
  filtrarPorProduto(agregados: ProdutoAgregado[], termo: string): ProdutoAgregado[] {
    const alvo = this.normalizar(termo);
    if (alvo === '') {
      return agregados;
    }
    return agregados.filter(p => this.normalizar(p.produto).includes(alvo));
  }

  private normalizar(texto: string): string {
    return texto
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .trim()
      .toLowerCase();
  }
}

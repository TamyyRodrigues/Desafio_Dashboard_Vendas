import { Injectable } from '@angular/core';

import { ResultadoParse, Venda } from '../models/venda.model';

export const CABECALHO_ESPERADO = ['id_venda', 'produto', 'quantidade', 'preco_unitario', 'data_venda'];

/**
 * Parse manual do CSV de vendas (somente split, sem libs externas).
 * Limitação conhecida: o nome do produto não pode conter vírgula.
 */
@Injectable({ providedIn: 'root' })
export class CsvParserService {
  parse(texto: string): ResultadoParse {
    const linhas = texto.replace(/^\uFEFF/, '').split(/\r?\n/);

    let inicio = 0;
    while (inicio < linhas.length && linhas[inicio].trim() === '') {
      inicio++;
    }
    if (inicio >= linhas.length) {
      return { vendas: [], avisos: [], erro: 'O arquivo está vazio.' };
    }

    const cabecalho = linhas[inicio].split(',').map(c => c.trim().toLowerCase());
    const cabecalhoValido =
      cabecalho.length === CABECALHO_ESPERADO.length &&
      cabecalho.every((coluna, i) => coluna === CABECALHO_ESPERADO[i]);
    if (!cabecalhoValido) {
      return {
        vendas: [],
        avisos: [],
        erro: `Cabeçalho inválido. Esperado: ${CABECALHO_ESPERADO.join(',')}`
      };
    }

    const vendas: Venda[] = [];
    const avisos: string[] = [];
    const idsVistos = new Set<number>();

    for (let i = inicio + 1; i < linhas.length; i++) {
      const linha = linhas[i];
      if (linha.trim() === '') {
        continue;
      }
      const numeroLinha = i + 1;
      const campos = linha.split(',').map(c => c.trim());

      if (campos.length !== CABECALHO_ESPERADO.length) {
        avisos.push(`Linha ${numeroLinha}: esperadas 5 colunas, encontradas ${campos.length}.`);
        continue;
      }

      const [idTxt, produto, qtdTxt, precoTxt, dataTxt] = campos;
      const erros: string[] = [];

      const id = /^\d+$/.test(idTxt) ? Number(idTxt) : NaN;
      if (!(id > 0)) {
        erros.push('id_venda deve ser um inteiro positivo');
      } else if (idsVistos.has(id)) {
        erros.push(`id_venda ${id} repetido`);
      }

      if (produto === '') {
        erros.push('produto não pode ser vazio');
      }

      const quantidade = /^\d+$/.test(qtdTxt) ? Number(qtdTxt) : NaN;
      if (!(quantidade > 0)) {
        erros.push('quantidade deve ser um inteiro positivo');
      }

      const preco = /^\d+(\.\d+)?$/.test(precoTxt) ? Number(precoTxt) : NaN;
      if (Number.isNaN(preco)) {
        erros.push('preco_unitario deve ser numérico (ex.: 49.90)');
      }

      const dataIso = this.converterData(dataTxt);
      if (dataIso === null) {
        erros.push('data_venda deve estar no formato dd/MM/aaaa e ser uma data válida');
      }

      if (erros.length > 0) {
        avisos.push(`Linha ${numeroLinha}: ${erros.join('; ')}.`);
        continue;
      }

      idsVistos.add(id);
      vendas.push({
        id_venda: id,
        produto,
        quantidade,
        preco_unitario: preco,
        data_venda: dataIso as string
      });
    }

    return { vendas, avisos };
  }

  /** dd/MM/yyyy -> yyyy-MM-dd; null se o formato ou a data forem inválidos. */
  private converterData(texto: string): string | null {
    const m = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(texto);
    if (!m) {
      return null;
    }
    const dia = Number(m[1]);
    const mes = Number(m[2]);
    const ano = Number(m[3]);
    const data = new Date(ano, mes - 1, dia);
    const valida =
      data.getFullYear() === ano && data.getMonth() === mes - 1 && data.getDate() === dia;
    return valida ? `${m[3]}-${m[2]}-${m[1]}` : null;
  }
}

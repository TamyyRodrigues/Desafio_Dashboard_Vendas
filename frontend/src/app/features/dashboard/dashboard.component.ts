import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';

import { Alerta, ProdutoAgregado, Venda } from '../../core/models/venda.model';
import { AgregacaoService } from '../../core/services/agregacao.service';
import { CsvParserService } from '../../core/services/csv-parser.service';
import { VendasService } from '../../core/services/vendas.service';

const CHAVE_STORAGE = 'vendas:ultimo-csv';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  @ViewChild('detalhesModal') detalhesModal!: TemplateRef<unknown>;

  agregados: ProdutoAgregado[] = [];
  filtrados: ProdutoAgregado[] = [];
  filtro = '';
  alertas: Alerta[] = [];
  selecionado: ProdutoAgregado | null = null;
  modalRef?: BsModalRef;

  chartData: any = { labels: [], datasets: [] };
  chartOptions: any = {
    responsive: true,
    maintainAspectRatio: false,
    scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
  };

  constructor(
    private parser: CsvParserService,
    private agregacao: AgregacaoService,
    private api: VendasService,
    private modal: BsModalService
  ) {}

  ngOnInit(): void {
    // Bônus: restaura o último CSV salvo no localStorage; senão, tenta carregar da API.
    const salvo = this.lerLocal();
    if (salvo) {
      const resultado = this.parser.parse(salvo);
      if (!resultado.erro && resultado.vendas.length > 0) {
        this.definirVendas(resultado.vendas);
        return;
      }
    }
    this.api.listar().subscribe({
      next: vendas => this.definirVendas(vendas),
      error: () => this.avisar('warning', 'Não foi possível carregar as vendas da API. Importe um CSV para começar.')
    });
  }

  aoCarregarCsv(texto: string): void {
    this.alertas = [];
    const resultado = this.parser.parse(texto);

    if (resultado.erro) {
      this.avisar('danger', resultado.erro);
      return;
    }
    resultado.avisos.forEach(aviso => this.avisar('warning', aviso));

    if (resultado.vendas.length === 0) {
      this.avisar('warning', 'Nenhuma venda válida foi encontrada no arquivo.');
      return;
    }

    this.salvarLocal(texto);
    this.definirVendas(resultado.vendas);

    this.api.importar(resultado.vendas).subscribe({
      next: r => this.avisar('success', `${r.importadas} venda(s) importada(s) para a API.`),
      error: (e: HttpErrorResponse) =>
        this.avisar(
          'warning',
          e.status === 409
            ? `${e.error?.title ?? 'IDs já cadastrados na API.'} Os dados estão sendo exibidos apenas localmente.`
            : 'API indisponível: os dados estão sendo exibidos apenas localmente.'
        )
    });
  }

  aplicarFiltro(): void {
    this.filtrados = this.agregacao.filtrarPorProduto(this.agregados, this.filtro);
    this.atualizarGrafico();
  }

  abrirDetalhes(produto: ProdutoAgregado): void {
    this.selecionado = produto;
    this.modalRef = this.modal.show(this.detalhesModal, {
      class: 'modal-lg',
      ariaLabelledBy: 'titulo-detalhes'
    });
  }

  exportarCsv(): void {
    const linhas = ['produto,quantidade_total,valor_total,data_venda'];
    for (const p of this.filtrados) {
      const [ano, mes, dia] = p.dataVenda.split('-');
      linhas.push([this.escaparCsv(p.produto), p.quantidadeTotal, p.valorTotal.toFixed(2), `${dia}/${mes}/${ano}`].join(','));
    }
    // BOM (\uFEFF) faz o Excel reconhecer UTF-8 e preservar ç e acentos; CRLF é o padrão do Excel no Windows.
    const blob = new Blob(['\uFEFF' + linhas.join('\r\n')], { type: 'text/csv;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = 'vendas-agregadas.csv';
    link.click();
    URL.revokeObjectURL(url);
  }

  avisar(tipo: Alerta['tipo'], mensagem: string): void {
    this.alertas = [...this.alertas, { tipo, mensagem }];
  }

  fecharAlerta(alerta: Alerta): void {
    this.alertas = this.alertas.filter(a => a !== alerta);
  }

  private definirVendas(vendas: Venda[]): void {
    this.agregados = this.agregacao.agruparPorProduto(vendas);
    this.aplicarFiltro();
  }

  private atualizarGrafico(): void {
    this.chartData = {
      labels: this.filtrados.map(p => p.produto),
      datasets: [
        {
          label: 'Quantidade vendida',
          data: this.filtrados.map(p => p.quantidadeTotal),
          backgroundColor: '#42A5F5'
        }
      ]
    };
  }

  private escaparCsv(valor: string): string {
    return /[",\n]/.test(valor) ? `"${valor.replace(/"/g, '""')}"` : valor;
  }

  private lerLocal(): string | null {
    try {
      return localStorage.getItem(CHAVE_STORAGE);
    } catch {
      return null;
    }
  }

  private salvarLocal(texto: string): void {
    try {
      localStorage.setItem(CHAVE_STORAGE, texto);
    } catch {
      /* armazenamento indisponível ou cheio: segue sem persistir */
    }
  }
}

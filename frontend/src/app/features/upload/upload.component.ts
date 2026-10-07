import { Component, EventEmitter, Output } from '@angular/core';

import { decodificarTexto } from '../../core/utils/decodificar-texto';

/** Upload do CSV: lê o arquivo com FileReader e emite o texto bruto. O parse fica no CsvParserService. */
@Component({
  selector: 'app-upload',
  template: `
    <div class="form-group mb-0">
      <label for="arquivo-csv" class="font-weight-bold">Importar CSV de vendas</label>
      <input
        id="arquivo-csv"
        type="file"
        class="form-control-file"
        accept=".csv,text/csv"
        aria-label="Selecionar arquivo CSV de vendas"
        aria-describedby="ajuda-csv"
        (change)="aoSelecionar($event)"
      />
      <small id="ajuda-csv" class="form-text text-muted">
        Cabeçalho esperado: id_venda,produto,quantidade,preco_unitario,data_venda
      </small>
    </div>
  `
})
export class UploadComponent {
  @Output() csvLoaded = new EventEmitter<string>();
  @Output() readError = new EventEmitter<string>();

  aoSelecionar(evento: Event): void {
    const input = evento.target as HTMLInputElement;
    const arquivo = input.files?.[0];
    if (!arquivo) {
      return;
    }

    const leitor = new FileReader();
    // Lê os bytes e decodifica manualmente: aceita UTF-8 e ANSI (Windows-1252).
    leitor.onload = () => this.csvLoaded.emit(decodificarTexto(leitor.result as ArrayBuffer));
    leitor.onerror = () => this.readError.emit('Não foi possível ler o arquivo selecionado.');
    leitor.readAsArrayBuffer(arquivo);

    input.value = ''; // permite selecionar o mesmo arquivo novamente
  }
}

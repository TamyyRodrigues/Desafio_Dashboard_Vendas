import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'currencyBr' })
export class CurrencyBrPipe implements PipeTransform {
  private readonly formatador = new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL'
  });

  transform(valor: number | null | undefined): string {
    return this.formatador.format(valor ?? 0);
  }
}

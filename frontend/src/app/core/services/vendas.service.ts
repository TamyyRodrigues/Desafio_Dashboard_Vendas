import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { FiltroVendas, ResultadoImportacao, Venda } from '../models/venda.model';

@Injectable({ providedIn: 'root' })
export class VendasService {
  private readonly url = environment.apiUrl;

  constructor(private http: HttpClient) {}

  listar(filtro: FiltroVendas = {}): Observable<Venda[]> {
    let params = new HttpParams();
    (Object.keys(filtro) as (keyof FiltroVendas)[]).forEach(chave => {
      const valor = filtro[chave];
      if (valor !== undefined && valor !== null && valor !== '') {
        params = params.set(chave, String(valor));
      }
    });
    return this.http.get<Venda[]>(this.url, { params });
  }

  obter(id: number): Observable<Venda> {
    return this.http.get<Venda>(`${this.url}/${id}`);
  }

  criar(venda: Venda): Observable<Venda> {
    return this.http.post<Venda>(this.url, venda);
  }

  /** Envia em lote as vendas lidas do CSV (a API grava tudo ou nada). */
  importar(vendas: Venda[]): Observable<ResultadoImportacao> {
    return this.http.post<ResultadoImportacao>(`${this.url}/importacao`, vendas);
  }

  atualizar(id: number, venda: Venda): Observable<Venda> {
    return this.http.put<Venda>(`${this.url}/${id}`, venda);
  }

  remover(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}

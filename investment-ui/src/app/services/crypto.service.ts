import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CryptoService {
  private apiUrl = 'http://localhost:5247/api/prices/current';

  constructor(private http: HttpClient) {}

  getPrices(symbols: string[] = ['BTC', 'ETH', 'SOL']): Observable<{ [symbol: string]: number }> {
    const normalizedSymbols = symbols.map(s => s.toUpperCase());
    const query = normalizedSymbols.join(',');

    return this.http.get<Record<string, number>>(`${this.apiUrl}?symbols=${query}`).pipe(
      catchError(error => this.handleError(error))
    );
  }

  private handleError(error: HttpErrorResponse): Observable<Record<string, number>> {
    console.error('Błąd pobierania cen kryptowalut:', error);
    return of({});
  }
}

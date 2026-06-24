import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CryptoService {
  private apiUrl = 'http://localhost:5247/api/prices/current';

  constructor(private http: HttpClient) {}

  /**
   * Pobiera ceny dla symboli (krypto, akcje, ETF)
   * @param symbols - tablica symboli (BTC, AAPL, CSPX itp)
   * @param type - typ papieru: 'crypto' | 'stocks' | 'all'
   */
  getPrices(symbols: string[] = ['BTC', 'ETH', 'SOL'], type: 'crypto' | 'stocks' | 'all' = 'crypto'): Observable<{ [symbol: string]: number }> {
    const normalizedSymbols = symbols.map(s => s.toUpperCase());
    const query = normalizedSymbols.join(',');

    // Dodaj parametr type do zapytania
    const url = `${this.apiUrl}?symbols=${query}&type=${type}`;
    
    console.log(`[CryptoService] Pobieranie cen: ${type} - ${query}`);

    return this.http.get<Record<string, number>>(url).pipe(
      catchError(error => this.handleError(error, type))
    );
  }

  /**
   * Pobiera ceny dla kryptowalut
   */
  getCryptoPrices(symbols: string[]): Observable<{ [symbol: string]: number }> {
    return this.getPrices(symbols, 'crypto');
  }

  /**
   * Pobiera ceny dla akcji i ETF
   */
  getStockPrices(symbols: string[]): Observable<{ [symbol: string]: number }> {
    return this.getPrices(symbols, 'stocks');
  }

  private handleError(error: HttpErrorResponse, type: string): Observable<Record<string, number>> {
    console.error(`[CryptoService] Błąd pobierania cen (${type}):`, error);
    return of({});
  }
}

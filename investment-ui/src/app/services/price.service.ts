import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';

export interface PriceResponse {
  [symbol: string]: number;
}

@Injectable({
  providedIn: 'root'
})
export class PriceService {
  private cryptoApiUrl = 'http://localhost:5247/api/prices/current';
  private stocksApiUrl = 'http://localhost:5247/api/prices/stocks';
  private goldApiUrl = 'http://localhost:5247/api/prices/gold';

  constructor(private http: HttpClient) {}

  /**
   * Pobiera ceny kryptowalut
   */
  getCryptoPrices(symbols: string[] = ['BTC', 'ETH', 'SOL']): Observable<PriceResponse> {
    const normalizedSymbols = symbols.map(s => s.toUpperCase());
    const query = normalizedSymbols.join(',');

    return this.http.get<PriceResponse>(`${this.cryptoApiUrl}?symbols=${query}`).pipe(
      catchError(error => this.handleError(error, 'Crypto'))
    );
  }

  /**
   * Pobiera ceny akcji i ETF
   */
  getStockPrices(symbols: string[]): Observable<PriceResponse> {
    const normalizedSymbols = symbols.map(s => s.toUpperCase());
    const query = normalizedSymbols.join(',');

    return this.http.get<PriceResponse>(`${this.stocksApiUrl}?symbols=${query}`).pipe(
      catchError(error => this.handleError(error, 'Stocks'))
    );
  }

  /**
   * Pobiera ceny złota
   * @param units - jednostki: "g" (gram), "oz" (uncja)
   */
  getGoldPrices(units: string[] = ['g', 'oz']): Observable<PriceResponse> {
    const query = units.join(',');

    return this.http.get<PriceResponse>(`${this.goldApiUrl}?units=${query}`).pipe(
      catchError(error => this.handleError(error, 'Gold'))
    );
  }

  /**
   * Uniwersalna metoda do pobierania cen
   */
  getPrices(symbols: string[], type: 'crypto' | 'stocks' | 'gold' = 'crypto'): Observable<PriceResponse> {
    switch (type) {
      case 'crypto':
        return this.getCryptoPrices(symbols);
      case 'stocks':
        return this.getStockPrices(symbols);
      case 'gold':
        return this.getGoldPrices(symbols);
      default:
        return this.getCryptoPrices(symbols);
    }
  }

  private handleError(error: HttpErrorResponse, type: string): Observable<PriceResponse> {
    console.error(`Błąd pobierania cen ${type}:`, error);
    return of({});
  }
}

import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, map, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CryptoService {
  constructor(private http: HttpClient) {}

  getPrices(symbols: string[] = ['BTC', 'ETH', 'SOL']): Observable<{ [symbol: string]: number }> {
    const normalizedSymbols = symbols.map(s => s.toUpperCase());
    const ids = normalizedSymbols.map(s => this.mapToCoinGeckoId(s)).join(',');
    const url = `https://api.coingecko.com/api/v3/simple/price?ids=${ids}&vs_currencies=pln`;

    return this.http.get<Record<string, { pln: number }>>(url).pipe(
      map(response => {
        const result: { [symbol: string]: number } = {};

        for (const symbol of normalizedSymbols) {
          const id = this.mapToCoinGeckoId(symbol);
          result[symbol.toLowerCase()] = response[id]?.pln ?? 0;
        }

        return result;
      }),
      catchError(error => this.handleError(error))
    );
  }

  private mapToCoinGeckoId(symbol: string): string {
    const map: { [key: string]: string } = {
      BTC: 'bitcoin',
      ETH: 'ethereum',
      SOL: 'solana',
      DOGE: 'dogecoin',
      SHIB: 'shiba-inu',
      XRP: 'ripple',
      ADA: 'cardano',
      LINK: 'chainlink',
      USDT: 'tether'
    };

    return map[symbol] || symbol.toLowerCase();
  }

  private handleError(error: HttpErrorResponse): Observable<Record<string, number>> {
    console.error('Błąd pobierania cen kryptowalut:', error);
    return of({} as Record<string, number>);
  }
}

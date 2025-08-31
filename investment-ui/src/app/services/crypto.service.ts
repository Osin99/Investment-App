import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CryptoService {
  constructor(private http: HttpClient) {}

  // 🔁 Pobiera ceny dla podanych symboli lub domyślnych, w PLN
  getPrices(symbols: string[] = ['BTC', 'ETH', 'SOL']): Observable<{ [symbol: string]: number }> {
    const ids = symbols.map(s => this.mapToCoinGeckoId(s)).join(',');
    const url = `https://api.coingecko.com/api/v3/simple/price?ids=${ids}&vs_currencies=pln`;

    return this.http.get<any>(url).pipe(
      map(response => {
        const result: { [symbol: string]: number } = {};
        for (const symbol of symbols) {
          const id = this.mapToCoinGeckoId(symbol);
          result[symbol.toLowerCase()] = response[id]?.pln ?? 0;
        }
        return result;
      })
    );
  }

  // 🔄 Mapuje symbol (np. BTC) na ID w CoinGecko (np. bitcoin)
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
      // dodaj więcej w razie potrzeby
    };

    return map[symbol.toUpperCase()] || symbol.toLowerCase();
  }
}

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, throwError } from 'rxjs';
import { catchError, retry, delay, map } from 'rxjs/operators';

export interface HistoricalPrice {
  date: string;
  price: number;
  currency: string;
}

@Injectable({
  providedIn: 'root'
})
export class HistoricalPriceService {
  private apiUrl = 'http://localhost:5247/api/prices';

  constructor(private http: HttpClient) {}

  /**
   * Pobiera historyczną cenę zamknięcia dla akcji/ETF na konkretny dzień
   * @param symbol - ticker akcji/ETF (np. AAPL, CSPX)
   * @param date - data w formacie YYYY-MM-DD
   * @returns Observable z ceną lub null jeśli niedostępna
   */
  getHistoricalPrice(symbol: string, date: string): Observable<number | null> {
    const normalizedSymbol = symbol.toUpperCase();
    
    // Konwertuj datę na unix timestamp
    const dateObj = new Date(date);
    const from = Math.floor(dateObj.getTime() / 1000);
    const to = from + 86400; // +1 dzień

    console.log(`[PriceAutoFetch] Pobieranie ceny historycznej: ${normalizedSymbol} na dzień ${date}`);

    // Endpoint do Finnhub: /stock/candle
    const url = `${this.apiUrl}/historical?symbol=${normalizedSymbol}&from=${from}&to=${to}`;

    return this.http.get<{ [key: string]: number }>(url).pipe(
      retry({
        count: 3,
        delay: (error, retryCount) => {
          if (error.status === 429) {
            console.warn(`[PriceAutoFetch] 429 Too Many Requests - próba ${retryCount}/3, czekam 3s`);
            return of(null).pipe(delay(3000));
          }
          return throwError(() => error);
        }
      }),
      catchError(error => {
        console.error(`[PriceAutoFetch] Błąd pobierania ceny dla ${normalizedSymbol}:`, error);
        return of(null);
      }),
      // Wyciągnij cenę z obiektu
      map(response => {
        if (response && typeof response === 'object') {
          const price = Object.values(response)[0];
          return typeof price === 'number' ? price : null;
        }
        return null;
      })
    );
  }

  /**
   * Pobiera cenę z najbliższego dostępnego dnia (maksymalnie 5 dni wstecz)
   * @param symbol - ticker akcji/ETF
   * @param date - data w formacie YYYY-MM-DD
   * @returns Observable z ceną lub null
   */
  getHistoricalPriceWithFallback(symbol: string, date: string): Observable<number | null> {
    return new Observable(observer => {
      this.tryGetPriceWithRetry(symbol, date, 0, observer);
    });
  }

  private tryGetPriceWithRetry(
    symbol: string,
    date: string,
    daysBack: number,
    observer: any
  ): void {
    if (daysBack > 5) {
      console.warn(`[PriceAutoFetch] Nie znaleziono ceny dla ${symbol} w ciągu ostatnich 5 dni`);
      observer.next(null);
      observer.complete();
      return;
    }

    const dateObj = new Date(date);
    dateObj.setDate(dateObj.getDate() - daysBack);
    const adjustedDate = dateObj.toISOString().split('T')[0];

    this.getHistoricalPrice(symbol, adjustedDate).subscribe({
      next: price => {
        if (price && price > 0) {
          if (daysBack > 0) {
            console.log(`[PriceAutoFetch] Cena znaleziona dla ${symbol} z ${adjustedDate} (${daysBack} dni wstecz): ${price}`);
          }
          observer.next(price);
          observer.complete();
        } else {
          // Spróbuj dzień wcześniej
          this.tryGetPriceWithRetry(symbol, date, daysBack + 1, observer);
        }
      },
      error: err => {
        // Spróbuj dzień wcześniej
        this.tryGetPriceWithRetry(symbol, date, daysBack + 1, observer);
      }
    });
  }
}

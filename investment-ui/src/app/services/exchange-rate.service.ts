import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

export interface ExchangeRateResponse {
  currency: string;
  rate: number;
  date: string;
}

@Injectable({
  providedIn: 'root'
})
export class ExchangeRateService {
  private nbpBaseUrl = 'https://api.nbp.pl/api/exchangerates/rates/a';

  constructor(private http: HttpClient) {}

  /**
   * Pobiera kurs wymiany dla danej waluty na konkretny dzień
   * @param currency - kod waluty (USD, EUR, GBP itp.)
   * @param date - data w formacie YYYY-MM-DD
   * @returns Observable z kursem lub null jeśli niedostępny
   */
  getExchangeRate(currency: string, date: string): Observable<number | null> {
    const url = `${this.nbpBaseUrl}/${currency.toLowerCase()}/${date}/?format=json`;

    console.log(`[PriceAutoFetch] Pobieranie kursu ${currency} na dzień ${date}`);

    return this.http.get<any>(url).pipe(
      map(response => {
        const rate = response?.rates?.[0]?.mid;
        if (rate && rate > 0) {
          console.log(`[PriceAutoFetch] Kurs ${currency}: ${rate.toFixed(4)} PLN`);
          return rate;
        }
        return null;
      }),
      catchError(error => {
        if (error.status === 404) {
          console.warn(`[PriceAutoFetch] Brak kursu dla ${currency} na dzień ${date} (weekend/święto)`);
          // Spróbuj znaleźć ostatni dostępny kurs
          return this.getLastAvailableRate(currency, date);
        }
        console.error(`[PriceAutoFetch] Błąd pobierania kursu ${currency}:`, error);
        return of(null);
      })
    );
  }

  /**
   * Pobiera ostatni dostępny kurs przed daną datą (maksymalnie 5 dni wstecz)
   */
  private getLastAvailableRate(currency: string, date: string): Observable<number | null> {
    return new Observable(observer => {
      this.tryGetRateWithRetry(currency, date, 1, observer);
    });
  }

  private tryGetRateWithRetry(
    currency: string,
    date: string,
    daysBack: number,
    observer: any
  ): void {
    if (daysBack > 5) {
      console.warn(`[PriceAutoFetch] Nie znaleziono kursu dla ${currency} w ciągu ostatnich 5 dni`);
      observer.next(null);
      observer.complete();
      return;
    }

    const dateObj = new Date(date);
    dateObj.setDate(dateObj.getDate() - daysBack);
    const adjustedDate = dateObj.toISOString().split('T')[0];
    const url = `${this.nbpBaseUrl}/${currency.toLowerCase()}/${adjustedDate}/?format=json`;

    this.http.get<any>(url).pipe(
      catchError(() => of(null))
    ).subscribe({
      next: response => {
        const rate = response?.rates?.[0]?.mid;
        if (rate && rate > 0) {
          console.log(`[PriceAutoFetch] Kurs ${currency}: Użyto kursu z ${adjustedDate}: ${rate.toFixed(4)} PLN`);
          observer.next(rate);
          observer.complete();
        } else {
          this.tryGetRateWithRetry(currency, date, daysBack + 1, observer);
        }
      },
      error: () => {
        this.tryGetRateWithRetry(currency, date, daysBack + 1, observer);
      }
    });
  }

  /**
   * Przelicza cenę z waluty obcej na PLN
   */
  convertToPln(price: number, exchangeRate: number): number {
    return price * exchangeRate;
  }
}

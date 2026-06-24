import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, catchError } from 'rxjs/operators';

export interface SecuritySearchResult {
  id: number;
  symbol: string;
  name: string;
  type: 'Stock' | 'ETF' | 'Crypto' | 'Other';
  exchange?: string;
  currency: string;
  coinGeckoId?: string;
}

@Injectable({
  providedIn: 'root'
})
export class SecuritySearchService {
  private apiUrl = 'http://localhost:5247/api/securities';

  constructor(private http: HttpClient) {}

  searchSecurities(query: string, limit: number = 20): Observable<SecuritySearchResult[]> {
    if (!query || query.trim().length === 0) {
      return of([]);
    }

    return this.http.get<SecuritySearchResult[]>(
      `${this.apiUrl}/search?query=${encodeURIComponent(query)}&limit=${limit}`
    ).pipe(
      catchError(error => {
        console.error('Error searching securities:', error);
        return of([]);
      })
    );
  }
}

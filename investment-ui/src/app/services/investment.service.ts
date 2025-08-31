import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Investment {
  id: number;
  symbol: string;
  amount: number;
  buyPrice: number;
  buyDate: string;
}

export interface InvestmentSummary {
  symbol: string;
  totalAmount: number;
  totalInvested: number;
}

@Injectable({ providedIn: 'root' })
export class InvestmentService {
  private baseUrl = environment.apiBaseUrl;                // np. https://xxx.onrender.com
  private apiUrl  = `${this.baseUrl}/api/investments`;     // pełny HTTPS w produkcji

  constructor(private http: HttpClient) {}

  getInvestments(): Observable<Investment[]> {
    return this.http.get<Investment[]>(this.apiUrl);
  }

  getInvestmentById(id: number): Observable<Investment> {
    return this.http.get<Investment>(`${this.apiUrl}/${id}`);
  }

  addInvestment(investment: Investment): Observable<Investment> {
    return this.http.post<Investment>(this.apiUrl, investment);
  }

  updateInvestment(id: number, investment: Investment): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, investment);
  }

  deleteInvestment(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getInvestmentSummary(): Observable<InvestmentSummary[]> {
    return this.http.get<InvestmentSummary[]>(`${this.apiUrl}/summary`);
  }
}

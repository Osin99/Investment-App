import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export enum TransactionType {
  Buy = 0,
  Sell = 1
}

export interface Investment {
  id: number;
  symbol: string;
  type: TransactionType;
  amount: number;
  buyPrice: number;
  buyDate: string;
}

export interface InvestmentSummary {
  symbol: string;
  totalAmount: number;
  totalInvested: number;
}

export interface PortfolioHistoryPoint {
  date: string;
  totalAmount: number;
  totalInvested: number;
  marketValue: number;
  profit: number;
  profitPercent: number;
  isPurchaseDate: boolean;
  purchaseInvested: number;
}

export interface PurchaseHistory {
  id: number;
  symbol: string;
  type: TransactionType;
  amount: number;
  buyPrice: number;
  buyDate: string;
  totalAmount: number;
  totalInvested: number;
  marketValue: number;
  profit: number;
  profitPercent: number;
}

export interface PortfolioHistory {
  history: PortfolioHistoryPoint[];
  purchases: PurchaseHistory[];
  totalAmount: number;
  totalInvested: number;
  marketValue: number;
  profit: number;
  profitPercent: number;
}

@Injectable({
  providedIn: 'root'
})
export class InvestmentService {
  private apiUrl = 'http://localhost:5247/api/investments';

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

  getPortfolioHistory(): Observable<PortfolioHistory> {
    return this.http.get<PortfolioHistory>(`${this.apiUrl}/history`);
  }
}

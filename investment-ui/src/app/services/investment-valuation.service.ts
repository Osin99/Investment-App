import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { CryptoService } from './crypto.service';
import { InvestmentSummary } from './investment.service';

export interface InvestmentValuation extends InvestmentSummary {
  marketValue: number;
  profit: number;
  profitPercent: number;
}

@Injectable({
  providedIn: 'root'
})
export class InvestmentValuationService {
  constructor(private cryptoService: CryptoService) {}

   getValuations(summary: InvestmentSummary[]): Observable<InvestmentValuation[]> {
     const symbols = summary.map(s => s.symbol);

     return this.cryptoService.getPrices(symbols).pipe(
       map(prices => summary.map(s => this.mapValuation(s, prices[s.symbol.toUpperCase()] || 0)))
     );
   }

   private mapValuation(summary: InvestmentSummary, currentPrice: number): InvestmentValuation {
     const marketValue = summary.totalAmount * currentPrice;
     const profit = marketValue - summary.totalInvested;
     const invested = summary.totalInvested || 1;

     return {
       ...summary,
       marketValue,
       profit,
       profitPercent: (profit / invested) * 100
     };
   }
}

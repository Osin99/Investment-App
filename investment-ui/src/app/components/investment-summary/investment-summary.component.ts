import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InvestmentService, InvestmentSummary } from '../../services/investment.service';
import { CryptoService } from '../../services/crypto.service';

@Component({
  selector: 'app-investment-summary',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './investment-summary.component.html'
})
export class InvestmentSummaryComponent implements OnInit {
  summaries: (InvestmentSummary & { marketValue?: number })[] = [];
  isLoading = true;

  constructor(
    private investmentService: InvestmentService,
    private cryptoService: CryptoService
  ) {}

  ngOnInit(): void {
  this.investmentService.getInvestmentSummary().subscribe({
    next: summary => {
      const symbols = summary.map(s => s.symbol);
      this.cryptoService.getPrices(symbols).subscribe({
        next: prices => {
          this.summaries = summary.map(s => ({
            ...s,
            marketValue: s.totalAmount * (prices[s.symbol.toLowerCase()] || 0)
          }));
          this.isLoading = false;
        },
        error: err => {
          console.error('Błąd podczas pobierania cen z CryptoService:', err);
          this.isLoading = false;
        }
      });
    },
    error: err => {
      console.error('Błąd podczas pobierania danych inwestycji:', err);
      this.isLoading = false;
    }
  });
}
getProfit(s: any): number {
  return (s.marketValue || 0) - s.totalInvested;
}

getProfitPercent(s: any): string {
  const invested = s.totalInvested || 1;
  const profit = this.getProfit(s);
  const percent = (profit / invested) * 100;
  return percent.toFixed(2);
}

}

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InvestmentService } from '../../services/investment.service';
import { CryptoService } from '../../services/crypto.service';

@Component({
  selector: 'app-investment-summary-box',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './investment-summary-box.component.html'
})
export class InvestmentSummaryBoxComponent implements OnInit {
  totalInvested = 0;
  totalMarketValue = 0;
  isLoading = true;

  constructor(
    private investmentService: InvestmentService,
    private cryptoService: CryptoService
  ) {}

  ngOnInit(): void {
    this.investmentService.getInvestmentSummary().subscribe(summary => {
      const symbols = summary.map(s => s.symbol.toLowerCase());

      this.cryptoService.getPrices(symbols).subscribe(prices => {
        this.totalInvested = 0;
        this.totalMarketValue = 0;

        summary.forEach(s => {
          const currentPrice = prices[s.symbol.toLowerCase()] || 0;
          const marketValue = s.totalAmount * currentPrice;

          this.totalInvested += s.totalInvested;
          this.totalMarketValue += marketValue;
        });

        this.isLoading = false;
      });
    });
  }

  get profit(): number {
    return this.totalMarketValue - this.totalInvested;
  }

  get profitPercent(): string {
  const invested = this.totalInvested || 1; // zabezpieczenie
  const percent = (this.profit / invested) * 100;
  return percent.toFixed(2);
}
}

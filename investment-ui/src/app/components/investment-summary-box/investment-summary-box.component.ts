import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InvestmentService } from '../../services/investment.service';
import { InvestmentValuationService } from '../../services/investment-valuation.service';

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
  lastUpdateTime = '';

  constructor(
    private investmentService: InvestmentService,
    private investmentValuationService: InvestmentValuationService
  ) {}

  ngOnInit(): void {
    this.investmentService.getInvestmentSummary().subscribe({
      next: summary => {
        this.investmentValuationService.getValuations(summary).subscribe({
          next: valuations => {
            this.totalInvested = valuations.reduce((total, valuation) => total + valuation.totalInvested, 0);
            this.totalMarketValue = valuations.reduce((total, valuation) => total + valuation.marketValue, 0);
            this.lastUpdateTime = new Date().toLocaleTimeString('pl-PL');
            this.isLoading = false;
          },
          error: err => {
            console.error('Błąd podczas wyceny inwestycji:', err);
            this.isLoading = false;
          }
        });
      },
      error: err => {
        console.error('Błąd pobierania podsumowania inwestycji:', err);
        this.isLoading = false;
      }
    });
  }

  get profit(): number {
    return this.totalMarketValue - this.totalInvested;
  }

  get profitPercent(): string {
    const invested = this.totalInvested || 1;
    const percent = (this.profit / invested) * 100;
    return percent.toFixed(2);
  }
}

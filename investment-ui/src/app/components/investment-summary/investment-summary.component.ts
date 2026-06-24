import { Component, OnInit, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InvestmentService } from '../../services/investment.service';
import { InvestmentValuation, InvestmentValuationService } from '../../services/investment-valuation.service';
import { AssetCategory } from '../../models/asset-category';

@Component({
  selector: 'app-investment-summary',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './investment-summary.component.html'
})
export class InvestmentSummaryComponent implements OnInit, OnChanges {
  @Input() category: AssetCategory | null = null;
  
  summaries: InvestmentValuation[] = [];
  isLoading = true;

  constructor(
    private investmentService: InvestmentService,
    private investmentValuationService: InvestmentValuationService
  ) {}

  ngOnInit(): void {
    this.loadSummary();
  }

  ngOnChanges(): void {
    this.loadSummary();
  }

  private loadSummary(): void {
    this.isLoading = true;
    this.investmentService.getInvestmentSummary(this.category ?? undefined).subscribe({
      next: summary => {
        this.investmentValuationService.getValuations(summary).subscribe({
          next: valuations => {
            this.summaries = valuations;
            this.isLoading = false;
          },
          error: err => {
            console.error('Błąd podczas wyceny inwestycji:', err);
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

  getProfit(s: InvestmentValuation): number {
    return s.profit;
  }

  getProfitPercent(s: InvestmentValuation): string {
    return s.profitPercent.toFixed(2);
  }
}

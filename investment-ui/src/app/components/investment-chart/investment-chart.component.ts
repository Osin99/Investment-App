import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgChartsModule } from 'ng2-charts';
import { ChartType, ChartConfiguration } from 'chart.js';
import { InvestmentService } from '../../services/investment.service';
import { InvestmentValuationService } from '../../services/investment-valuation.service';

@Component({
  selector: 'app-investment-chart',
  standalone: true,
  imports: [CommonModule, NgChartsModule],
  templateUrl: './investment-chart.component.html'
})
export class InvestmentChartComponent implements OnInit {
  chartType: ChartType = 'bar';
  isLoading = true;

  chartData: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: [
      {
        label: 'Zysk / Strata (PLN)',
        data: [],
        backgroundColor: []
      }
    ]
  };

  constructor(
    private investmentService: InvestmentService,
    private investmentValuationService: InvestmentValuationService
  ) {}

  ngOnInit(): void {
    this.investmentService.getInvestmentSummary().subscribe({
      next: summary => {
        this.investmentValuationService.getValuations(summary).subscribe({
          next: valuations => {
            const labels = valuations.map(v => v.symbol.toUpperCase());
            const data = valuations.map(v => v.profit);
            const colors = valuations.map(v => v.profit >= 0 ? 'rgba(75, 192, 192, 0.8)' : 'rgba(255, 99, 132, 0.8)');

            this.chartData = {
              labels,
              datasets: [
                {
                  label: 'Zysk / Strata (PLN)',
                  data,
                  backgroundColor: colors
                }
              ]
            };

            this.isLoading = false;
          },
          error: err => {
            console.error('Błąd pobierania danych wykresu:', err);
            this.isLoading = false;
          }
        });
      },
      error: err => {
        console.error('Błąd pobierania podsumowania dla wykresu:', err);
        this.isLoading = false;
      }
    });
  }
}

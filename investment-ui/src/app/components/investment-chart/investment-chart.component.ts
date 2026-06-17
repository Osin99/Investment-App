import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgChartsModule } from 'ng2-charts';
import { ChartConfiguration, ChartOptions } from 'chart.js';
import { InvestmentService } from '../../services/investment.service';
import { InvestmentValuationService } from '../../services/investment-valuation.service';

@Component({
  selector: 'app-investment-chart',
  standalone: true,
  imports: [CommonModule, NgChartsModule],
  templateUrl: './investment-chart.component.html'
})

export class InvestmentChartComponent implements OnInit {
  chartType: 'bar' = 'bar';
  isLoading = true;
  totalProfit = 0;

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

  chartOptions: ChartOptions<'bar'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false }
    },
    scales: {
      x: {
        grid: { color: 'rgba(148, 163, 184, 0.12)' },
        ticks: { color: '#94a3b8' },
        title: { display: true, text: 'Kryptowaluta', color: '#cbd5e1' }
      },
      y: {
        grid: { color: 'rgba(148, 163, 184, 0.12)' },
        ticks: { color: '#94a3b8' },
        title: { display: true, text: 'Zysk / Strata (PLN)', color: '#cbd5e1' }
      }
    }
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
            const colors = valuations.map(v => v.profit >= 0 ? 'rgba(75, 192, 192, 0.8)' : 'rgba(251, 113, 133, 0.8)');

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

            this.totalProfit = valuations.reduce((total, valuation) => total + valuation.profit, 0);
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



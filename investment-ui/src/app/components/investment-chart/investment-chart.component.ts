import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgChartsModule } from 'ng2-charts';
import { ChartType, ChartConfiguration } from 'chart.js';
import { InvestmentService } from '../../services/investment.service';
import { CryptoService } from '../../services/crypto.service';

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
    private cryptoService: CryptoService
  ) {}

  ngOnInit(): void {
    this.investmentService.getInvestmentSummary().subscribe(summary => {
      const symbols = summary.map(s => s.symbol);

      this.cryptoService.getPrices(symbols).subscribe(prices => {
        const labels: string[] = [];
        const data: number[] = [];
        const colors: string[] = [];

        summary.forEach(s => {
          const symbol = s.symbol.toUpperCase();
          const currentPrice = prices[symbol.toLowerCase()] || 0;
          const marketValue = s.totalAmount * currentPrice;
          const profit = marketValue - s.totalInvested;

          labels.push(symbol);
          data.push(profit);
          colors.push(profit >= 0 ? 'rgba(75, 192, 192, 0.8)' : 'rgba(255, 99, 132, 0.8)');
        });

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
      });
    });
  }
}

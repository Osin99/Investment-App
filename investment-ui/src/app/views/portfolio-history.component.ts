import { Component, OnInit } from '@angular/core';
import { CommonModule, CurrencyPipe, DecimalPipe, NgFor, NgIf, UpperCasePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { NgChartsModule } from 'ng2-charts';
import { ChartConfiguration, ChartOptions } from 'chart.js';
import { CryptoService } from '../services/crypto.service';
import { Investment, InvestmentService, PortfolioHistory, TransactionType } from '../services/investment.service';

interface TransactionValuation {
  id: number;
  symbol: string;
  type: TransactionType;
  amount: number;
  buyPrice: number;
  buyDate: string;
  currentPrice: number;
  invested: number;
  marketValue: number;
  profit: number;
  profitPercent: number;
}
 
@Component({
  selector: 'app-portfolio-history',
  standalone: true,
  imports: [CommonModule, RouterLink, NgChartsModule, NgFor, NgIf, CurrencyPipe, DecimalPipe, UpperCasePipe],
  templateUrl: './portfolio-history.component.html',
  styleUrl: './portfolio-history.component.css'
})
 
export class PortfolioHistoryComponent implements OnInit {
  portfolioHistory: PortfolioHistory | null = null;
  transactionValuations: TransactionValuation[] = [];
  isLoading = true;
  transactionValuationsLoading = false;
  errorMessage = '';
  transactionErrorMessage = '';

  chartType: 'line' = 'line';

  chartOptions: ChartOptions<'line'> = {
    responsive: true,
    maintainAspectRatio: false,
    interaction: {
      mode: 'index',
      intersect: false
    },
    plugins: {
      legend: {
        display: true,
        position: 'bottom',
        labels: {
          color: '#cbd5e1',
          usePointStyle: true,
          pointStyle: 'circle'
        }
      },
      tooltip: {
        backgroundColor: 'rgba(15, 23, 42, 0.94)',
        borderColor: 'rgba(148, 163, 184, 0.24)',
        borderWidth: 1,
        titleColor: '#f8fafc',
        bodyColor: '#cbd5e1',
        padding: 12
      }
    },
    scales: {
      x: {
        grid: { color: 'rgba(148, 163, 184, 0.12)' },
        ticks: { color: '#94a3b8', maxRotation: 0 },
        title: { display: true, text: 'Data', color: '#cbd5e1' }
      },
      y: {
        grid: { color: 'rgba(148, 163, 184, 0.12)' },
        ticks: { color: '#94a3b8' },
        title: { display: true, text: 'PLN', color: '#cbd5e1' }
      }
    }
  };

  // Dane wykresu są budowane z historii portfela pobranej z backendu
  chartData: ChartConfiguration<'line'>['data'] = {
    labels: [],
    datasets: [
      {
        label: 'Wartość portfela wg historycznych cen',
        data: [],
        borderColor: 'rgba(75, 192, 192, 1)',
        backgroundColor: 'rgba(75, 192, 192, 0.2)',
        tension: 0.3
      },
      {
        label: 'Zainwestowany kapitał',
        data: [],
        borderColor: 'rgba(54, 162, 235, 1)',
        backgroundColor: 'rgba(54, 162, 235, 0.2)',
        tension: 0.3
      },
      {
        label: 'Dni wpłat',
        data: [],
        borderColor: 'rgba(255, 159, 64, 1)',
        backgroundColor: 'rgba(255, 159, 64, 1)',
        pointRadius: 6,
        pointHoverRadius: 8,
        showLine: false
      }
    ]
  };

  constructor(
    private investmentService: InvestmentService,
    private cryptoService: CryptoService
  ) {}

  ngOnInit(): void {
    this.investmentService.getPortfolioHistory().subscribe({
      next: data => {
        this.portfolioHistory = data;
        this.buildChart(data);
        this.isLoading = false;
        this.loadTransactionValuations();
      },
      error: err => {
        this.errorMessage = 'Nie udało się pobrać historii portfela.';
        console.error('Nie udało się pobrać historii portfela', err);
        this.isLoading = false;
      }
    });
  }

  private loadTransactionValuations(): void {
    this.transactionValuationsLoading = true;
    this.transactionErrorMessage = '';

    this.investmentService.getInvestments().subscribe({
      next: investments => {
        const symbols = investments.map(investment => investment.symbol);

        this.cryptoService.getPrices(symbols).subscribe({
          next: prices => {
            this.transactionValuations = investments
              .map(investment => this.mapTransactionValuation(investment, prices[investment.symbol.toLowerCase()] ?? 0))
              .sort((a, b) => new Date(a.buyDate).getTime() - new Date(b.buyDate).getTime() || a.id - b.id);
            this.transactionValuationsLoading = false;
          },
          error: err => {
            this.transactionErrorMessage = 'Nie udało się pobrać aktualnych cen dla tabeli wpłat.';
            console.error('Nie udało się pobrać aktualnych cen dla tabeli wpłat', err);
            this.transactionValuationsLoading = false;
          }
        });
      },
      error: err => {
        this.transactionErrorMessage = 'Nie udało się pobrać wpłat dla tabeli.';
        console.error('Nie udało się pobrać wpłat dla tabeli', err);
        this.transactionValuationsLoading = false;
      }
    });
  }

  private mapTransactionValuation(investment: Investment, currentPrice: number): TransactionValuation {
    const invested = investment.amount * investment.buyPrice;
    const marketValue = investment.amount * currentPrice;
    const profit = marketValue - invested;

    return {
      id: investment.id,
      symbol: investment.symbol,
      type: investment.type ?? TransactionType.Buy,
      amount: investment.amount,
      buyPrice: investment.buyPrice,
      buyDate: investment.buyDate,
      currentPrice,
      invested,
      marketValue,
      profit,
      profitPercent: invested > 0 ? (profit / invested) * 100 : 0
    };
  }

  private buildChart(history: PortfolioHistory): void {
    // Wykres pokazuje dzienną wartość portfela oraz kumulowany zainwestowany kapitał
    this.chartData = {
      labels: history.history.map(point => point.date),
      datasets: [
        {
          label: 'Wartość portfela wg historycznych cen',
          data: history.history.map(point => point.marketValue),
          borderColor: 'rgba(75, 192, 192, 1)',
          backgroundColor: 'rgba(75, 192, 192, 0.2)',
          tension: 0.3
        },
        {
          label: 'Zainwestowany kapitał',
          data: history.history.map(point => point.totalInvested),
          borderColor: 'rgba(54, 162, 235, 1)',
          backgroundColor: 'rgba(54, 162, 235, 0.2)',
          tension: 0.3
        },
        {
          label: 'Dni wpłat',
          data: history.history.map(point => point.isPurchaseDate ? point.totalInvested : null),
          borderColor: 'rgba(255, 159, 64, 1)',
          backgroundColor: 'rgba(255, 159, 64, 1)',
          pointRadius: 6,
          pointHoverRadius: 8,
          showLine: false
        }
      ]
    };
  }

  get totalProfit(): number {
    return this.portfolioHistory?.profit ?? 0;
  }

  get isProfit(): boolean {
    return this.totalProfit >= 0;
  }
}


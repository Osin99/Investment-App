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
  category: number;
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
  filteredTransactionValuations: TransactionValuation[] = [];
  isLoading = true;
  transactionValuationsLoading = false;
  errorMessage = '';
  transactionErrorMessage = '';
  
  selectedCategory: number | null = null;
  currentPage = 1;
  itemsPerPage = 10;
  totalPages = 1;

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
    
    // Inicjalizuj filtry domyślnie
    this.updateFilteredData();
  }

   private loadTransactionValuations(): void {
     this.transactionValuationsLoading = true;
     this.transactionErrorMessage = '';

     this.investmentService.getInvestments().subscribe({
       next: investments => {
         // Oddziel symbole na krypto i akcje/ETF
         const cryptoSymbols = investments
           .filter(inv => this.isCrypto(inv.symbol))
           .map(inv => inv.symbol);
         
         const stockSymbols = investments
           .filter(inv => !this.isCrypto(inv.symbol))
           .map(inv => inv.symbol);

         // Pobierz ceny dla obu typów
         let cryptoPrices: { [key: string]: number } = {};
         let stockPrices: { [key: string]: number } = {};
         let completedRequests = 0;

         const onComplete = () => {
           completedRequests++;
           if (completedRequests === 2) {
             // Połącz ceny z obu źródeł
             const allPrices = { ...cryptoPrices, ...stockPrices };
             
             this.transactionValuations = investments
               .map(investment => this.mapTransactionValuation(investment, allPrices[investment.symbol.toUpperCase()] ?? 0))
               .sort((a, b) => new Date(a.buyDate).getTime() - new Date(b.buyDate).getTime() || a.id - b.id);
             
             this.transactionValuationsLoading = false;
             this.updateFilteredData();
             this.updateChart(); // Odśwież wykres z aktualnymi cenami
           }
         };

         // Pobierz ceny kryptowalut
         if (cryptoSymbols.length > 0) {
           this.cryptoService.getCryptoPrices(cryptoSymbols).subscribe({
             next: prices => {
               cryptoPrices = prices;
               onComplete();
             },
             error: err => {
               console.warn('[Portfolio] Błąd pobierania cen krypto:', err);
               onComplete();
             }
           });
         } else {
           completedRequests++;
         }

         // Pobierz ceny akcji/ETF
         if (stockSymbols.length > 0) {
           this.cryptoService.getStockPrices(stockSymbols).subscribe({
             next: prices => {
               stockPrices = prices;
               onComplete();
             },
             error: err => {
               console.warn('[Portfolio] Błąd pobierania cen akcji/ETF:', err);
               onComplete();
             }
           });
         } else {
           completedRequests++;
         }

         // Jeśli nie ma żadnych symboli
         if (cryptoSymbols.length === 0 && stockSymbols.length === 0) {
           this.transactionValuationsLoading = false;
         }
       },
       error: err => {
         this.transactionErrorMessage = 'Nie udało się pobrać wpłat dla tabeli.';
         console.error('Nie udało się pobrać wpłat dla tabeli', err);
         this.transactionValuationsLoading = false;
       }
     });
   }

   /**
    * Sprawdza czy symbol to kryptowaluta
    */
   private isCrypto(symbol: string): boolean {
     const cryptoSymbols = ['BTC', 'ETH', 'SOL', 'DOGE', 'SHIB', 'XRP', 'ADA', 'LINK', 'USDT'];
     return cryptoSymbols.includes(symbol.toUpperCase());
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
      profitPercent: invested > 0 ? (profit / invested) * 100 : 0,
      category: 0 // Domyślnie Crypto - będzie mapowane z backendu
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

  get dca(): number {
    const filtered = this.getFilteredTransactions();
    if (filtered.length === 0) return 0;
    
    const totalAmount = filtered.reduce((sum, t) => sum + t.amount, 0);
    const totalInvested = filtered.reduce((sum, t) => sum + t.invested, 0);
    
    if (totalAmount === 0) return 0;
    return totalInvested / totalAmount;
  }

  get maxDrawdown(): number {
    if (!this.portfolioHistory || this.portfolioHistory.history.length === 0) return 0;
    
    const filtered = this.getFilteredTransactions();
    if (filtered.length === 0) return 0;
    
    let maxValue = 0;
    let maxDD = 0;
    
    for (const point of this.portfolioHistory.history) {
      if (point.marketValue > maxValue) {
        maxValue = point.marketValue;
      }
      const drawdown = ((point.marketValue - maxValue) / maxValue) * 100;
      if (drawdown < maxDD) {
        maxDD = drawdown;
      }
    }
    
    return maxDD;
  }

  get investmentDays(): number {
    const filtered = this.getFilteredTransactions();
    if (filtered.length === 0) return 0;
    
    const sortedByDate = [...filtered].sort(
      (a, b) => new Date(a.buyDate).getTime() - new Date(b.buyDate).getTime()
    );
    
    const firstDate = new Date(sortedByDate[0].buyDate);
    const lastDate = new Date(sortedByDate[sortedByDate.length - 1].buyDate);
    
    return Math.floor((lastDate.getTime() - firstDate.getTime()) / (1000 * 60 * 60 * 24));
  }

  get roi(): number {
    const filtered = this.getFilteredTransactions();
    if (filtered.length === 0) return 0;
    
    const totalInvested = filtered.reduce((sum, t) => sum + t.invested, 0);
    const totalMarketValue = filtered.reduce((sum, t) => sum + t.marketValue, 0);
    const profit = totalMarketValue - totalInvested;
    
    if (totalInvested === 0) return 0;
    return (profit / totalInvested) * 100;
  }

  private getFilteredTransactions(): TransactionValuation[] {
    if (this.selectedCategory === null) {
      return this.transactionValuations;
    }
    return this.transactionValuations.filter(t => t.category === this.selectedCategory);
  }

  get isProfit(): boolean {
    return this.totalProfit >= 0;
  }

   selectCategory(category: number | null): void {
     this.selectedCategory = category;
     this.currentPage = 1;
     this.updateFilteredData();
     this.updateChart();
   }

   private updateChart(): void {
     if (!this.portfolioHistory) return;
     
     // Jeśli nie ma wybranej kategorii, pokaż cały portfel
     if (this.selectedCategory === null) {
       this.buildChart(this.portfolioHistory);
       return;
     }
     
     const filtered = this.getFilteredTransactions();
     
     if (filtered.length === 0) {
       // Jeśli brak danych dla kategorii, pokaż pusty wykres
       this.chartData = {
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
           }
         ]
       };
       return;
     }
     
     // Filtruj historię portfela na podstawie wybranych transakcji
     const filteredDates = new Set(filtered.map(t => t.buyDate));
     const filteredHistory = this.portfolioHistory.history.filter(point => {
       // Pokaż punkty od pierwszej transakcji w kategorii
       const firstDate = new Date(filtered[0].buyDate);
       const pointDate = new Date(point.date);
       return pointDate >= firstDate;
     });
     
     this.chartData = {
       labels: filteredHistory.map(point => point.date),
       datasets: [
         {
           label: 'Wartość portfela wg historycznych cen',
           data: filteredHistory.map(point => point.marketValue),
           borderColor: 'rgba(75, 192, 192, 1)',
           backgroundColor: 'rgba(75, 192, 192, 0.2)',
           tension: 0.3
         },
         {
           label: 'Zainwestowany kapitał',
           data: filteredHistory.map(point => point.totalInvested),
           borderColor: 'rgba(54, 162, 235, 1)',
           backgroundColor: 'rgba(54, 162, 235, 0.2)',
           tension: 0.3
         }
       ]
     };
   }

  private updateFilteredData(): void {
    if (this.selectedCategory === null) {
      this.filteredTransactionValuations = [...this.transactionValuations];
    } else {
      this.filteredTransactionValuations = this.transactionValuations.filter(
        t => t.category === this.selectedCategory
      );
    }
    this.totalPages = Math.ceil(this.filteredTransactionValuations.length / this.itemsPerPage);
  }

  get paginatedData(): TransactionValuation[] {
    const start = (this.currentPage - 1) * this.itemsPerPage;
    return this.filteredTransactionValuations.slice(start, start + this.itemsPerPage);
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
    }
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
    }
  }
}


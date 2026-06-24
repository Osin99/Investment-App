import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgIf, NgFor, DecimalPipe } from '@angular/common';
import { Investment, InvestmentService, TransactionType } from '../services/investment.service';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { PriceService } from '../services/price.service';
import { CryptoService } from '../services/crypto.service';
import { CATEGORY_NAMES } from '../models/asset-category';
import { SecuritySearchComponent } from './security-search.component';
import { SecuritySearchResult } from '../services/security-search.service';

@Component({
  selector: 'app-investment-form',
  standalone: true,
  templateUrl: './investment-form.component.html',
  styleUrls: ['./investment-form.component.css'],
  imports: [FormsModule, NgIf, NgFor, RouterModule, SecuritySearchComponent, DecimalPipe]
})
export class InvestmentFormComponent implements OnInit {
  form: Investment & { category?: number; unit?: string } = {
    id: 0,
    symbol: '',
    type: TransactionType.Buy,
    amount: 0,
    buyPrice: 0,
    buyDate: '',
    category: 0,
    unit: undefined
  };

  readonly transactionTypes = [
    { value: TransactionType.Buy, label: 'Zakup' },
    { value: TransactionType.Sell, label: 'Sprzedaż' }
  ];

  errorMessage = '';
  isSaving = false;
  goldUnits = ['g', 'oz'];
  selectedGoldUnit = 'g';
  isLoadingPrice = false;
  currentPrice: number | null = null;

  constructor(
    private investmentService: InvestmentService,
    private priceService: PriceService,
    private cryptoService: CryptoService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // Pobierz kategorię z query params
    this.route.queryParams.subscribe(params => {
      if (params['category'] !== undefined) {
        this.form.category = parseInt(params['category'], 10);
      }
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.investmentService.getInvestmentById(+id).subscribe({
        next: data => {
          this.form = { ...data, category: data.category || 0 };
        },
        error: err => {
          this.errorMessage = 'Nie udało się załadować inwestycji.';
          console.error('Nie udało się załadować inwestycji', err);
        }
      });
    }
  }

  getCategorySubtitle(): string {
    const subtitles: { [key: number]: string } = {
      0: `Dodaj zakup kryptowaluty, aby aktualizować portfel i wykresy.`,
      1: `Dodaj ETF, aby śledzić Twoje inwestycje w fundusze.`,
      2: `Dodaj akcje, aby śledzić Twoje inwestycje w spółki.`,
      3: `Dodaj złoto, aby śledzić Twoje inwestycje w metale szlachetne.`
    };
    return subtitles[this.form.category || 0] || subtitles[0];
  }

  getSymbolLabel(): string {
    const labels: { [key: number]: string } = {
      0: 'Symbol kryptowaluty',
      1: 'Ticker ETF',
      2: 'Ticker akcji',
      3: 'Typ złota (np. XAU)'
    };
    return labels[this.form.category || 0] || labels[0];
  }

  getSymbolPlaceholder(): string {
    const placeholders: { [key: number]: string } = {
      0: 'np. BTC, ETH',
      1: 'np. VWRL, CSPX',
      2: 'np. AAPL, MSFT',
      3: 'np. XAU'
    };
    return placeholders[this.form.category || 0] || placeholders[0];
  }

   onCategoryChange(): void {
     // Resetuj pola gdy zmieni się kategoria
     this.form.symbol = '';
     this.form.amount = 0;
     this.form.buyPrice = 0;
     this.form.unit = undefined;
   }

   onSecuritySelected(security: SecuritySearchResult): void {
     this.form.symbol = security.symbol;
     this.loadCurrentPrice();
   }

   private loadCurrentPrice(): void {
     if (!this.form.symbol) {
       this.currentPrice = null;
       return;
     }

     this.isLoadingPrice = true;
     const symbol = this.form.symbol.toUpperCase();

     // Określ typ na podstawie kategorii
     let type: 'crypto' | 'stocks' = 'crypto';
     if (this.form.category === 1 || this.form.category === 2) {
       type = 'stocks';
     }

     this.cryptoService.getPrices([symbol], type).subscribe({
       next: prices => {
         this.currentPrice = prices[symbol] || null;
         if (this.currentPrice) {
           this.form.buyPrice = this.currentPrice;
         }
         this.isLoadingPrice = false;
       },
       error: err => {
         console.warn('Błąd pobierania ceny:', err);
         this.isLoadingPrice = false;
       }
     });
   }

  submitForm(): void {
    this.errorMessage = '';
    this.isSaving = true;
    this.form.symbol = this.form.symbol.trim().toUpperCase();

    // Ustaw unit dla złota
    if (this.form.category === 3) {
      this.form.unit = this.selectedGoldUnit;
    }

    if (this.form.id && this.form.id > 0) {
      this.investmentService.updateInvestment(this.form.id, this.form).subscribe({
        next: () => this.router.navigate(['/']),
        error: err => this.handleSaveError(err)
      });
    } else {
      this.investmentService.addInvestment(this.form).subscribe({
        next: () => this.router.navigate(['/']),
        error: err => this.handleSaveError(err)
      });
    }
  }

  private handleSaveError(err: unknown): void {
    this.errorMessage = 'Nie udało się zapisać inwestycji.';
    this.isSaving = false;
    console.error('Błąd zapisywania inwestycji', err);
  }
}

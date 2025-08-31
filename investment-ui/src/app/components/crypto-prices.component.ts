import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CryptoService } from '../services/crypto.service';

@Component({
  selector: 'app-crypto-prices',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div>
      <h5>💸 Ceny kryptowalut</h5>
      <ul class="list-group">
        <li 
          class="list-group-item d-flex justify-content-between align-items-center"
          *ngFor="let key of getKeys(prices)"
        >
          {{ key.toUpperCase() }}
          <span class="badge bg-success">{{ prices[key]?.usd }} USD</span>
        </li>
      </ul>
    </div>
  `
})
export class CryptoPricesComponent implements OnInit {
  // Zmienna do przechowywania danych z API
  prices: any = {};

  // Wstrzykujemy serwis pobierający dane z CoinGecko
  constructor(private cryptoService: CryptoService) {}

  // Po załadowaniu komponentu pobieramy dane
  ngOnInit(): void {
    this.cryptoService.getPrices().subscribe({
      next: (data: any) => {
        this.prices = data;
      },
      error: (err: any) => {
        console.error('Błąd pobierania cen:', err);
      }
    });
  }

  // Zwraca listę kluczy (np. 'bitcoin', 'ethereum') do użycia w *ngFor
  getKeys(obj: any): string[] {
    return Object.keys(obj);
  }
}

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
          *ngFor="let symbol of priceKeys"
        >
          {{ symbol.toUpperCase() }}
          <span class="badge bg-success">{{ prices[symbol] | currency:'PLN':'symbol':'1.2-2' }}</span>
        </li>
      </ul>
    </div>
  `
})
export class CryptoPricesComponent implements OnInit {
  prices: { [symbol: string]: number } = {};
  priceKeys: string[] = [];

  constructor(private cryptoService: CryptoService) {}

  ngOnInit(): void {
    this.cryptoService.getPrices().subscribe(data => {
      this.prices = data;
      this.priceKeys = Object.keys(data);
    });
  }
}

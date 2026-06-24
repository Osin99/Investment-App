import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';

interface PriceUpdateStatus {
  lastUpdatedAt: string | null;
  isOnCooldown: boolean;
  nextAvailableAt: string | null;
  remainingSeconds: number;
}

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.css']
})
export class AdminComponent implements OnInit, OnDestroy {
  isLoadingSecurities = false;
  isLoadingPrices = false;
  message: string | null = null;
  messageType: 'success' | 'error' | null = null;

  priceStatus: PriceUpdateStatus = {
    lastUpdatedAt: null,
    isOnCooldown: false,
    nextAvailableAt: null,
    remainingSeconds: 0
  };

  private statusInterval: any;
  private countdownInterval: any;
  remainingDisplay = '';

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loadPriceStatus();
    // Odświeżaj status co 30 sekund
    this.statusInterval = setInterval(() => this.loadPriceStatus(), 30000);
  }

  ngOnDestroy(): void {
    if (this.statusInterval) clearInterval(this.statusInterval);
    if (this.countdownInterval) clearInterval(this.countdownInterval);
  }

  loadPriceStatus(): void {
    this.http.get<PriceUpdateStatus>('http://localhost:5247/api/admin/price-update-status').subscribe({
      next: (status) => {
        this.priceStatus = status;
        this.startCountdown();
      },
      error: () => {}
    });
  }

  startCountdown(): void {
    if (this.countdownInterval) clearInterval(this.countdownInterval);

    if (!this.priceStatus.isOnCooldown) {
      this.remainingDisplay = '';
      return;
    }

    let remaining = this.priceStatus.remainingSeconds;
    this.updateRemainingDisplay(remaining);

    this.countdownInterval = setInterval(() => {
      remaining--;
      if (remaining <= 0) {
        clearInterval(this.countdownInterval);
        this.priceStatus.isOnCooldown = false;
        this.remainingDisplay = '';
        this.loadPriceStatus();
      } else {
        this.updateRemainingDisplay(remaining);
      }
    }, 1000);
  }

  updateRemainingDisplay(seconds: number): void {
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    this.remainingDisplay = `${m}m ${s}s`;
  }

  updateSecurities(): void {
    if (this.isLoadingSecurities) return;

    this.isLoadingSecurities = true;
    this.message = null;
    this.messageType = null;

    this.http.post('http://localhost:5247/api/admin/update-securities', {}).subscribe({
      next: (response: any) => {
        this.isLoadingSecurities = false;
        this.message = response.message || 'Papierów wartościowych zostały zaktualizowane pomyślnie';
        this.messageType = 'success';
        this.autoHideMessage();
      },
      error: (error) => {
        this.isLoadingSecurities = false;
        this.message = error.error?.error || 'Błąd podczas aktualizacji papierów wartościowych';
        this.messageType = 'error';
      }
    });
  }

  updatePrices(): void {
    if (this.isLoadingPrices || this.priceStatus.isOnCooldown) return;

    this.isLoadingPrices = true;
    this.message = null;
    this.messageType = null;

    this.http.post<any>('http://localhost:5247/api/admin/update-prices', {}).subscribe({
      next: (response) => {
        this.isLoadingPrices = false;
        this.message = response.message || 'Ceny zostały zaktualizowane';
        this.messageType = response.status === 'SUCCESS' ? 'success' : 'error';
        this.loadPriceStatus();
        this.autoHideMessage();
      },
      error: (error) => {
        this.isLoadingPrices = false;
        if (error.status === 400 && error.error?.nextAvailableAt) {
          this.message = `Cooldown aktywny. Następna aktualizacja możliwa o ${error.error.nextAvailableAt}`;
          this.messageType = 'error';
          this.loadPriceStatus();
        } else {
          this.message = error.error?.error || 'Błąd podczas aktualizacji cen';
          this.messageType = 'error';
        }
      }
    });
  }

  private autoHideMessage(): void {
    setTimeout(() => {
      this.message = null;
      this.messageType = null;
    }, 6000);
  }
}

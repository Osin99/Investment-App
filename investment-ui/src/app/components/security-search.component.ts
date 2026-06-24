import { Component, Output, EventEmitter, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SecuritySearchService, SecuritySearchResult } from '../services/security-search.service';

@Component({
  selector: 'app-security-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './security-search.component.html',
  styleUrls: ['./security-search.component.css']
})
export class SecuritySearchComponent implements OnInit {
  @Output() securitySelected = new EventEmitter<SecuritySearchResult>();
  @Input() categoryFilter?: number; // 0=Krypto, 1=ETF, 2=Akcje, 3=Złoto

  searchQuery: string = '';
  searchResults: SecuritySearchResult[] = [];
  isLoading: boolean = false;
  isDropdownOpen: boolean = false;
  selectedIndex: number = -1;
  selectedSecurity: SecuritySearchResult | null = null;

  constructor(private securitySearchService: SecuritySearchService) {}

  ngOnInit(): void {}

    onSearchInput(): void {
      this.selectedIndex = -1;
      
      if (!this.searchQuery || this.searchQuery.trim().length === 0) {
        this.searchResults = [];
        this.isDropdownOpen = false;
        return;
      }

      this.isLoading = true;
      this.isDropdownOpen = true;

      // Debounce search
      setTimeout(() => {
        this.securitySearchService.searchSecurities(this.searchQuery, 20).subscribe({
          next: (results) => {
            // Zwróć wszystkie wyniki bez filtrowania po kategorii
            this.searchResults = results;
            this.isLoading = false;
            this.isDropdownOpen = this.searchResults.length > 0;
          },
          error: (err) => {
            console.error('Search error:', err);
            this.searchResults = [];
            this.isLoading = false;
            this.isDropdownOpen = false;
          }
        });
      }, 300);
    }

  selectSecurity(security: SecuritySearchResult): void {
    this.selectedSecurity = security;
    this.searchQuery = security.symbol;
    this.searchResults = [];
    this.isDropdownOpen = false;
    this.securitySelected.emit(security);
  }

  closeDropdown(): void {
    this.isDropdownOpen = false;
  }

  getSecurityTypeLabel(type: string): string {
    const labels: { [key: string]: string } = {
      'Stock': 'Akcja',
      'ETF': 'ETF',
      'Crypto': 'Kryptowaluta',
      'Other': 'Inne'
    };
    return labels[type] || type;
  }

  getSecurityTypeColor(type: string): string {
    const colors: { [key: string]: string } = {
      'Stock': '#60a5fa',
      'ETF': '#4ade80',
      'Crypto': '#a78bfa',
      'Other': '#fbbf24'
    };
    return colors[type] || '#94a3b8';
  }
}

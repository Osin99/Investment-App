import { Component, OnInit } from "@angular/core";
import { InvestmentService, Investment } from "../services/investment.service";
import { DatePipe, NgFor, DecimalPipe, NgIf } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-investment-list',
  standalone: true,
  templateUrl: './investment-list.component.html',
  styleUrls: ['./investment-list.component.css'],
  imports: [DatePipe, NgFor, DecimalPipe, NgIf]
})
export class InvestmentListComponent implements OnInit {
  investments: Investment[] = [];
  isLoading = true;
  errorMessage = '';

  constructor(
    private investmentService: InvestmentService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.investmentService.getInvestments().subscribe({
      next: data => {
        this.investments = data;
        this.isLoading = false;
      },
      error: err => {
        this.errorMessage = 'Nie udało się pobrać inwestycji.';
        console.error('Błąd pobierania inwestycji', err);
        this.isLoading = false;
      }
    });
  }

  goToForm(): void {
    this.router.navigate(['/dodaj']);
  }

  deleteInvestment(id: number): void {
    if (!confirm('Czy na pewno chcesz usunąć tę inwestycję?')) {
      return;
    }

    this.investmentService.deleteInvestment(id).subscribe({
      next: () => {
        this.investments = this.investments.filter(i => i.id !== id);
      },
      error: err => console.error('Błąd usuwania', err)
    });
  }

  editInvestment(id: number): void {
    this.router.navigate(['/edytuj', id]);
  }
}

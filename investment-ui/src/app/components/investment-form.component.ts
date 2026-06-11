import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgIf } from '@angular/common';
import { Investment, InvestmentService } from '../services/investment.service';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-investment-form',
  standalone: true,
  templateUrl: './investment-form.component.html',
  styleUrls: ['./investment-form.component.css'],
  imports: [FormsModule, NgIf, RouterModule]
})
export class InvestmentFormComponent implements OnInit {
  form: Investment = {
    id: 0,
    symbol: '',
    amount: 0,
    buyPrice: 0,
    buyDate: ''
  };

  errorMessage = '';
  isSaving = false;

  constructor(
    private investmentService: InvestmentService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.investmentService.getInvestmentById(+id).subscribe({
        next: data => this.form = data,
        error: err => {
          this.errorMessage = 'Nie udało się załadować inwestycji.';
          console.error('Nie udało się załadować inwestycji', err);
        }
      });
    }
  }

  submitForm(): void {
    this.errorMessage = '';
    this.isSaving = true;

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

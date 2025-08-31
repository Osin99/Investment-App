import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Investment, InvestmentService } from '../services/investment.service';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-investment-form',
  standalone: true,
  templateUrl: './investment-form.component.html',
  styleUrls: ['./investment-form.component.css'],
  imports: [FormsModule, RouterModule]
})
export class InvestmentFormComponent implements OnInit {

  // 🧱 Obiekt formularza – zawiera dane inwestycji (dla dodawania i edycji)
  form: Investment = {
    id: 0,             // ← Jeśli 0 → dodajemy nową; jeśli > 0 → edytujemy istniejącą
    symbol: '',
    amount: 0,
    buyPrice: 0,
    buyDate: ''
  };

  constructor(
    private investmentService: InvestmentService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  // 🚀 Wczytywanie danych do edycji (jeśli w adresie URL jest id)
  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.investmentService.getInvestmentById(+id).subscribe({
        next: (data) => this.form = data,
        error: (err) => console.error("Nie udało się załadować inwestycji", err)
      });
    }
  }

  // 📤 Zapisz formularz – dodaj nową inwestycję lub zaktualizuj istniejącą
  submitForm() {
    if (this.form.id && this.form.id > 0) {
      // ✏️ Aktualizacja inwestycji
      this.investmentService.updateInvestment(this.form.id, this.form).subscribe(() => {
        this.router.navigate(['/']);
      });
    } else {
      // ➕ Dodawanie nowej inwestycji
      this.investmentService.addInvestment(this.form).subscribe(() => {
        this.router.navigate(['/']);
      });
    }
  }
}

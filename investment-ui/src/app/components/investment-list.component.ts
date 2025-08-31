import { Component, OnInit } from "@angular/core";
import { InvestmentService, Investment } from "../services/investment.service";
import { DatePipe, NgFor, NgIf, DecimalPipe } from '@angular/common';
import { Router } from '@angular/router';




// Dekorator @Component opisuje konfigurację komponentu
@Component({
  selector: 'app-investment-list',//Nazwa tagu HTML do użycia, np. <app-investment-list>
  standalone: true,// To jest komponent bezmodułowy
  templateUrl: './investment-list.component.html',// Szablon HTML
  styleUrls: ['./investment-list.component.css'],// Styl
  imports: [DatePipe, NgFor, DecimalPipe] // 👉 dodajemy DatePipe i inne używane dyrektywy
})
export class InvestmentListComponent implements OnInit{
  

    // Tablica, która będzie przechowywać inwestycje pobrane z backendu
    investments: Investment[]=[];

    // Konstruktor – Angular wstrzykuje nasz serwis do zmiennej `investmentService`
   constructor(
    private investmentService: InvestmentService,
    private router: Router
  ) {}


    // ngOnInit() to metoda, która wykonuje się automatycznie po załadowaniu komponentu
    ngOnInit(): void {
      // Wywołujemy metodę z serwisu, która zwraca Observable
      this.investmentService.getInvestments().subscribe({
        next:(data) => this.investments = data,// Gdy przyjdą dane – przypisz je do zmiennej
        error: (err) => console.error("Błąd pobierania inwestycji",err)// Obsługa błędu+
      });
    }
      goToForm() {
    this.router.navigate(['/dodaj']);
  }
  deleteInvestment(id: number): void{
    if(!confirm("Czy na pewno chcesz usunąć tę inwestycję?"))return;
    this.investmentService.deleteInvestment(id).subscribe({
      next:()=>{
        this.investments = this.investments.filter(i => i.id !==id)
      },
      error: err => console.error("Błąd usuwania", err)
    });
  }
  editInvestment(id: number){
    this.router.navigate(['/edytuj', id])// przekierowanie z ID inwestycji
  }
}

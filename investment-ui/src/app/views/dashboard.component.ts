import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';

import { InvestmentListComponent } from '../components/investment-list.component';
import { InvestmentSummaryComponent } from '../components/investment-summary/investment-summary.component';
import { InvestmentChartComponent } from '../components/investment-chart/investment-chart.component';
import { InvestmentSummaryBoxComponent } from '../components/investment-summary-box/investment-summary-box.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  templateUrl: './dashboard.component.html',
  imports: [
    CommonModule,
    RouterModule,
    InvestmentListComponent,
    InvestmentSummaryComponent,
    InvestmentChartComponent,
    InvestmentSummaryBoxComponent
  ]
})
export class DashboardComponent {
  lastUpdateTime = '';

  constructor(private router: Router) {}

  ngOnInit(): void {
    this.lastUpdateTime = new Date().toLocaleTimeString('pl-PL');
  }

  goToForm(): void {
    this.router.navigate(['/dodaj']);
  }
}

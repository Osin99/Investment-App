import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';

import { InvestmentListComponent } from '../components/investment-list.component';
import { InvestmentSummaryComponent } from '../components/investment-summary/investment-summary.component';
import { InvestmentChartComponent } from '../components/investment-chart/investment-chart.component';
import { InvestmentSummaryBoxComponent } from '../components/investment-summary-box/investment-summary-box.component';
import { AssetCategory, CATEGORY_NAMES } from '../models/asset-category';

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
export class DashboardComponent implements OnInit {
  lastUpdateTime = '';
  selectedCategory: AssetCategory | null = null;
  categoryName = '';

  constructor(
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.lastUpdateTime = new Date().toLocaleTimeString('pl-PL');
    
    // Pobierz kategorię z route data
    this.route.data.subscribe(data => {
      if (data['category'] !== undefined) {
        this.selectedCategory = data['category'] as AssetCategory;
        this.categoryName = CATEGORY_NAMES[this.selectedCategory];
      } else {
        this.selectedCategory = null;
        this.categoryName = '';
      }
    });
  }

}

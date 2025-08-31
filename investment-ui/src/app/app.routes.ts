import { Routes } from '@angular/router';
import { InvestmentListComponent } from './components/investment-list.component';
import { InvestmentFormComponent } from './components/investment-form.component';
import { DashboardComponent } from './views/dashboard.component';

export const routes: Routes = [
  { path: '', component: DashboardComponent },
  {
    path: '',
    component: InvestmentListComponent
  },
  {
    path: 'dodaj',
     loadComponent: () =>
    import('./components/investment-form.component').then(m => m.InvestmentFormComponent)
  },
  {
    path: 'edytuj/:id',
    loadComponent: () =>
    import('./components/investment-form.component').then(m => m.InvestmentFormComponent)
  }
];

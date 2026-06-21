import { Routes } from '@angular/router';
import { InvestmentListComponent } from './components/investment-list.component';
import { InvestmentFormComponent } from './components/investment-form.component';
import { DashboardComponent } from './views/dashboard.component';
import { PortfolioHistoryComponent } from './views/portfolio-history.component';
import { LoginComponent } from './views/login.component';
import { AuthGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: '', component: DashboardComponent, canActivate: [AuthGuard] },
  { path: 'lista', component: InvestmentListComponent, canActivate: [AuthGuard] },
  { path: 'historia', component: PortfolioHistoryComponent, canActivate: [AuthGuard] },
  {
    path: 'dodaj',
    loadComponent: () =>
      import('./components/investment-form.component').then(m => m.InvestmentFormComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'edytuj/:id',
    loadComponent: () =>
      import('./components/investment-form.component').then(m => m.InvestmentFormComponent),
    canActivate: [AuthGuard]
  }
];

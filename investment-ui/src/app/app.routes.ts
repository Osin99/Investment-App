import { Routes } from '@angular/router';
import { InvestmentListComponent } from './components/investment-list.component';
import { InvestmentFormComponent } from './components/investment-form.component';
import { DashboardComponent } from './views/dashboard.component';
import { PortfolioHistoryComponent } from './views/portfolio-history.component';
import { LoginComponent } from './views/login.component';
import { AdminComponent } from './views/admin.component';
import { AuthGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: '', component: DashboardComponent, canActivate: [AuthGuard] },
  { path: 'crypto', component: DashboardComponent, canActivate: [AuthGuard], data: { category: 0 } },
  { path: 'etf', component: DashboardComponent, canActivate: [AuthGuard], data: { category: 1 } },
  { path: 'stocks', component: DashboardComponent, canActivate: [AuthGuard], data: { category: 2 } },
  { path: 'other', component: DashboardComponent, canActivate: [AuthGuard], data: { category: 3 } },
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
  },
  { path: 'ustawienia', component: DashboardComponent, canActivate: [AuthGuard] },
  { path: 'admin', component: AdminComponent, canActivate: [AuthGuard] }
];

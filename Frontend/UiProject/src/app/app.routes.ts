import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { HistoricalChartComponent } from './components/historical-chart/historical-chart.component';
import { UserProfileComponent } from './components/user-profile/user-profile.component';
import { SavingsPlanListComponent } from './components/savings-plan-list/savings-plan-list.component';
import { SavingsPlanDetailComponent } from './components/savings-plan-detail/savings-plan-detail.component';
import { TransactionListComponent } from './components/transaction-list/transaction-list.component';
import { PlanClosureComponent } from './components/plan-closure/plan-closure.component';
import { AddSavingPlanComponent } from './components/add-saving-plan/add-saving-plan.component';
import { KpiSummaryComponent } from './components/kpi-summary/kpi-summary.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'charts', component: HistoricalChartComponent},
  
  { path: 'profile', component: UserProfileComponent},
  { path: 'saving-plans', component: SavingsPlanListComponent},
  { path: 'savings-plan-detail/:id', component: SavingsPlanDetailComponent},
  { path: 'transactions', component: TransactionListComponent },
  { path: 'plan-closure/:id', component: PlanClosureComponent},
  { path: 'add-new-plan', component: AddSavingPlanComponent},
  { path: 'summary', component: KpiSummaryComponent},
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' }
];
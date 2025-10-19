import { ChangeDetectorRef, Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { KpiSummaryComponent } from "../kpi-summary/kpi-summary.component";
import { SavingsPlanService } from '../../services/savings-plan';

interface SavingsPlan {
  id: string;
  planType: string;
  monthlyAmount: number;
  startDate: string;
  endDate: string | null;
  isActive: boolean;
  createdDate: string;
}

@Component({
  selector: 'app-savings-plan-list',
  standalone: true,
  imports: [CommonModule, TableModule, ButtonModule, CardModule, TagModule, KpiSummaryComponent, KpiSummaryComponent],
  templateUrl: './savings-plan-list.component.html',
  host: { class: 'flex justify-content-center align-items-center min-h-screen bg-gray-100' },
})
export class SavingsPlanListComponent implements OnInit {
  status: 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast' | null = 'success';
  plans: SavingsPlan[] = [];
  loading = false;
  apiUrl = 'http://localhost:8080/api';

  constructor(
    private savingsPlanService: SavingsPlanService,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: object,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.loadPlans();
    }
  }

  loadPlans() {
    const token = localStorage.getItem('jwt_token');
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    this.loading = true;
    this.savingsPlanService.getUserPlans(headers).subscribe({
      next: (data) => {
        this.plans = data;
        this.loading = false;
        console.log('Loaded Plans: ', this.plans);
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Error loading plans:', err);
        this.loading = false;
        this.cdr.markForCheck();
      },
    });
  }

  viewPlanDetails(planId: string) {
    this.router.navigate(['/savings-plan-detail', planId]);
  }

  addNewPlan() {
    this.router.navigate(['/add-new-plan']);
  }

  getSeverity(isActive: boolean): 'success' | 'danger' {
    return isActive ? 'success' : 'danger';
  }

  getStatusLabel(isActive: boolean): string {
    return isActive ? 'Active' : 'Closed';
  }
}

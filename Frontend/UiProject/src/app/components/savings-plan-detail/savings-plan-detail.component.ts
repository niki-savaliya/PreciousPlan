import { ChangeDetectorRef, Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpHeaders } from '@angular/common/http';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TabsModule } from 'primeng/tabs';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { SavingsPlanService } from '../../services/savings-plan';
import { QuarterlyFeesService } from '../../services/quarterly-fees';
import { DepositService } from '../../services/deposit';

interface SavingsPlan {
  id: string;
  planType: string;
  monthlyAmount: number;
  startDate: string;
  endDate: string | null;
  isActive: boolean;
}

interface Deposit {
  id: string;
  depositDate: string;
  amount: number;
}

interface QuarterlyFee {
  id: string;
  feeAmount: number;
  feeDate: string;
}

@Component({
  selector: 'app-savings-plan-detail',
  standalone: true,
  imports: [CommonModule, CardModule, ButtonModule, TabsModule, TableModule, TagModule],
  templateUrl: './savings-plan-detail.component.html',
  styleUrls: ['./savings-plan-detail.component.css'],
  host: { class: 'flex justify-content-center align-items-center min-h-screen bg-gray-100' },
})
export class SavingsPlanDetailComponent implements OnInit {
  planId: string = '';
  plan: SavingsPlan | null = null;
  deposits: Deposit[] = [];
  fees: QuarterlyFee[] = [];
  loading = false;
  private headers: HttpHeaders = new HttpHeaders();

  constructor(
    private savingsPlanService: SavingsPlanService,
    private quarterlyFeesService: QuarterlyFeesService,
    private depositService: DepositService,
    private route: ActivatedRoute,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: object,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.planId = this.route.snapshot.paramMap.get('id') || '';

      if (!this.planId) {
        // If no planId, navigate back to list or safe default route
        this.router.navigate(['/savings-plan-list']);
        return;
      }

      const token = localStorage.getItem('jwt_token');
      if (token) {
        this.headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
      } else {
        this.headers = new HttpHeaders();
      }

      this.loadPlanDetails();
      this.loadDeposits();
      this.loadFees();
    }
  }

  loadPlanDetails() {
    this.loading = true;
    const token = localStorage.getItem('jwt_token');
    if (!token) {
      this.router.navigate(['/login']);
      return;
    }
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    this.savingsPlanService.getPlanDetails(headers, this.planId)
      .subscribe({
        next: (data) => {
          this.plan = data;
          this.loading = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Error loading plan:', err);
          this.loading = false;
          this.cdr.detectChanges();
        },
      });
  }

  loadDeposits() {
    this.depositService.getDepositsForPlan(this.planId, this.headers)
      .subscribe({
        next: (data) => {
          this.deposits = data;
          this.cdr.detectChanges();
        },
        error: (err) => console.error('Error loading deposits:', err),
      });
  }

  loadFees() {
    this.quarterlyFeesService.getFeesForPlan(this.planId, this.headers)
      .subscribe({
        next: (data) => {
          this.fees = data;
          this.cdr.detectChanges();
        },
        error: (err) => console.error('Error loading fees:', err),
      });
  }

  closePlan() {
    this.router.navigate(['/plan-closure', this.planId]);
  }
  backToPlans() {
    this.router.navigate(['/saving-plans']);
  }
}

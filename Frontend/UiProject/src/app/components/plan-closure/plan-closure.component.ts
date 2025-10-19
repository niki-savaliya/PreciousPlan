import { ChangeDetectorRef, Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { SavingsPlanService } from '../../services/savings-plan';
import { TransactionService } from '../../services/transaction';

@Component({
  selector: 'app-plan-closure',
  standalone: true,
  imports: [CommonModule, CardModule, ButtonModule, FormsModule, InputTextModule],
  templateUrl: './plan-closure.component.html',
  host: { class: 'flex justify-content-center align-items-center min-h-screen bg-gray-100' }
})
export class PlanClosureComponent implements OnInit {
  planId = '';
  payoutAmount = 0;
  bankAccountNumber = '';
  loading = false;
  apiUrl = 'http://localhost:8080/api';
  private headers: HttpHeaders = new HttpHeaders();

  constructor(
    private savingsPlanService : SavingsPlanService,
    private transactionService: TransactionService,
    private route: ActivatedRoute,
    private http: HttpClient,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: object,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.planId = this.route.snapshot.paramMap.get('id') || '';

      const token = localStorage.getItem('jwt_token');
      if (token) {
        this.headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
      } else {
        this.headers = new HttpHeaders();
      }
      this.estimatePayout();
    }
  }

  estimatePayout() {
    this.loading = true;
    const token = localStorage.getItem('jwt_token');
    const headers = token
      ? new HttpHeaders().set('Authorization', `Bearer ${token}`)
      : new HttpHeaders();

    this.savingsPlanService.getEstimatedPayout(headers, this.planId)
      .subscribe({
        next: (res) => {
          this.payoutAmount = res.amount;
          this.loading = false;
          this.cdr.detectChanges();
        },
        error: () => (this.loading = false),
      });
  }

  cancelClosure(){
    this.router.navigate(['/saving-plans']);
  }

  confirmClosure() {
    this.loading = true;
    const token = localStorage.getItem('jwt_token');
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    this.transactionService.processPayout(this.planId, headers)
      .subscribe({
        next: () => {
          this.loading = false;
          alert('Plan closed and payout processed.');
          this.router.navigate(['/saving-plans']);
          this.cdr.detectChanges();
        },
        error: () => {
          this.loading = false;
          alert('Error processing payout.');
        },
      });
  }
}

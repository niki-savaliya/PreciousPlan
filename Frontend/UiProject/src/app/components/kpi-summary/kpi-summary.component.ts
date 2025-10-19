import { CommonModule, isPlatformBrowser } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ChangeDetectorRef, Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { CardModule } from 'primeng/card';
import { Tooltip } from "primeng/tooltip";
import { SavingsPlanService } from '../../services/savings-plan';
import { Router } from '@angular/router';

@Component({
  selector: 'app-kpi-summary',
  imports: [CardModule, CommonModule, Tooltip],
  standalone: true,
  templateUrl: './kpi-summary.component.html',
  styleUrl: './kpi-summary.component.css'
})
export class KpiSummaryComponent implements OnInit {
  apiUrl = 'http://localhost:8080/api';
  kpis: any;
  constructor(private http: HttpClient, @Inject(PLATFORM_ID) private platformId: object, private cdr: ChangeDetectorRef, private savingsPlanService: SavingsPlanService, private router: Router) { }

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.getKpis();
    }
  }

  getKpis() {
    const token = localStorage.getItem('jwt_token');
    if (!token) {
      this.router.navigate(['/login']);
      return;
    }
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    this.savingsPlanService.getKpis(headers).subscribe({
      next: (data) => {
        this.kpis = data;
        console.log('Loaded Plans: ', this.kpis);
        this.cdr.markForCheck();
      },
      error: (err) => {
        console.error('Error loading plans:', err);
        this.cdr.markForCheck();
      }
    });
  }
}

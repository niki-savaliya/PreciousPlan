import { HttpHeaders } from '@angular/common/http';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { SavingsPlanService } from '../../services/savings-plan';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';

@Component({
  selector: 'app-add-saving-plan',
  imports: [CommonModule, FormsModule, CardModule, ButtonModule, InputTextModule],
  templateUrl: './add-saving-plan.component.html',
  styleUrl: './add-saving-plan.component.css',
  host: { class: 'flex justify-content-center align-items-center min-h-screen bg-gray-100' }
})
export class AddSavingPlanComponent {
  savingsPlans: any[] = [];
  modalOpen = false;
  confirmDeleteId: string | null = null;
  planType: number = 1;

  formModel = {
    id: '',          // for editing existing plans
    planType: 1,
    monthlyAmount: 0,
    isActive: true
  };

  message: string = '';

  constructor(
    private savingsPlanService: SavingsPlanService,
    private router: Router
  ) { }

  loadPlans() {
    const token = localStorage.getItem('jwt_token');
    if (!token) {
      this.message = 'User session expired. Please log in.';
      this.router.navigate(['/login']);
      return;
    }
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    this.savingsPlanService.getUserPlans(headers).subscribe({
      next: (plans) => {
        console.log('Loaded Plans: ', plans);
        this.savingsPlans = plans;
      },
      error: (err) => {
        this.message = 'Error loading savings plans.';
      },
    });
  }

  cancel() {
    this.router.navigate(['/saving-plans']);
  }

  savePlan() {
    const token = localStorage.getItem('jwt_token');
    if (!token) {
      this.message = 'User session expired. Please log in.';
      this.router.navigate(['/login']);
      return;
    }

    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);

      this.formModel.monthlyAmount <= 0;
      const payload = {
        planType: +this.formModel.planType,
        monthlyAmount: this.formModel.monthlyAmount
      };
      console.log('New Saving Plan to add ', payload);
      // Create new plan
      this.savingsPlanService.create(payload, headers).subscribe({
        next: () => {
          this.modalOpen = false;
          this.loadPlans();
          this.message = 'Plan added successfully!';
          alert("Plan added successfully!");
          this.router.navigate(['/saving-plans'])
        },
        error: () => {
          this.message = 'Failed to add plan.';
          alert(this.message);
        },
      });
  }
}

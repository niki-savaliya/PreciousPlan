import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { FloatLabelModule } from 'primeng/floatlabel';
import { Router } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { DividerModule } from 'primeng/divider';
import {CardModule} from 'primeng/card';

@Component({
  selector: 'app-register',
  standalone: true,
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
  host: { class: 'flex justify-content-center align-items-center min-h-screen bg-gray-100' },
  imports: [
    InputTextModule,
    PasswordModule,
    ButtonModule,
    FormsModule,
    CommonModule,
    FloatLabelModule,
    SelectModule,
    DividerModule,
    CardModule
  ]
})
export class RegisterComponent {
  name: string = '';
  email: string = '';
  bankAccountNumber: string = '';
  password: string = '';
  confirmPassword: string = '';
  role: string = 'User';
  errorMessage: string = '';
  successMessage: string = '';
  roleOptions = [
    { label: 'User', value: 'User' },
    { label: 'Admin', value: 'Admin' }
  ];

  columns = [{ field: 'label' }, { field: 'input' }];

  formRows = [
    { id: 1, field: 'name', label: 'Name', model: this.name, type: 'text', required: true },
    { id: 2, field: 'email', label: 'Email', model: this.email, type: 'text', inputType: 'email', required: true },
    { id: 3, field: 'bankAccountNumber', label: 'Bank Account Number', model: this.bankAccountNumber, type: 'text', required: true },
    { id: 4, field: 'password', label: 'Password', model: this.password, type: 'password', feedback: true, required: true },
    { id: 5, field: 'confirmPassword', label: 'Confirm Password', model: this.confirmPassword, type: 'password', required: true },
    { id: 6, field: 'role', label: 'Role', model: this.role, type: 'dropdown', options: this.roleOptions, required: true },
  ];


  constructor(private authService: AuthService, private router: Router) { }

  validateEmail(email: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }

  validateBankAccount(bankAccountNumber: string): boolean {
    // Basic German IBAN validation (length check only)
    return bankAccountNumber.length >= 15 && bankAccountNumber.length <= 34 && bankAccountNumber.startsWith('DE');
  }

  validateForm(): boolean {
    if (!this.name.trim()) {
      this.errorMessage = 'Name is required.';
      return false;
    }
    if (!this.email.trim() || !this.validateEmail(this.email)) {
      this.errorMessage = 'A valid email is required.';
      return false;
    }
    if (!this.bankAccountNumber.trim() || !this.validateBankAccount(this.bankAccountNumber)) {
      this.errorMessage = 'A valid German bank account number is required.';
      return false;
    }
    if (!this.password || this.password.length < 8) {
      this.errorMessage = 'Password must be at least 8 characters.';
      return false;
    }
    if (this.password !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      return false;
    }
    if (!this.role) {
      this.errorMessage = 'Role is required.';
      return false;
    }
    return true;
  }

  onRegister(): void {
    this.errorMessage = '';
    this.successMessage = '';
    if (!this.validateForm()) {
      return;
    }
    console.log('Register called');

    const payload = {
      name: this.name,
      email: this.email,
      bankAccountNumber: this.bankAccountNumber,
      passwordHash: this.password,
      role: this.role
    };

    this.authService.register(payload).subscribe({
      next: res => {
        console.log('Register API response:', res);
        this.successMessage = 'Registration successful! Redirecting to login...';
        this.errorMessage = '';
        setTimeout(() => this.router.navigate(['/login']), 2000);
      },
      error: err => {
        console.error('API error:', err);
        this.errorMessage = 'Registration failed. Please check your details.';
        this.successMessage = '';
      }
    });

  }
  onBackToLogin(): void {
    this.router.navigate(['/login']);
  }
}

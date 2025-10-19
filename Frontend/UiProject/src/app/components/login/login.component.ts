import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; // For ngModel
import { AuthService } from '../../services/auth.service';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { FloatLabelModule } from 'primeng/floatlabel';
import { Router } from '@angular/router';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';
import { ClassNamesModule } from 'primeng/classnames'
import {MessageModule} from 'primeng/message';

@Component({
  selector: 'app-login',
  standalone: true,
  templateUrl: './login.component.html',
  host: { class: 'flex justify-content-center align-items-center min-h-screen bg-gray-100' },
  styleUrls: ['./login.component.css'],
  imports: [InputTextModule, PasswordModule, ButtonModule, FormsModule, CommonModule, FloatLabelModule, CardModule, DividerModule, ClassNamesModule, MessageModule]
})

export class LoginComponent {
  username: string = '';
  password: string = '';
  errorMessage: string = '';

  constructor(private authService: AuthService, private router: Router) {}

  onLogin(): void {
    // Construct payload as expected by backend (Username and Password fields)
    this.authService.login({ username: this.username, password: this.password })
      .subscribe({
        next: res => {
          if (res && res.token) {
            // Save token (to localStorage or a service)
            localStorage.setItem('jwt_token', res.token);
            localStorage.setItem('user_id', res.userId);
            this.errorMessage = '';
            setTimeout(() => this.router.navigate(['/dashboard']), 2000);
          } else {
            this.errorMessage = 'Unexpected response from server.';
            alert(this.errorMessage);
          }
        },
        error: err => {
          this.errorMessage = 'Login failed. Please check your credentials.';
          alert(this.errorMessage);
        }
      });
  }

  onRegister(): void {
    this.router.navigate(['/register']);
  }
}

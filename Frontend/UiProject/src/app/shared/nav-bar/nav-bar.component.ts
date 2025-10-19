// src/app/shared/nav-bar/nav-bar.component.ts
import { Component, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-nav-bar',
  standalone: true,
  imports: [RouterModule, ButtonModule, CommonModule],
  templateUrl: './nav-bar.component.html',
  styleUrls: ['./nav-bar.component.css']
})
export class NavBarComponent {
  auth = inject(AuthService);

  onLogout(): void {
    confirm('Are you sure you want to logout?') &&
    this.auth.logout();
  }
}
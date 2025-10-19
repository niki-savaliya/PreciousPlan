import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CardModule } from 'primeng/card';
import { HttpHeaders } from '@angular/common/http';
import { Inject, PLATFORM_ID } from '@angular/core';
import { PasswordDirective } from 'primeng/password';
import { UserService } from '../../services/user.service';

interface User {
  id: string;
  name: string;
  email: string;
  bankAccountNumber: string;
}

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule, InputTextModule, CardModule],
  templateUrl: './user-profile.component.html',
  host: { class: 'flex justify-content-center align-items-center min-h-screen bg-gray-100' },
})
export class UserProfileComponent implements OnInit {
  user: User = {
    id: '',
    name: '',
    email: '',
    bankAccountNumber: '',
  };

  isEditing = false;
  loading = false;

  constructor(
    private userService: UserService,
    @Inject(PLATFORM_ID) private platformId: object,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      // we’re running in the browser—safe to call localStorage
      this.loadUserProfile();
    }
  }

  loadUserProfile() {
    const token = localStorage.getItem('jwt_token');
    const userId = localStorage.getItem('user_id');

    if (!userId) return;

    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);

    this.userService.getUser(userId, headers).subscribe({
      next: (data) => {
        this.user = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading profile:', err);
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }

  toggleEdit() {
    this.isEditing = !this.isEditing;
  }

  saveProfile() {
    const token = localStorage.getItem('jwt_token');
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    this.loading = true;
    this.userService.updateUser(this.user.id, this.user, headers ).subscribe({
      next: () => {
        this.isEditing = false;
        this.loading = false;
        alert('Profile updated successfully!');
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error saving profile:', err);
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }
}

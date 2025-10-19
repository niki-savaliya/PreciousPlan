// src/app/services/auth.service.ts

import { Injectable, signal, inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { isPlatformBrowser } from '@angular/common';
import { Observable, tap } from 'rxjs';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:8080/api/Auth';
  private platformId = inject(PLATFORM_ID);

  /** signal holds whether the user is logged in */
  isLoggedIn = signal<boolean>(false);

  constructor(private http: HttpClient, private router: Router) {
    // Only read localStorage if running in the browser
    if (isPlatformBrowser(this.platformId)) {
      this.isLoggedIn.set(!!localStorage.getItem('jwt_token'));
    }
  }

  login(credentials: { username: string; password: string }): Observable<{ token: string; userId: string }> {
    return this.http
      .post<{ token: string; userId: string }>(`${this.apiUrl}/login`, credentials)
      .pipe(
        tap(response => {
          if (isPlatformBrowser(this.platformId)) {
            localStorage.setItem('jwt_token', response.token);
            localStorage.setItem('user_id', response.userId);
          }
          this.isLoggedIn.set(true);
        })
      );
  }

  register(registerData: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, registerData);
  }

  logout(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('jwt_token');
      localStorage.removeItem('user_id');
    }
    this.isLoggedIn.set(false);
    this.router.navigate(['/login']);
  }
}
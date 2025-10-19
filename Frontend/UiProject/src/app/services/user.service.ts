import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface User {
  id?: string;
  name: string;
  email: string;
  bankAccountNumber: string;
  createdDate?: string;
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private baseUrl = 'http://localhost:8080/api/user';

  constructor(private http: HttpClient) {}

  getUser(id: string, headers: HttpHeaders): Observable<any> {
    return this.http.get(`${this.baseUrl}/${id}`, { headers });
  }

  createUser(user: User, headers: HttpHeaders): Observable<User> {
    return this.http.post<User>(this.baseUrl, user, { headers });
  }

  updateUser(id: string, user: User, headers: HttpHeaders): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, user, { headers });
  }
}
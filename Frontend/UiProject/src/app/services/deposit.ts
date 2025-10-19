import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DepositService {
  
  private apiUrl = 'http://localhost:8080/api/deposit';

  constructor(private http: HttpClient) { }

  getDepositsForPlan(planId: string, headers: HttpHeaders): Observable<any> {
    return this.http.get(`${this.apiUrl}/plan/${planId}`, { headers });
  }
}

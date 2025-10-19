import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TransactionService {

  private apiUrl = 'http://localhost:8080/api/Transactions';

  constructor(private http: HttpClient) { }

  getTransactions( headers: HttpHeaders): Observable<any> {
    return this.http.get(`${this.apiUrl}`, { headers });
  }
  processPayout(planId: string, headers: HttpHeaders): Observable<any> {
    return this.http.post(`${this.apiUrl}/plan/${planId}/payout`, '' , { headers });
  }
}
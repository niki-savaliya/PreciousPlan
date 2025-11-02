import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SavingsPlanService {

  private apiUrl = 'http://localhost:8080/api/SavingsPlan';

  constructor(private http: HttpClient) { }

  create(savingsPlanData: { planType: number, monthlyAmount: number }, headers: HttpHeaders): Observable<any> {
    return this.http.post(`${this.apiUrl}/create`, savingsPlanData, { headers });
  }
  getUserPlans(headers: HttpHeaders): Observable<any> {
    return this.http.get(`${this.apiUrl}/user-plans`, { headers });
  }
  getPlanDetails(headers: HttpHeaders, planId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/${planId}`, { headers });
  }
  getEstimatedPayout(headers: HttpHeaders, planId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/${planId}/estimate-payout`, { headers });
  }
  getKpis(headers: HttpHeaders): Observable<any> {
    return this.http.get(`${this.apiUrl}/kpis`, { headers });
  }
  getsimulations(data: { planType: string, monthlyAmount: number, startDate: string, endDate: string },headers: HttpHeaders): Observable<any> {
    return this.http.post(`${this.apiUrl}/simulate`, data, { headers });
  }
  getLatestPrice(metalType: string, headers: HttpHeaders): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/latest-price/${metalType}`, { headers });
  }
}
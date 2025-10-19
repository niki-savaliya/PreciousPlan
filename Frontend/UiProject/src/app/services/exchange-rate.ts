// src/app/services/exchange-rate.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class ExchangeRateService {
  appId = 'APP_ID'; // Replace with your real one
  baseUrl = 'https://openexchangerates.org/api';

  constructor(private http: HttpClient) { }

  getLatest(base = 'USD', symbol = 'EUR') {
    return this.http.get<any>(
      `${this.baseUrl}/latest.json?app_id=${this.appId}&symbols=${symbol}`
    );
  }

  getHistorical(date: string, symbols = 'XAU,XAG') {
    return this.http.get<any>(
      `${this.baseUrl}/historical/${date}.json?app_id=${this.appId}&symbols=${symbols}`
    );
  }

  // For simulation: Get multiple historical points
  async getHistoricalRange(startDate: string, endDate: string, symbols = 'XAU,XAG') {
    const promises = [];

    // Loop through each year between start and end
    const startYear = parseInt(startDate.substring(0, 4), 10);
    const endYear = parseInt(endDate.substring(0, 4), 10);

    for (let year = startYear; year <= endYear; year++) {
      // Use December 31 of each year or the endDate for the last year
      const dateStr = (year === endYear) ? endDate : `${year}-12-31`;
      // Fetch historical data for that date
      promises.push(
        this.http.get<any>(`${this.baseUrl}/historical/${dateStr}.json?app_id=${this.appId}&symbols=${symbols}`).toPromise()
      );
    }
    return Promise.all(promises);
  }
}

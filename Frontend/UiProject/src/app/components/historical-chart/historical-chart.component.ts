import { ChangeDetectorRef, Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpHeaders } from '@angular/common/http';
import { ChartModule } from 'primeng/chart';
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TooltipModule } from 'primeng/tooltip';

import { SavingsPlanService } from '../../services/savings-plan';

interface SimulationKpi {
  totalDeposits: number;
  portfolioValue: number;
  profitLoss: number;
  returnRatePercent: number;
  years: string[];
  cumulativeDeposits: number[];
  portfolioValues: number[];
  profitLossHistory: number[];
}

@Component({
  selector: 'app-historical-chart',
  standalone: true,
  imports: [CommonModule, FormsModule, ChartModule, TableModule, CardModule, ButtonModule, InputTextModule, SelectModule, TooltipModule],
  templateUrl: './historical-chart.component.html'
})
export class HistoricalChartComponent implements OnInit {
  latestChartData: any;
  latestChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      y: {
        type: 'linear',
        display: true,
        position: 'left',
        title: { display: true, text: 'Price (EUR)' }
      }
    },
    plugins: {
      legend: { display: true, position: 'top' },
      tooltip: { enabled: true }
    }
  };

  simulationData: any = {
    deposits: null,
    portfolio: null,
    profitLoss: null
  };

  simulationOptions = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      x: { title: { display: true, text: 'Years' } },
      y: { title: { display: true, text: 'EUR' } }
    },
    plugins: {
      legend: { display: true, position: 'top' as const }
    }
  };

  availableMetals = [
    { code: 'XAU', name: 'Gold (1 oz)' },
    { code: 'XAG', name: 'Silver (1000g)' }
  ];

  selectedMetal = 'XAU';
  monthlySavings = 100;
  maxDate: string = new Date().toISOString().split('T')[0];
  startDate: string = '2023-01-01';
  endDate: string = this.maxDate;

  simulationKpi: SimulationKpi | null = null;
  isLoading = false;
  isPriceLoading = false;
  errorMessage = '';

  constructor(
    private savingsPlanService: SavingsPlanService,
    @Inject(PLATFORM_ID) private platformId: object,
    private cdRef: ChangeDetectorRef
  ) { }

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.fetchLatestPrices();
      this.runSimulation();
    }
  }

  fetchLatestPrices() {
    this.isPriceLoading = true;
    const headers = this.getAuthHeaders();

    // Fetch Gold Price
    this.savingsPlanService.getLatestPrice('XAU', headers).subscribe({
      next: (goldPrice: number) => {
        // Fetch Silver Price
        this.savingsPlanService.getLatestPrice('XAG', headers).subscribe({
          next: (silverPrice: number) => {
            this.latestChartData = {
              labels: ['Gold (1 oz)', 'Silver (1000g)'],
              datasets: [
                {
                  label: 'Current Price (EUR)',
                  data: [goldPrice, silverPrice],
                  backgroundColor: ['#FFD700', '#C0C0C0'],
                  borderColor: ['#DAA520', '#A8A8A8'],
                  borderWidth: 2
                }
              ]
            };
            this.isPriceLoading = false;
            this.cdRef.detectChanges();
          },
          error: (err) => {
            console.error('Error fetching silver price:', err);
            this.isPriceLoading = false;
          }
        });
      },
      error: (err) => {
        console.error('Error fetching gold price:', err);
        this.isPriceLoading = false;
      }
    });
  }

  async runSimulation() {

    const today = new Date();
    // Truncate 'today' to the 15th of current month if today is after day 15, else keep 15th of previous month to avoid requesting future prices
    let adjustedToday = new Date(today.getFullYear(), today.getMonth(), 15);
    if (today.getDate() < 15) {
      // if today before 15th, use 15th of previous month
      adjustedToday = new Date(today.getFullYear(), today.getMonth() - 1, 15);
    }

    let endDate = new Date(this.endDate);

    // Validation
    if (endDate > adjustedToday) {
      endDate = adjustedToday;
    }

    if (new Date(this.startDate) >= endDate) {
      alert('Start date must be before end date.');
      return;
    }

    if (this.monthlySavings <= 0) {
      alert('Monthly savings must be greater than zero.');
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    // Prepare request payload
    const simulationRequest = {
      planType: this.selectedMetal,
      monthlyAmount: this.monthlySavings,
      startDate: this.formatDate(this.startDate),
      endDate: this.formatDate(endDate.toISOString().substring(0, 10))
    };

    const headers = this.getAuthHeaders();
    console.log('Simulation Request:', simulationRequest);
    console.log('Headers:', headers);
    this.savingsPlanService.getsimulations(simulationRequest, headers).subscribe({
      next: (response: SimulationKpi) => {
        this.simulationKpi = response;
        this.createSimulationCharts(response);
        this.isLoading = false;
        this.cdRef.detectChanges();
      },
      error: (error) => {
        console.error('Simulation error:', error);
        this.errorMessage = error.error?.message || 'Failed to run simulation. Please try again.';
        this.isLoading = false;
        this.cdRef.detectChanges();
      }
    });
  }

  createSimulationCharts(kpi: SimulationKpi) {
    // Cumulative Deposits Chart
    this.simulationData.deposits = {
      labels: kpi.years,
      datasets: [
        {
          label: 'Cumulative Deposits (EUR)',
          data: kpi.cumulativeDeposits,
          borderColor: '#4CAF50',
          backgroundColor: 'rgba(76, 175, 80, 0.1)',
          fill: true,
          tension: 0.4
        }
      ]
    };

    // Portfolio Value vs Deposits Chart
    this.simulationData.portfolio = {
      labels: kpi.years,
      datasets: [
        {
          label: 'Deposits (EUR)',
          data: kpi.cumulativeDeposits,
          borderColor: '#4CAF50',
          backgroundColor: 'transparent',
          fill: false,
          tension: 0.4
        },
        {
          label: 'Portfolio Value (EUR)',
          data: kpi.portfolioValues,
          borderColor: '#FF9800',
          backgroundColor: 'rgba(255, 152, 0, 0.1)',
          fill: true,
          tension: 0.4
        }
      ]
    };

    // Profit/Loss Chart
    this.simulationData.profitLoss = {
      labels: kpi.years,
      datasets: [
        {
          label: 'Profit/Loss (EUR)',
          data: kpi.profitLossHistory,
          borderColor: '#2196F3',
          backgroundColor: 'rgba(33, 150, 243, 0.3)',
          fill: true,
          tension: 0.4
        }
      ]
    };
  }

  getTotalDeposits(): string {
    return this.simulationKpi
      ? this.simulationKpi.totalDeposits.toLocaleString('de-DE', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
      : '0.00';
  }

  getLatestPortfolioValue(): string {
    return this.simulationKpi
      ? this.simulationKpi.portfolioValue.toLocaleString('de-DE', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
      : '0.00';
  }

  getLatestProfit(): string {
    if (!this.simulationKpi) return '0.00';
    const value = this.simulationKpi.profitLoss;
    const formatted = Math.abs(value).toLocaleString('de-DE', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    return value >= 0 ? formatted : `-${formatted}`;
  }

  getLatestProfitValue(): number {
    return this.simulationKpi ? this.simulationKpi.profitLoss : 0;
  }

  getReturnPercentage(): string {
    return this.simulationKpi
      ? this.simulationKpi.returnRatePercent.toFixed(2)
      : '0.00';
  }

  getReturnPercentageValue(): number {
    return this.simulationKpi ? this.simulationKpi.returnRatePercent : 0;
  }

  formatDate(dateString: string): string {
    // Convert 'YYYY-MM-DD' to 'YYYY-MM-15T00:00:00Z' (15th of month)
    const date = new Date(dateString);
    date.setDate(15);
    return date.toISOString();
  }

  getAuthHeaders(): HttpHeaders {
    const token = this.getAuthToken();
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }

  getAuthToken(): string {
    return localStorage.getItem('jwt_token') || '';
  }

  onSimulationUpdate() {
    this.runSimulation();
  }
}
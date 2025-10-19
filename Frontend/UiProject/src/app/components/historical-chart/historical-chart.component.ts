// src/app/components/historical-chart/historical-chart.component.ts
import { ChangeDetectorRef, Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { ExchangeRateService } from '../../services/exchange-rate';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChartModule } from 'primeng/chart';
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TooltipModule } from 'primeng/tooltip';

interface SimulationResult {
  years: string[];
  cumulativeDeposits: number[];
  portfolioValues: number[];
  profitLoss: number[];
}

@Component({
  selector: 'app-historical-chart',
  imports: [CommonModule, FormsModule, ChartModule, TableModule, CardModule, ButtonModule, InputTextModule, SelectModule, TooltipModule],
  templateUrl: './historical-chart.component.html'
})
export class HistoricalChartComponent implements OnInit {
  // Latest chart
  latestChartData: any;
  latestChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      y: {
        type: 'linear',
        display: true,
        position: 'left',
        title: { display: true, text: 'Price Metal 1 (EUR)' }
      },
      y1: {
        type: 'linear',
        display: true,
        position: 'right',
        grid: { drawOnChartArea: false },
        title: { display: true, text: 'Price Metal 2 (EUR)' }
      }
    },
    plugins: {
      legend: { display: true, position: 'top' },
      tooltip: { enabled: true }
    }
  };
  // Simulation charts
  simulationData: any = {
    deposits: null,
    portfolio: null,
    profitLoss: null
  };

  simulationOptions = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      x: { title: { display: true, text: 'Time (Years)' } },
      y: { title: { display: true, text: 'Value (EUR)' } }
    },
    plugins: {
      legend: { display: true, position: 'top' as const }
    }
  };

  // Form data
  latestSymbol = 'XAU'; // Gold
  availableMetals = [
    { code: 'XAU', name: 'Gold (1 oz)' },
    { code: 'XAG', name: 'Silver (1 oz)' }
  ];

  // Simulation parameters
  selectedMetal = 'XAU';
  monthlySavings = 100;

  maxDate: string = new Date().toISOString().split('T')[0]; // Today in 'YYYY-MM-DD'

  startDate: string = '2020-01-01';  // Default or bind from form
  endDate: string = this.maxDate; // Default endDate to today

  constructor(private rates: ExchangeRateService, @Inject(PLATFORM_ID) private platformId: object, private cdRef: ChangeDetectorRef) { }

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.fetchLatest();
      this.runSimulation();
    }
  }

  fetchLatest() {
    // Fetch two metals prices (e.g., Gold XAU and Silver XAG)
    this.rates.getLatest('USD', 'XAU').subscribe(resp1 => {
      const val1 = resp1.rates['XAU'];
      const priceMetal1 = 1 / val1; // Convert USD to EUR approx

      this.rates.getLatest('USD', 'XAG').subscribe(resp2 => {
        const val2 = resp2.rates['XAG'];
        const priceMetal2 = 1 / val2; // Convert USD to EUR approx

        this.latestChartData = {
          labels: [this.getMetalName('XAU'), this.getMetalName('XAG')],
          datasets: [
            {
              label: `Gold Price (EUR)`,
              data: [priceMetal1, null], // Data aligned to first label
              backgroundColor: '#42A5F5',
              yAxisID: 'y'
            },
            {
              label: `Silver Price (EUR)`,
              data: [null, priceMetal2], // Data aligned to second label
              type: 'line',
              borderColor: '#FFA726',
              fill: false,
              yAxisID: 'y1'
            }
          ]
        };
        this.cdRef.detectChanges();
      });
    });
  }

  async runSimulation() {
    if (new Date(this.endDate) > new Date()) {
      alert('End date cannot be in the future.');
      return;
    }
    if (new Date(this.startDate) > new Date(this.endDate)) {
      alert('Start date cannot be after end date.');
      return;
    }

    const historicalData = await this.rates.getHistoricalRange(
      this.startDate,
      this.endDate,
      this.selectedMetal
    );
    this.cdRef.detectChanges();
    const simulation = this.calculateSavingsPlan(historicalData);
    this.createSimulationCharts(simulation);
  }

  getTotalDeposits(): string {
    if (!this.startDate || !this.endDate) return '0';

    const startYear = new Date(this.startDate).getFullYear();
    const endYear = new Date(this.endDate).getFullYear();

    const years = endYear - startYear + 1;

    // Guard against negative or zero years
    if (years <= 0) return '0';

    const total = this.monthlySavings * 12 * years;
    return total.toLocaleString();
  }

  calculateSavingsPlan(historicalData: any[]): SimulationResult {
    const years: string[] = [];
    const cumulativeDeposits: number[] = [];
    const portfolioValues: number[] = [];
    const profitLoss: number[] = [];

    let totalDeposited = 0;
    let totalUnits = 0;

    // Extract years from startDate and endDate for calculations
    const startYear = new Date(this.startDate).getFullYear();

    historicalData.forEach((data, index) => {
      const year = startYear + index;
      const metalPrice = data.rates[this.selectedMetal];
      const priceInEur = 1 / metalPrice; // Approximate EUR price

      const annualDeposit = this.monthlySavings * 12;
      totalDeposited += annualDeposit;

      const unitsPurchased = annualDeposit / priceInEur;
      totalUnits += unitsPurchased;

      const portfolioValue = totalUnits * priceInEur;
      const profit = portfolioValue - totalDeposited;

      years.push(year.toString());
      cumulativeDeposits.push(totalDeposited);
      portfolioValues.push(portfolioValue);
      profitLoss.push(profit);
    });
    this.cdRef.detectChanges();

    return { years, cumulativeDeposits, portfolioValues, profitLoss };
  }

  createSimulationCharts(simulation: SimulationResult) {
    // Chart 1: Cumulative Deposits (Sparbeträge)
    this.simulationData.deposits = {
      labels: simulation.years,
      datasets: [{
        label: 'Cumulative Deposits (EUR)',
        data: simulation.cumulativeDeposits,
        borderColor: '#4CAF50',
        backgroundColor: 'rgba(76, 175, 80, 0.1)',
        fill: true
      }]
    };

    // Chart 2: Portfolio Value (Wertentwicklung)
    this.simulationData.portfolio = {
      labels: simulation.years,
      datasets: [
        {
          label: 'Deposits (EUR)',
          data: simulation.cumulativeDeposits,
          borderColor: '#4CAF50',
          backgroundColor: 'transparent',
          fill: false
        },
        {
          label: 'Portfolio Value (EUR)',
          data: simulation.portfolioValues,
          borderColor: '#FF9800',
          backgroundColor: 'rgba(255, 152, 0, 0.1)',
          fill: true
        }
      ]
    };

    // Chart 3: Profit/Loss (Ertrag-/Verlustkurve)
    this.simulationData.profitLoss = {
      labels: simulation.years,
      datasets: [{
        label: 'Profit/Loss (EUR)',
        data: simulation.profitLoss,
        borderColor: '#2196F3',
        backgroundColor: 'rgba(33, 150, 243, 0.3)', // Fixed static color
        fill: true
      }]
    };

    this.cdRef.detectChanges();
  }

  getMetalName(code: string): string {
    const metal = this.availableMetals.find(m => m.code === code);
    return metal ? metal.name : code;
  }

  onSimulationUpdate() {
    this.runSimulation();
  }

  getLatestPortfolioValue(): string {
    if (!this.simulationData.portfolio?.datasets?.[1]?.data) return '0';
    const data = this.simulationData.portfolio.datasets[1].data;
    return data[data.length - 1]?.toLocaleString() || '0';
  }

  getLatestProfit(): string {
    const profitValue = this.getLatestProfitValue();
    return profitValue.toLocaleString();
  }

  getLatestProfitValue(): number {
    if (!this.simulationData.profitLoss?.datasets?.[0]?.data) return 0;
    const data = this.simulationData.profitLoss.datasets[0].data;
    return data[data.length - 1] || 0;
  }

  getReturnPercentage(): string {
    const returnValue = this.getReturnPercentageValue();
    return returnValue.toFixed(2);
  }

  getReturnPercentageValue(): number {
    const totalDeposits = this.monthlySavings * 12 * (new Date(this.endDate).getFullYear() - new Date(this.startDate).getFullYear() + 1);
    if (!this.simulationData.profitLoss?.datasets?.[0]?.data || totalDeposits === 0) return 0;

    const data = this.simulationData.profitLoss.datasets[0].data;
    const latestProfit = data[data.length - 1] || 0;
    return (latestProfit / totalDeposits) * 100;
  }
}

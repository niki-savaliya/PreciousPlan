import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HistoricalChartComponent } from "../historical-chart/historical-chart.component";

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, HistoricalChartComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent {

}
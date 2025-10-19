import {
  Component,
  Inject,
  Input,
  OnChanges,
  OnInit,
  PLATFORM_ID,
  SimpleChanges,
} from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { HttpHeaders } from '@angular/common/http';
import { ChangeDetectorRef } from '@angular/core';
import { TransactionService } from '../../services/transaction';

export interface Transaction {
  id: string;
  transactionType: string;
  transactionDate: string;
  amount: number;
}

@Component({
  selector: 'app-transaction-list',
  standalone: true,
  imports: [CommonModule, TableModule, CardModule],
  templateUrl: './transaction-list.component.html',
  host: { class: 'flex justify-content-center align-items-center min-h-screen bg-gray-100' },
})
export class TransactionListComponent implements OnInit {
  @Input() planId = '';
  transactions: Transaction[] = [];
  loading = false;

  constructor(
    private transactionService: TransactionService,
    @Inject(PLATFORM_ID) private platformId: object,
    private cdRef: ChangeDetectorRef
  ) {}

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.loadTransactions();
    }
  }

  loadTransactions() {
    const token = localStorage.getItem('jwt_token');
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    this.loading = true;
    this.transactionService.getTransactions(headers).subscribe({
      next: (txs) => {
        this.transactions = txs;
        this.loading = false;
        this.cdRef.detectChanges();
      },
      error: () => (this.loading = false),
    });
  }
}

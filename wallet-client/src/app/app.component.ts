import { Component, OnInit } from '@angular/core';
import { WalletService } from './services/wallet.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {

  balance = 0;
  withdrawalAmount: number | null = null;

  message = '';
  error = '';
  loading = false;

  constructor(private walletService: WalletService) {}

  ngOnInit(): void {
    this.loadBalance();
  }

  loadBalance(): void {
    this.walletService.getBalance().subscribe({
      next: response => {
        this.balance = response.balance;
      },
      error: () => {
        this.error = 'Could not load wallet balance.';
      }
    });
  }

  withdraw(): void {
    this.message = '';
    this.error = '';

    if (!this.withdrawalAmount || this.withdrawalAmount <= 0) {
      this.error = 'Enter a valid withdrawal amount.';
      return;
    }

    this.loading = true;

    this.walletService.withdraw(this.withdrawalAmount).subscribe({
      next: response => {
        this.balance = response.balance;
        this.message = 'Withdrawal successful.';
        this.withdrawalAmount = null;
        this.loading = false;
      },
      error: error => {
        this.error = error.error?.error || 'Withdrawal failed.';
        this.loading = false;
      }
    });
  }
}
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface BalanceResponse {
  walletId: string;
  balance: number;
}

export interface WithdrawRequest {
  amount: number;
}

@Injectable({
  providedIn: 'root'
})
export class WalletService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = 'https://localhost:7292/api/wallets';
  private readonly walletId = '11111111-1111-1111-1111-111111111111';

  getBalance(): Observable<BalanceResponse> {
    return this.http.get<BalanceResponse>(
      `${this.apiUrl}/${this.walletId}/balance`
    );
  }

  withdraw(amount: number): Observable<BalanceResponse> {
    return this.http.post<BalanceResponse>(
      `${this.apiUrl}/${this.walletId}/withdraw`,
      { amount }
    );
  }
}
// =============================================================================
// File: src/Components/reports/party-balances/party-balances.component.ts
// Used for both Customer Balances and Supplier Balances (mode comes from route data)
// Route data: { mode: 'customer' } or { mode: 'supplier' }
// =============================================================================

import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { Subscription } from 'rxjs';
import { FinancialReportsService } from '../../../app/Services/financial-reports.service';
import { DateRangeReq, PartyBalancesReport } from '../../../app/models/Reports';

@Component({
  selector: 'app-party-balances',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule],
  styleUrls: ['../reports-shared.css'],
  template: `
    <div class="report-page" dir="rtl">
      <header class="report-head">
        <div>
          <h2>
            <mat-icon>{{ mode === 'customer' ? 'people' : 'local_shipping' }}</mat-icon>
            {{ title }}
          </h2>
          <p class="subtitle">{{ subtitle }}</p>
        </div>
      </header>

      <section class="filters">
        <div class="field">
          <label>من تاريخ</label>
          <input type="date" [(ngModel)]="req.fromDate">
        </div>
        <div class="field">
          <label>إلى تاريخ</label>
          <input type="date" [(ngModel)]="req.toDate">
        </div>
        <button class="primary-btn" (click)="load()">
          <mat-icon>filter_alt</mat-icon> تطبيق
        </button>
        <button class="ghost-btn" (click)="reset()">إعادة تعيين</button>
      </section>

      <section class="kpis" *ngIf="data">
        <div class="kpi" [class.income]="mode === 'customer'" [class.expense]="mode === 'supplier'">
          <span class="label">{{ mode === 'customer' ? 'إجمالي المدين على العملاء' : 'إجمالي المستحق للموردين' }}</span>
          <span class="value">
            {{ (mode === 'customer' ? data.totalReceivables : data.totalPayables) | number:'1.2-2' }}
          </span>
        </div>
        <div class="kpi">
          <span class="label">عدد {{ mode === 'customer' ? 'العملاء' : 'الموردين' }} ذوي الأرصدة</span>
          <span class="value">{{ data.parties.length }}</span>
        </div>
      </section>

      <section class="table-wrap" *ngIf="data && data.parties.length">
        <table class="report-table">
          <thead>
            <tr>
              <th>الكود</th>
              <th>الاسم</th>
              <th>إجمالي المدين</th>
              <th>إجمالي الدائن</th>
              <th>الرصيد</th>
              <th>آخر حركة</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let p of data.parties">
              <td>{{ p.accountCode }}</td>
              <td class="desc">{{ p.accountName }}</td>
              <td class="num">{{ p.totalDebit | number:'1.2-2' }}</td>
              <td class="num">{{ p.totalCredit | number:'1.2-2' }}</td>
              <td class="num" [class.income]="p.balance > 0" [class.expense]="p.balance < 0">
                {{ p.balance | number:'1.2-2' }}
              </td>
              <td>{{ p.lastTransactionDate ? (p.lastTransactionDate | date:'yyyy-MM-dd') : '-' }}</td>
            </tr>
          </tbody>
        </table>
      </section>

      <div *ngIf="data && !data.parties.length && !loading" class="empty">
        <mat-icon>inbox</mat-icon>
        <p>لا توجد أرصدة في الفترة المحددة</p>
      </div>

      <div *ngIf="loading" class="loading">
        <mat-icon class="spin">refresh</mat-icon>
        <p>جاري التحميل...</p>
      </div>
    </div>
  `
})
export class PartyBalancesComponent implements OnInit, OnDestroy {
  private service = inject(FinancialReportsService);
  private route = inject(ActivatedRoute);
  private sub = new Subscription();

  mode: 'customer' | 'supplier' = 'customer';
  data: PartyBalancesReport | null = null;
  loading = false;

  req: DateRangeReq = {
    fromDate: this.firstDayOfYear(),
    toDate: this.today()
  };

  ngOnInit(): void {
    this.mode = (this.route.snapshot.data['mode'] === 'supplier') ? 'supplier' : 'customer';
    this.load();
  }

  ngOnDestroy(): void { this.sub.unsubscribe(); }

  get title(): string {
    return this.mode === 'customer' ? 'أرصدة العملاء (المدينون)' : 'أرصدة الموردين (الدائنون)';
  }
  get subtitle(): string {
    return this.mode === 'customer'
      ? 'ملخص أرصدة العملاء خلال فترة محددة'
      : 'ملخص أرصدة الموردين خلال فترة محددة';
  }

  load(): void {
    this.loading = true;
    const obs = this.mode === 'customer'
      ? this.service.getCustomerBalances(this.req)
      : this.service.getSupplierBalances(this.req);

    this.sub.add(obs.subscribe({
      next: r => { this.data = r.data ?? null; this.loading = false; },
      error: () => { this.loading = false; }
    }));
  }

  reset(): void {
    this.req = { fromDate: this.firstDayOfYear(), toDate: this.today() };
    this.load();
  }

  private today(): string { return new Date().toISOString().slice(0, 10); }
  private firstDayOfYear(): string {
    const jan1 = new Date(new Date().getFullYear(), 0, 1);
    return jan1.toISOString().slice(0, 10);
  }
}

// =============================================================================
// File: src/Components/reports/trial-balance/trial-balance.component.ts
// =============================================================================

import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { Subscription } from 'rxjs';
import { FinancialReportsService } from '../../../app/Services/financial-reports.service';
import { DateRangeReq, TrialBalance } from '../../../app/models/Reports';

@Component({
  selector: 'app-trial-balance',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule],
  styleUrls: ['../reports-shared.css'],
  template: `
    <div class="report-page" dir="rtl">
      <header class="report-head">
        <div>
          <h2><mat-icon>balance</mat-icon> ميزان المراجعة</h2>
          <p class="subtitle">إجمالي المدين والدائن لكل حساب نهائي</p>
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
      </section>

      <section class="kpis" *ngIf="data">
        <div class="kpi">
          <span class="label">إجمالي المدين</span>
          <span class="value">{{ data.totalDebit | number:'1.2-2' }}</span>
        </div>
        <div class="kpi">
          <span class="label">إجمالي الدائن</span>
          <span class="value">{{ data.totalCredit | number:'1.2-2' }}</span>
        </div>
        <div class="kpi" [class.income]="data.isBalanced" [class.expense]="!data.isBalanced">
          <span class="label">حالة التوازن</span>
          <span class="value">{{ data.isBalanced ? 'متوازن ✓' : 'غير متوازن ✗' }}</span>
        </div>
      </section>

      <section class="table-wrap" *ngIf="data && data.rows.length">
        <table class="report-table">
          <thead>
            <tr>
              <th>الكود</th>
              <th>اسم الحساب</th>
              <th>النوع</th>
              <th>المدين</th>
              <th>الدائن</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let r of data.rows">
              <td>{{ r.accountCode }}</td>
              <td class="desc">{{ r.accountName }}</td>
              <td>{{ accountTypeLabel(r.type) }}</td>
              <td class="num">{{ r.debit | number:'1.2-2' }}</td>
              <td class="num">{{ r.credit | number:'1.2-2' }}</td>
            </tr>
          </tbody>
          <tfoot>
            <tr>
              <td colspan="3">الإجمالي</td>
              <td>{{ data.totalDebit | number:'1.2-2' }}</td>
              <td>{{ data.totalCredit | number:'1.2-2' }}</td>
            </tr>
          </tfoot>
        </table>
      </section>

      <div *ngIf="data && !data.rows.length && !loading" class="empty">
        <mat-icon>inbox</mat-icon>
        <p>لا توجد قيود في الفترة المحددة</p>
      </div>

      <div *ngIf="loading" class="loading">
        <mat-icon class="spin">refresh</mat-icon>
        <p>جاري التحميل...</p>
      </div>
    </div>
  `
})
export class TrialBalanceComponent implements OnInit, OnDestroy {
  private service = inject(FinancialReportsService);
  private sub = new Subscription();

  data: TrialBalance | null = null;
  loading = false;

  req: DateRangeReq = {
    fromDate: this.firstDayOfYear(),
    toDate: this.today()
  };

  ngOnInit(): void { this.load(); }
  ngOnDestroy(): void { this.sub.unsubscribe(); }

  load(): void {
    this.loading = true;
    this.sub.add(
      this.service.getTrialBalance(this.req).subscribe({
        next: r => { this.data = r.data ?? null; this.loading = false; },
        error: () => { this.loading = false; }
      })
    );
  }

  accountTypeLabel(type: number): string {
    // Matches AccountTypes enum order: 1=Assets, 2=Liabilities, 3=Equity, 4=Income, 5=Expenses
    // Adjust if your enum values differ.
    switch (type) {
      case 1: return 'أصول';
      case 2: return 'خصوم';
      case 3: return 'حقوق ملكية';
      case 4: return 'إيرادات';
      case 5: return 'مصروفات';
      default: return '-';
    }
  }

  private today(): string { return new Date().toISOString().slice(0, 10); }
  private firstDayOfYear(): string {
    const jan1 = new Date(new Date().getFullYear(), 0, 1);
    return jan1.toISOString().slice(0, 10);
  }
}

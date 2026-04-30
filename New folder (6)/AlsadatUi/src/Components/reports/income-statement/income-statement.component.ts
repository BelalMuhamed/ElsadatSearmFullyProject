// =============================================================================
// File: src/Components/reports/income-statement/income-statement.component.ts
// =============================================================================

import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { Subscription } from 'rxjs';
import { FinancialReportsService } from '../../../app/Services/financial-reports.service';
import { DateRangeReq, IncomeStatement } from '../../../app/models/Reports';

@Component({
  selector: 'app-income-statement',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule],
  styleUrls: ['../reports-shared.css'],
  template: `
    <div class="report-page" dir="rtl">
      <header class="report-head">
        <div>
          <h2><mat-icon>trending_up</mat-icon> قائمة الدخل</h2>
          <p class="subtitle">الإيرادات والمصروفات وصافي الربح خلال فترة محددة</p>
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
        <div class="kpi income">
          <span class="label">إجمالي الإيرادات</span>
          <span class="value">{{ data.totalRevenue | number:'1.2-2' }}</span>
        </div>
        <div class="kpi expense">
          <span class="label">تكلفة البضاعة المباعة</span>
          <span class="value">{{ data.totalCogs | number:'1.2-2' }}</span>
        </div>
        <div class="kpi" [class.income]="data.grossProfit >= 0" [class.expense]="data.grossProfit < 0">
          <span class="label">إجمالي الربح</span>
          <span class="value">{{ data.grossProfit | number:'1.2-2' }}</span>
        </div>
        <div class="kpi expense">
          <span class="label">إجمالي المصروفات</span>
          <span class="value">{{ data.totalExpenses | number:'1.2-2' }}</span>
        </div>
        <div class="kpi closing" [class.income]="data.netIncome >= 0" [class.expense]="data.netIncome < 0">
          <span class="label">صافي الدخل</span>
          <span class="value">{{ data.netIncome | number:'1.2-2' }}</span>
        </div>
      </section>

      <section class="table-wrap" *ngIf="data && data.revenueLines.length">
        <h3 class="section-title">الإيرادات</h3>
        <table class="report-table">
          <thead>
            <tr>
              <th>الكود</th>
              <th>اسم الحساب</th>
              <th>المدين</th>
              <th>الدائن</th>
              <th>صافي</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let r of data.revenueLines">
              <td>{{ r.accountCode }}</td>
              <td class="desc">{{ r.accountName }}</td>
              <td class="num">{{ r.debit | number:'1.2-2' }}</td>
              <td class="num">{{ r.credit | number:'1.2-2' }}</td>
              <td class="num income">{{ (r.credit - r.debit) | number:'1.2-2' }}</td>
            </tr>
          </tbody>
        </table>
      </section>

      <section class="table-wrap" *ngIf="data && data.expenseLines.length" style="margin-top: 18px;">
        <h3 class="section-title">المصروفات</h3>
        <table class="report-table">
          <thead>
            <tr>
              <th>الكود</th>
              <th>اسم الحساب</th>
              <th>المدين</th>
              <th>الدائن</th>
              <th>صافي</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let r of data.expenseLines">
              <td>{{ r.accountCode }}</td>
              <td class="desc">{{ r.accountName }}</td>
              <td class="num">{{ r.debit | number:'1.2-2' }}</td>
              <td class="num">{{ r.credit | number:'1.2-2' }}</td>
              <td class="num expense">{{ (r.debit - r.credit) | number:'1.2-2' }}</td>
            </tr>
          </tbody>
        </table>
      </section>

      <div *ngIf="data && !data.revenueLines.length && !data.expenseLines.length && !loading" class="empty">
        <mat-icon>inbox</mat-icon>
        <p>لا توجد بيانات في الفترة المحددة</p>
      </div>

      <div *ngIf="loading" class="loading">
        <mat-icon class="spin">refresh</mat-icon>
        <p>جاري التحميل...</p>
      </div>
    </div>
  `,
  styles: [`
    .section-title {
      color: var(--gold);
      padding: 12px 16px;
      margin: 0;
      background: rgba(212, 175, 55, 0.1);
      border-bottom: 1px solid rgba(212, 175, 55, 0.3);
    }
  `]
})
export class IncomeStatementComponent implements OnInit, OnDestroy {
  private service = inject(FinancialReportsService);
  private sub = new Subscription();

  data: IncomeStatement | null = null;
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
      this.service.getIncomeStatement(this.req).subscribe({
        next: r => { this.data = r.data ?? null; this.loading = false; },
        error: () => { this.loading = false; }
      })
    );
  }

  private today(): string { return new Date().toISOString().slice(0, 10); }
  private firstDayOfYear(): string {
    const jan1 = new Date(new Date().getFullYear(), 0, 1);
    return jan1.toISOString().slice(0, 10);
  }
}

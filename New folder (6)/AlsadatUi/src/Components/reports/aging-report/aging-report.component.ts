// =============================================================================
// File: src/Components/reports/aging-report/aging-report.component.ts
// Used for both Receivables Aging and Payables Aging.
// Route data: { mode: 'receivables' } or { mode: 'payables' }
// =============================================================================

import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { Subscription } from 'rxjs';
import { FinancialReportsService } from '../../../app/Services/financial-reports.service';
import { AgingReport, AgingReportReq } from '../../../app/models/Reports';

@Component({
  selector: 'app-aging-report',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule],
  styleUrls: ['../reports-shared.css'],
  template: `
    <div class="report-page" dir="rtl">
      <header class="report-head">
        <div>
          <h2><mat-icon>schedule</mat-icon> {{ title }}</h2>
          <p class="subtitle">تحليل أعمار الديون حتى تاريخ محدد</p>
        </div>
      </header>

      <section class="filters">
        <div class="field">
          <label>حتى تاريخ</label>
          <input type="date" [(ngModel)]="req.asOfDate">
        </div>
        <div class="field">
          <label>الفترة 1 (يوم)</label>
          <input type="number" min="1" [(ngModel)]="req.bucket1Days">
        </div>
        <div class="field">
          <label>الفترة 2 (يوم)</label>
          <input type="number" min="1" [(ngModel)]="req.bucket2Days">
        </div>
        <div class="field">
          <label>الفترة 3 (يوم)</label>
          <input type="number" min="1" [(ngModel)]="req.bucket3Days">
        </div>
        <button class="primary-btn" (click)="load()">
          <mat-icon>filter_alt</mat-icon> تطبيق
        </button>
      </section>

      <section class="table-wrap" *ngIf="data && data.rows.length">
        <table class="report-table">
          <thead>
            <tr>
              <th>{{ mode === 'receivables' ? 'العميل' : 'المورد' }}</th>
              <th>الحالي</th>
              <th>1 - {{ req.bucket1Days }}</th>
              <th>{{ req.bucket1Days + 1 }} - {{ req.bucket2Days }}</th>
              <th>{{ req.bucket2Days + 1 }} - {{ req.bucket3Days }}</th>
              <th>أكثر من {{ req.bucket3Days }}</th>
              <th>الإجمالي</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let row of data.rows">
              <td class="desc">{{ row.accountName }}</td>
              <td class="num">{{ row.current | number:'1.2-2' }}</td>
              <td class="num">{{ row.bucket1 | number:'1.2-2' }}</td>
              <td class="num">{{ row.bucket2 | number:'1.2-2' }}</td>
              <td class="num">{{ row.bucket3 | number:'1.2-2' }}</td>
              <td class="num expense">{{ row.over | number:'1.2-2' }}</td>
              <td class="num"><strong>{{ row.total | number:'1.2-2' }}</strong></td>
            </tr>
          </tbody>
          <tfoot>
            <tr>
              <td>الإجمالي</td>
              <td>{{ data.totals.current | number:'1.2-2' }}</td>
              <td>{{ data.totals.bucket1 | number:'1.2-2' }}</td>
              <td>{{ data.totals.bucket2 | number:'1.2-2' }}</td>
              <td>{{ data.totals.bucket3 | number:'1.2-2' }}</td>
              <td>{{ data.totals.over | number:'1.2-2' }}</td>
              <td>{{ data.totals.total | number:'1.2-2' }}</td>
            </tr>
          </tfoot>
        </table>
      </section>

      <div *ngIf="data && !data.rows.length && !loading" class="empty">
        <mat-icon>inbox</mat-icon>
        <p>لا توجد ديون حتى التاريخ المحدد</p>
      </div>

      <div *ngIf="loading" class="loading">
        <mat-icon class="spin">refresh</mat-icon>
        <p>جاري التحميل...</p>
      </div>
    </div>
  `
})
export class AgingReportComponent implements OnInit, OnDestroy {
  private service = inject(FinancialReportsService);
  private route = inject(ActivatedRoute);
  private sub = new Subscription();

  mode: 'receivables' | 'payables' = 'receivables';
  data: AgingReport | null = null;
  loading = false;

  req: AgingReportReq = {
    asOfDate: new Date().toISOString().slice(0, 10),
    bucket1Days: 30,
    bucket2Days: 60,
    bucket3Days: 90
  };

  ngOnInit(): void {
    this.mode = (this.route.snapshot.data['mode'] === 'payables') ? 'payables' : 'receivables';
    this.load();
  }

  ngOnDestroy(): void { this.sub.unsubscribe(); }

  get title(): string {
    return this.mode === 'receivables'
      ? 'أعمار ديون العملاء'
      : 'أعمار ديون الموردين';
  }

  load(): void {
    this.loading = true;
    const obs = this.mode === 'receivables'
      ? this.service.getReceivablesAging(this.req)
      : this.service.getPayablesAging(this.req);

    this.sub.add(obs.subscribe({
      next: r => { this.data = r.data ?? null; this.loading = false; },
      error: () => { this.loading = false; }
    }));
  }
}

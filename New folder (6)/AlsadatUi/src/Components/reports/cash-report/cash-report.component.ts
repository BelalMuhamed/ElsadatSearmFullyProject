// =============================================================================
// File: src/Components/reports/cash-report/cash-report.component.ts
// Place reports-shared.css at: src/Components/reports/reports-shared.css
// =============================================================================

import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { Subscription } from 'rxjs';
import { FinancialReportsService } from '../../../app/Services/financial-reports.service';
import { CashReport, CashReportReq } from '../../../app/models/Reports';

@Component({
  selector: 'app-cash-report',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatPaginatorModule],
  styleUrls: ['../reports-shared.css'],
  template: `
    <div class="report-page" dir="rtl">
      <header class="report-head">
        <div>
          <h2><mat-icon>payments</mat-icon> تقرير حركة الصندوق</h2>
          <p class="subtitle">الوارد والصادر النقدي خلال فترة محددة</p>
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
        <div class="field">
          <label>نوع الحركة</label>
          <select [(ngModel)]="req.direction">
            <option [ngValue]="0">الكل</option>
            <option [ngValue]="1">وارد فقط</option>
            <option [ngValue]="2">صادر فقط</option>
          </select>
        </div>
        <button class="primary-btn" (click)="load()">
          <mat-icon>filter_alt</mat-icon> تطبيق
        </button>
        <button class="ghost-btn" (click)="reset()">إعادة تعيين</button>
      </section>

      <section class="kpis" *ngIf="data">
        <div class="kpi">
          <span class="label">الرصيد الافتتاحي</span>
          <span class="value">{{ data.openingBalance | number:'1.2-2' }}</span>
        </div>
        <div class="kpi income">
          <span class="label">إجمالي الوارد</span>
          <span class="value">{{ data.totalIncoming | number:'1.2-2' }}</span>
        </div>
        <div class="kpi expense">
          <span class="label">إجمالي الصادر</span>
          <span class="value">{{ data.totalOutgoing | number:'1.2-2' }}</span>
        </div>
        <div class="kpi closing">
          <span class="label">الرصيد الختامي</span>
          <span class="value">{{ data.closingBalance | number:'1.2-2' }}</span>
        </div>
      </section>

      <section class="table-wrap" *ngIf="data && data.movements.length">
        <table class="report-table">
          <thead>
            <tr>
              <th>التاريخ</th>
              <th>رقم القيد</th>
              <th>نوع المرجع</th>
              <th>رقم المرجع</th>
              <th>الوصف</th>
              <th>وارد</th>
              <th>صادر</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let m of data.movements">
              <td>{{ m.entryDate | date:'yyyy-MM-dd' }}</td>
              <td>{{ m.journalEntryId }}</td>
              <td>{{ m.referenceType ?? '-' }}</td>
              <td>{{ m.referenceNo ?? '-' }}</td>
              <td class="desc">{{ m.description }}</td>
              <td class="num income">{{ m.incoming ? (m.incoming | number:'1.2-2') : '-' }}</td>
              <td class="num expense">{{ m.outgoing ? (m.outgoing | number:'1.2-2') : '-' }}</td>
            </tr>
          </tbody>
        </table>

        <mat-paginator
          [length]="data.totalCount"
          [pageSize]="req.pageSize"
          [pageIndex]="req.page - 1"
          [pageSizeOptions]="[10, 20, 50, 100]"
          (page)="onPage($event)">
        </mat-paginator>
      </section>

      <div *ngIf="data && !data.movements.length && !loading" class="empty">
        <mat-icon>inbox</mat-icon>
        <p>لا توجد حركات في الفترة المحددة</p>
      </div>

      <div *ngIf="loading" class="loading">
        <mat-icon class="spin">refresh</mat-icon>
        <p>جاري التحميل...</p>
      </div>
    </div>
  `
})
export class CashReportComponent implements OnInit, OnDestroy {
  private service = inject(FinancialReportsService);
  private sub = new Subscription();

  data: CashReport | null = null;
  loading = false;

  req: CashReportReq = {
    fromDate: this.firstDayOfMonth(),
    toDate: this.today(),
    direction: 0,
    page: 1,
    pageSize: 20
  };

  ngOnInit(): void { this.load(); }
  ngOnDestroy(): void { this.sub.unsubscribe(); }

  load(): void {
    this.loading = true;
    this.sub.add(
      this.service.getCashReport(this.req).subscribe({
        next: r => { this.data = r.data ?? null; this.loading = false; },
        error: () => { this.loading = false; }
      })
    );
  }

  reset(): void {
    this.req = {
      fromDate: this.firstDayOfMonth(),
      toDate: this.today(),
      direction: 0,
      page: 1,
      pageSize: 20
    };
    this.load();
  }

  onPage(e: PageEvent): void {
    this.req.page = e.pageIndex + 1;
    this.req.pageSize = e.pageSize;
    this.load();
  }

  private today(): string { return new Date().toISOString().slice(0, 10); }
  private firstDayOfMonth(): string {
    const d = new Date(); d.setDate(1);
    return d.toISOString().slice(0, 10);
  }
}

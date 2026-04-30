// =============================================================================
// File: src/Components/reports/inventory-report/inventory-report.component.ts
// =============================================================================

import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { Subscription } from 'rxjs';
import { FinancialReportsService } from '../../../app/Services/financial-reports.service';
import { InventoryMovement, InventoryMovementReq } from '../../../app/models/Reports';

@Component({
  selector: 'app-inventory-report',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatPaginatorModule],
  styleUrls: ['../reports-shared.css'],
  template: `
    <div class="report-page" dir="rtl">
      <header class="report-head">
        <div>
          <h2><mat-icon>inventory_2</mat-icon> تقرير حركة المخزون</h2>
          <p class="subtitle">حركة المخزون محاسبياً (الوارد والصادر القيمي)</p>
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
        <div class="kpi">
          <span class="label">القيمة الافتتاحية</span>
          <span class="value">{{ data.openingValue | number:'1.2-2' }}</span>
        </div>
        <div class="kpi income">
          <span class="label">إجمالي الوارد</span>
          <span class="value">{{ data.totalIn | number:'1.2-2' }}</span>
        </div>
        <div class="kpi expense">
          <span class="label">إجمالي الصادر</span>
          <span class="value">{{ data.totalOut | number:'1.2-2' }}</span>
        </div>
        <div class="kpi closing">
          <span class="label">القيمة الختامية</span>
          <span class="value">{{ data.closingValue | number:'1.2-2' }}</span>
        </div>
      </section>

      <section class="table-wrap" *ngIf="data && data.rows.length">
        <table class="report-table">
          <thead>
            <tr>
              <th>التاريخ</th>
              <th>نوع المرجع</th>
              <th>رقم المرجع</th>
              <th>الوصف</th>
              <th>وارد (مدين)</th>
              <th>صادر (دائن)</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let r of data.rows">
              <td>{{ r.date | date:'yyyy-MM-dd' }}</td>
              <td>{{ r.referenceType }}</td>
              <td>{{ r.referenceNo }}</td>
              <td class="desc">{{ r.description }}</td>
              <td class="num income">{{ r.stockIn ? (r.stockIn | number:'1.2-2') : '-' }}</td>
              <td class="num expense">{{ r.stockOut ? (r.stockOut | number:'1.2-2') : '-' }}</td>
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

      <div *ngIf="data && !data.rows.length && !loading" class="empty">
        <mat-icon>inbox</mat-icon>
        <p>لا توجد حركات مخزون في الفترة المحددة</p>
      </div>

      <div *ngIf="loading" class="loading">
        <mat-icon class="spin">refresh</mat-icon>
        <p>جاري التحميل...</p>
      </div>
    </div>
  `
})
export class InventoryReportComponent implements OnInit, OnDestroy {
  private service = inject(FinancialReportsService);
  private sub = new Subscription();

  data: InventoryMovement | null = null;
  loading = false;

  req: InventoryMovementReq = {
    fromDate: this.firstDayOfMonth(),
    toDate: this.today(),
    page: 1,
    pageSize: 20
  };

  ngOnInit(): void { this.load(); }
  ngOnDestroy(): void { this.sub.unsubscribe(); }

  load(): void {
    this.loading = true;
    this.sub.add(
      this.service.getInventoryMovement(this.req).subscribe({
        next: r => { this.data = r.data ?? null; this.loading = false; },
        error: () => { this.loading = false; }
      })
    );
  }

  reset(): void {
    this.req = {
      fromDate: this.firstDayOfMonth(),
      toDate: this.today(),
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

// Reports landing page — a card grid pointing to each report.

import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';

interface ReportCard {
  title: string;
  description: string;
  icon: string;
  route: string;
  accent: 'income' | 'expense' | 'asset' | 'neutral';
}

@Component({
  selector: 'app-reports-dashboard',
  standalone: true,
  imports: [CommonModule, MatIconModule, RouterLink],
  template: `
    <div class="reports-page" dir="rtl">
      <header class="page-head">
        <div>
          <h2>التقارير المحاسبية</h2>
          <p class="subtitle">تقارير مالية مفصلة لمتابعة وضع المنشأة</p>
        </div>
      </header>

      <div class="cards">
        <a *ngFor="let r of reports"
           class="card"
           [class.income]="r.accent === 'income'"
           [class.expense]="r.accent === 'expense'"
           [class.asset]="r.accent === 'asset'"
           [routerLink]="r.route">
          <span class="icon-wrap"><mat-icon>{{ r.icon }}</mat-icon></span>
          <div class="card-body">
            <h3>{{ r.title }}</h3>
            <p>{{ r.description }}</p>
          </div>
          <mat-icon class="chevron">chevron_left</mat-icon>
        </a>
      </div>
    </div>
  `,
  styles: [`
    .reports-page { padding: 24px; color: var(--text-color); }
    .page-head { margin-bottom: 24px; padding-bottom: 12px; border-bottom: 2px solid var(--gold); }
    .page-head h2 { color: var(--gold); margin: 0; }
    .subtitle { opacity: .7; margin: 4px 0 0; }

    .cards { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 18px; }
    .card {
      display: flex; align-items: center; gap: 14px;
      padding: 18px 16px; border-radius: 12px;
      background: var(--sidenav-bg); border: 1px solid rgba(212,175,55,.3);
      color: var(--text-color); text-decoration: none;
      transition: transform .15s ease, border-color .15s ease, box-shadow .2s;
    }
    .card:hover { transform: translateY(-2px); border-color: var(--gold); box-shadow: 0 6px 16px rgba(0,0,0,.35); }
    .icon-wrap {
      width: 48px; height: 48px; border-radius: 10px;
      display: inline-flex; align-items: center; justify-content: center;
      background: rgba(212,175,55,.15); color: var(--gold);
    }
    .card.income .icon-wrap  { background: rgba(102,187,106,.15); color: #66bb6a; }
    .card.expense .icon-wrap { background: rgba(239,83,80,.15);  color: #ef5350; }
    .card.asset .icon-wrap   { background: rgba(66,165,245,.15); color: #42a5f5; }

    .card-body { flex: 1; min-width: 0; }
    .card-body h3 { margin: 0 0 4px; font-size: 1rem; }
    .card-body p  { margin: 0; opacity: .7; font-size: .85rem; }
    .chevron { opacity: .4; }
  `]
})
export class ReportsDashboardComponent {
  reports: ReportCard[] = [
    { title: 'حركة الصندوق', description: 'الوارد والصادر النقدي خلال فترة', icon: 'payments',         route: '/reports/cash',              accent: 'asset'   },
    { title: 'أرصدة العملاء', description: 'أرصدة الذمم المدينة',           icon: 'people',           route: '/reports/customers/balances', accent: 'income'  },
    { title: 'أرصدة الموردين', description: 'أرصدة الذمم الدائنة',           icon: 'local_shipping',   route: '/reports/suppliers/balances', accent: 'expense' },
    { title: 'أعمار ديون العملاء', description: 'تحليل عمر الذمم المدينة',    icon: 'schedule',         route: '/reports/customers/aging',    accent: 'income'  },
    { title: 'أعمار ديون الموردين', description: 'تحليل عمر الذمم الدائنة',    icon: 'schedule',         route: '/reports/suppliers/aging',    accent: 'expense' },
    { title: 'حركة المخزون', description: 'الوارد والصادر للمخزون محاسبياً',   icon: 'inventory_2',      route: '/reports/inventory',          accent: 'asset'   },
    { title: 'ميزان المراجعة', description: 'إجمالي المدين والدائن لكل حساب',  icon: 'balance',          route: '/reports/trial-balance',      accent: 'neutral' },
    { title: 'قائمة الدخل', description: 'الإيرادات والمصروفات وصافي الربح',   icon: 'trending_up',      route: '/reports/income-statement',   accent: 'neutral' },
  ];
}
 
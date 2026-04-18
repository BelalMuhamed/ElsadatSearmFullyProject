import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { MatDividerModule } from '@angular/material/divider';
import { EmployeeService } from '../../app/Services/employee.service';
import { 
  EmployeeSalaryDTo, 
  MonthlySalarySummaryDto,
  MonthlyStatisticsDto,
  SalaryComparisonDto,
  SalaryHistoryDto 
} from '../../app/models/IEmployee';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-employee-salary',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSelectModule,
    MatCardModule,
    MatTabsModule,
    MatExpansionModule,
    MatProgressBarModule,
    MatTableModule,
    MatDividerModule,
    RouterModule
  ],
  templateUrl: './employee-salary-component.html',
  styleUrls: ['./employee-salary-component.css']
})
export class EmployeeSalaryComponent implements OnInit {
  private employeeService = inject(EmployeeService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  employeeCode: string = '';
  selectedMonth: number = new Date().getMonth() + 1;
  selectedYear: number = new Date().getFullYear();
  
  // Data objects
  salaryData: EmployeeSalaryDTo | null = null;
  summaryData: MonthlySalarySummaryDto | null = null;
  statisticsData: MonthlyStatisticsDto | null = null;
  comparisonData: SalaryComparisonDto | null = null;
  historyData: SalaryHistoryDto | null = null;
  
  isLoading = false;
  compareMonth: number = this.selectedMonth - 1 > 0 ? this.selectedMonth - 1 : 12;
  compareYear: number = this.selectedMonth - 1 > 0 ? this.selectedYear : this.selectedYear - 1;

  months = [
    { id: 1, name: 'يناير' },
    { id: 2, name: 'فبراير' },
    { id: 3, name: 'مارس' },
    { id: 4, name: 'أبريل' },
    { id: 5, name: 'مايو' },
    { id: 6, name: 'يونيو' },
    { id: 7, name: 'يوليو' },
    { id: 8, name: 'أغسطس' },
    { id: 9, name: 'سبتمبر' },
    { id: 10, name: 'أكتوبر' },
    { id: 11, name: 'نوفمبر' },
    { id: 12, name: 'ديسمبر' }
  ];

  years: number[] = [];

  ngOnInit(): void {
    // Generate years list (current year ± 5 years)
    const currentYear = new Date().getFullYear();
    for (let i = currentYear - 5; i <= currentYear + 5; i++) {
      this.years.push(i);
    }

    // Get employee code from route params
    this.route.params.subscribe((params) => {
      if (params['empCode']) {
        this.employeeCode = params['empCode'];
        this.loadAllData();
      }
    });
  }

  loadAllData(): void {
    if (!this.employeeCode) {
      Swal.fire('خطأ', 'كود الموظف غير متوفر', 'error');
      return;
    }

    this.isLoading = true;
    
    // Load all data in parallel
    this.employeeService.getEmployeeSalary(
      this.employeeCode,
      this.selectedMonth,
      this.selectedYear
    ).subscribe({
      next: (res: any) => {
        if (res?.isSuccess && res?.data) {
          this.salaryData = res.data;
        } else {
          Swal.fire('خطأ', res?.message ?? 'فشل في جلب بيانات الراتب', 'error');
        }
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ في جلب البيانات', 'error');
      }
    });

    this.employeeService.getMonthlySalarySummary(
      this.employeeCode,
      this.selectedMonth,
      this.selectedYear
    ).subscribe({
      next: (res: any) => {
        if (res?.isSuccess && res?.data) {
          this.summaryData = res.data;
        }
      },
      error: (err) => console.error('Summary error:', err)
    });

    this.employeeService.getMonthlyStatistics(
      this.employeeCode,
      this.selectedMonth,
      this.selectedYear
    ).subscribe({
      next: (res: any) => {
        if (res?.isSuccess && res?.data) {
          this.statisticsData = res.data;
        }
      },
      error: (err) => console.error('Statistics error:', err)
    });

    this.employeeService.getSalaryHistory(
      this.employeeCode,
      this.selectedYear
    ).subscribe({
      next: (res: any) => {
        if (res?.isSuccess && res?.data) {
          this.historyData = res.data;
        }
      },
      error: (err) => console.error('History error:', err)
    });
  }

  onMonthChange(): void {
    this.loadAllData();
  }

  onYearChange(): void {
    this.loadAllData();
  }

  loadComparison(): void {
    if (!this.salaryData) {
      Swal.fire('خطأ', 'لا توجد بيانات لمقارنة', 'error');
      return;
    }

    this.employeeService.compareMonthlySalaries(
      this.employeeCode,
      this.selectedMonth,
      this.selectedYear,
      this.compareMonth,
      this.compareYear
    ).subscribe({
      next: (res: any) => {
        if (res?.isSuccess && res?.data) {
          this.comparisonData = res.data;
        } else {
          Swal.fire('خطأ', res?.message ?? 'فشل في جلب المقارنة', 'error');
        }
      },
      error: (err) => {
        Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ في المقارنة', 'error');
      }
    });
  }

  formatCurrency(value: number | null | undefined): string {
    if (!value && value !== 0) {
      return '0 ج.م';
    }
    return value.toLocaleString('ar-EG', { style: 'currency', currency: 'EGP' });
  }

  formatPercentage(value: number | null | undefined): string {
    if (!value && value !== 0) {
      return '0%';
    }
    return value.toFixed(2) + '%';
  }

  getPerformanceColor(level: string): string {
    switch (level?.toLowerCase()) {
      case 'ممتاز':
      case 'excellent':
        return '#4caf50';
      case 'جيد جدا':
      case 'very good':
        return '#8bc34a';
      case 'جيد':
      case 'good':
        return '#ffc107';
      case 'مقبول':
      case 'acceptable':
        return '#ff9800';
      default:
        return '#f44336';
    }
  }

  getTrendIcon(trend: string): string {
    if (!trend) return 'trending_flat';
    if (trend.includes('متزايد') || trend.includes('increasing')) return 'trending_up';
    if (trend.includes('متناقص') || trend.includes('decreasing')) return 'trending_down';
    return 'trending_flat';
  }

  getStatusColor(status: string): string {
    switch (status?.toLowerCase()) {
      case 'جيد':
      case 'good':
        return '#4caf50';
      case 'متوسط':
      case 'medium':
        return '#ffc107';
      case 'ضعيف':
      case 'poor':
        return '#f44336';
      default:
        return '#666';
    }
  }

  getMonthName(month: number): string {
    const monthNames = [
      'يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو',
      'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر'
    ];
    return monthNames[month - 1] || 'غير معروف';
  }

  goBack(): void {
    this.router.navigate(['/hr/employees']);
  }

  printSalary(): void {
    if (!this.salaryData) {
      Swal.fire('خطأ', 'لا توجد بيانات لطباعة', 'error');
      return;
    }
    window.print();
  }

  downloadPDF(): void {
    Swal.fire({
      icon: 'info',
      title: 'قريباً',
      text: 'ميزة تحميل PDF ستكون متوفرة قريباً'
    });
  }
}

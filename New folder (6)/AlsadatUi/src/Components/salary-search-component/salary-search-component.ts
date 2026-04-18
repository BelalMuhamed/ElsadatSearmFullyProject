import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { MatCardModule } from '@angular/material/card';
import { EmployeeService } from '../../app/Services/employee.service';
import { EmployeeSalaryDTo } from '../../app/models/IEmployee';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-salary-search',
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
    RouterModule
  ],
  templateUrl: './salary-search-component.html',
  styleUrls: ['./salary-search-component.css']
})
export class SalarySearchComponent implements OnInit {
  private employeeService = inject(EmployeeService);
  private router = inject(Router);

  employeeCode: string = '';
  selectedMonth: number = new Date().getMonth() + 1;
  selectedYear: number = new Date().getFullYear();
  
  salaryData: EmployeeSalaryDTo | null = null;
  isLoading = false;
  isSearching = false;

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
  }

  getSalaryData(): void {
    if (!this.employeeCode || this.employeeCode.trim() === '') {
      Swal.fire('خطأ', 'الرجاء إدخال كود الموظف', 'error');
      return;
    }

    this.isSearching = true;

    this.employeeService.getEmployeeSalary(
      this.employeeCode.trim(),
      this.selectedMonth,
      this.selectedYear
    ).subscribe({
      next: (res: any) => {
        // response received
        
        if (res?.isSuccess && res?.data) {
          this.salaryData = res.data;
          this.isSearching = false;
        } else {
          this.isSearching = false;
          Swal.fire('خطأ', res?.message ?? 'فشل في جلب بيانات المرتب', 'error');
          this.salaryData = null;
        }
      },
      error: (err) => {
        this.isSearching = false;
        Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ في جلب البيانات', 'error');
        this.salaryData = null;
      }
    });
  }

  onMonthChange(): void {
    if (this.employeeCode && this.employeeCode.trim() !== '') {
      this.getSalaryData();
    }
  }

  onYearChange(): void {
    if (this.employeeCode && this.employeeCode.trim() !== '') {
      this.getSalaryData();
    }
  }

  formatCurrency(value: number): string {
    return value ? value.toLocaleString('ar-EG', { style: 'currency', currency: 'EGP' }) : '0 ج.م';
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

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
import { MatTabsModule } from '@angular/material/tabs';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { MatDividerModule } from '@angular/material/divider';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { PayrollService } from '../../app/Services/payroll.service';
import { EmployeeService } from '../../app/Services/employee.service';
import { RepresentativeService } from '../../app/Services/representative-service';
import {
  PayrollFilterDto,
  PayrollResponseDto,
  BulkPayrollResultDto,
  PayrollSummaryDto,
  GeneratePayrollRequestDto,
  GenerateBulkPayrollRequestDto,
  MarkPayrollPaidDto,
  PayrollPreviewDto,
  PreviewBulkPayrollDto,
  PayrollStatus
} from '../../app/models/IPayroll';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-payroll',
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
    MatCheckboxModule,
    MatSlideToggleModule,
    MatAutocompleteModule,
    MatProgressSpinnerModule,
    MatPaginatorModule,
    RouterModule
  ],
  templateUrl: './payroll-component.html',
  styleUrls: ['./payroll-component.css']
})
export class PayrollComponent implements OnInit {
  private payrollService = inject(PayrollService);
  private employeeService = inject(EmployeeService);
  private representativeService = inject(RepresentativeService);
  private router = inject(Router);

  // Expose PayrollStatus to template
  PayrollStatus = PayrollStatus;

  // UI State
  isLoading = false;
  selectedMonth: number = new Date().getMonth() + 1;
  selectedYear: number = new Date().getFullYear();
  activeTabIndex: number = 2; // Default to Payroll List tab (0-indexed, so 2 = third tab)

  // Employee Filter State
  generateEmployeeFilter = '';
  employees: any[] = [];
  employeesLoading = false;
  // when true the selected code is a representative code
  generateSelectedIsRepresentative = false;

  // Generate Payroll State
  generateEmployeeCode = '';
  generateMonth: number = new Date().getMonth() + 1;
  generateYear: number = new Date().getFullYear();
  generatePaymentMethod: string = 'BankTransfer';
  
  // Preview & Loan Deduction State
  payrollPreview: PayrollPreviewDto | null = null;
  payrollPreviewLoading = false;
  payLoansFromSalary = false;
  previewShowDialog = false;

  // Bulk Preview State
  bulkPayrollPreview: PreviewBulkPayrollDto | null = null;
  bulkPayrollPreviewLoading = false;
  bulkPreviewShowDialog = false;
  expandedPayrollIndex: number | null = null;

  // Bulk Payroll State
  bulkMonth: number = new Date().getMonth() + 1;
  bulkYear: number = new Date().getFullYear();
  bulkIncludeActiveOnly = true;
  bulkAutoPost = false;
  bulkPaymentMethod = 'BankTransfer';
  bulkPayLoansFromSalary = false;
  selectedEmployeeCodes: string[] = [];

  // Filter State
  filterMonth?: number;
  filterYear?: number;
  filterStatus: string = '';
  filterFromDate?: Date;
  filterToDate?: Date;
  filterMinSalary?: number;
  filterMaxSalary?: number;
  filterDepartments: string[] = [];

  // Data Objects
  payrolls: PayrollResponseDto[] = [];
  bulkResult: BulkPayrollResultDto | null = null;
  summary: PayrollSummaryDto | null = null;
  selectedPayroll: PayrollResponseDto | null = null;

  // Selection State
  selectedPayrollIds: number[] = [];

  // Computed selection helpers
  get selectedCreatedIds(): number[] {
    return this.selectedPayrollIds.filter(id => this.allPayrolls.find(p => p.id === id)?.status === PayrollStatus.Created);
  }

  get selectedApprovedIds(): number[] {
    return this.selectedPayrollIds.filter(id => this.allPayrolls.find(p => p.id === id)?.status === PayrollStatus.Approved);
  }

  get hasMixedSelection(): boolean {
    return this.selectedCreatedIds.length > 0 && this.selectedApprovedIds.length > 0;
  }

  get canPostSelected(): boolean {
    return this.selectedCreatedIds.length > 0;
  }

  get canPaySelected(): boolean {
    return this.selectedApprovedIds.length > 0;
  }

  // Check if a payroll can be selected (only status Created or Approved can be selected)
  canSelectPayroll(payroll: PayrollResponseDto): boolean {
    return payroll.status === PayrollStatus.Created || payroll.status === PayrollStatus.Approved;
  }

  // Get selectable payrolls in current page
  get selectablePayrollsInPage(): PayrollResponseDto[] {
    return this.paginatedPayrolls.filter(p => this.canSelectPayroll(p));
  }

  // Check if all selectable payrolls are selected
  get allSelectableSelected(): boolean {
    const selectableCount = this.selectablePayrollsInPage.length;
    if (selectableCount === 0) return false;
    return this.selectedPayrollIds.filter(id => 
      this.selectablePayrollsInPage.some(p => p.id === id)
    ).length === selectableCount;
  }

  // Check if some (but not all) selectable payrolls are selected
  get someSelectableSelected(): boolean {
    const selectableSelectedCount = this.selectedPayrollIds.filter(id => 
      this.selectablePayrollsInPage.some(p => p.id === id)
    ).length;
    return selectableSelectedCount > 0 && !this.allSelectableSelected;
  }

  // List State
  allPayrolls: PayrollResponseDto[] = [];
  paginatedPayrolls: PayrollResponseDto[] = [];
  pageSize = 10;
  currentPage = 1;
  totalPages = 1;

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

  statusOptions = [
    { value: '', name: 'الكل' },
    { value: PayrollStatus.Created, name: 'تم الإنشاء' },
    { value: PayrollStatus.Approved, name: 'تمت الموافقة' },
    { value: PayrollStatus.Paid, name: 'تم الدفع' },
    { value: PayrollStatus.Rejected, name: 'مرفوض' },
    { value: PayrollStatus.Cancelled, name: 'ملغى' }
  ];

  paymentMethods = [
    { value: 'BankTransfer', name: 'تحويل بنكي' },
    { value: 'Cash', name: 'نقداً' },
    { value: 'Check', name: 'شيك' }
  ];

  years: number[] = [];

  ngOnInit(): void {
    // Generate years list
    const currentYear = new Date().getFullYear();
    for (let i = currentYear - 5; i <= currentYear + 5; i++) {
      this.years.push(i);
    }

    this.loadAllEmployees();
    this.loadSummary();
    // تحميل جميع الكشوفات تلقائياً عند فتح الصفحة
    this.loadPayrolls();
  }

  // ==================== Employee Autocomplete ====================
  loadAllEmployees(): void {
    this.employeesLoading = true;
    const params = { pageNumber: 1, pageSize: 1000 };
    // load employees then representatives and merge into a single list
    this.employeeService.getEmployeesByFilter(params as any, {}).subscribe({
      next: (res: any) => {
        const payload = res?.data ?? res ?? [];
        const list = payload?.items ?? payload ?? [];
        const normalized: any[] = (list || []).map((e: any) => ({
          fullName: e.fullName || e.name,
          code: e.employeeCode ?? e.code ?? e.id,
          employeeCode: e.employeeCode ?? e.code ?? e.id,
          type: 'employee',
          raw: e
        }));

        // load representatives and merge
        this.representativeService.getRepresentativesByFilter(params as any, { representativeCode: undefined, representativeName: undefined, cityName: undefined, isActive: true, representiveType: 0 } as any)
          .subscribe({
            next: (rres: any) => {
              const rpayload = rres?.data ?? rres ?? [];
              const rlist = rpayload?.items ?? rpayload ?? [];
              const reps = (rlist || []).map((r: any) => ({
                fullName: r.fullName || r.name,
                code: r.representativeCode ?? r.representativesCode ?? r.id,
                representativeCode: r.representativeCode ?? r.representativesCode ?? r.id,
                type: 'representative',
                raw: r
              }));

              // merge and dedupe by code
              const map = new Map<string, any>();
              for (const e of [...normalized, ...reps]) {
                const key = (e.code || '').toString();
                if (key && !map.has(key)) map.set(key, e);
              }
              this.employees = Array.from(map.values());
              this.employeesLoading = false;
            },
            error: () => { this.employees = normalized; this.employeesLoading = false; }
          });
      },
      error: () => { this.employees = []; this.employeesLoading = false; }
    });
  }

  get filteredEmployees(): any[] {
    const q = (this.generateEmployeeFilter || '').toString().trim().toLowerCase();
    if (!q) return this.employees.slice(0, 30);
    return this.employees.filter((e: any) => {
      const name = (e.fullName || e.name || '').toString().toLowerCase();
      const code = (e.code || e.employeeCode || e.representativeCode || '').toString().toLowerCase();
      return name.includes(q) || code.includes(q);
    }).slice(0, 30);
  }

  onEmployeeFilterChange(q: string): void {
    this.generateEmployeeFilter = q;
  }

  onEmployeeSelected(event: any): void {
    const emp = event.option.value;
    this.selectEmployee(emp);
  }

  selectEmployee(emp: any): void {
    if (!emp) return;
    this.generateSelectedIsRepresentative = emp.type === 'representative';
    this.generateEmployeeCode = emp.representativeCode ?? emp.employeeCode ?? emp.code ?? emp.id;
    // عرض اسم المستخدم في الـ input
    this.generateEmployeeFilter = emp.fullName || emp.name || this.generateEmployeeCode;
  }

  // ==================== Generate Payroll ====================
  previewSinglePayroll(): void {
    if (!this.generateEmployeeCode) {
      Swal.fire('خطأ', 'يرجى إدخال كود الموظف/المندوب', 'error');
      return;
    }

    if (!this.generateMonth || !this.generateYear) {
      Swal.fire('خطأ', 'يرجى اختيار الشهر والسنة', 'error');
      return;
    }

    this.payrollPreviewLoading = true;

    const request: GeneratePayrollRequestDto = {
      month: this.generateMonth,
      year: this.generateYear,
      payLoansFromSalary: false // لأول مرة نعرض Preview بدون خصم
    } as any;

    if (this.generateSelectedIsRepresentative) {
      (request as any).representativeCode = this.generateEmployeeCode;
    } else {
      (request as any).employeeCode = this.generateEmployeeCode;
    }

    this.payrollService.previewPayroll(request).subscribe({
      next: (res: any) => {
        this.payrollPreviewLoading = false;
        const preview = res?.data ?? res;
        if (preview && preview.employeeCode) {
          this.payrollPreview = preview;
          this.payLoansFromSalary = false; // Reset الـ checkbox
          this.previewShowDialog = true; // عرض الـ popup
        } else {
          Swal.fire('خطأ', res?.message ?? 'فشل في تحميل المعاينة', 'error');
        }
      },
      error: (err) => {
        this.payrollPreviewLoading = false;
        Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ', 'error');
      }
    });
  }

  onPreviewLoanCheckboxChange(): void {
    if (!this.payrollPreview) return;
    // تحديث الراتب الصافي حسب الـ checkbox
    if (this.payLoansFromSalary) {
      this.payrollPreview.deductLoan = true;
    } else {
      this.payrollPreview.deductLoan = false;
    }
  }

  confirmAndGeneratePayroll(): void {
    if (!this.payrollPreview) return;

    this.isLoading = true;

    const request: GeneratePayrollRequestDto = {
      month: this.payrollPreview.month,
      year: this.payrollPreview.year,
      payLoansFromSalary: this.payLoansFromSalary,
      paymentMethodForLoans: 'Cash'
    } as any;

    if (this.payrollPreview.representativeCode) {
      (request as any).representativeCode = this.payrollPreview.representativeCode;
    } else {
      (request as any).employeeCode = this.payrollPreview.employeeCode;
    }

    this.payrollService.generatePayroll(request).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        const isSuccess = res?.isSuccess ?? res?.success ?? res?.id !== undefined;
        if (isSuccess) {
          Swal.fire('نجاح', 'تم إنشاء كشف الراتب بنجاح', 'success');
          this.closePreviewDialog();
          this.generateEmployeeCode = '';
          this.generateEmployeeFilter = '';
          this.loadSummary();
        } else {
          Swal.fire('خطأ', res?.message ?? 'فشل في إنشاء كشف الراتب', 'error');
        }
      },
      error: (err) => {
        this.isLoading = false;
        Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ', 'error');
      }
    });
  }

  closePreviewDialog(): void {
    this.previewShowDialog = false;
    this.payrollPreview = null;
    this.payLoansFromSalary = false;
  }

  generateSinglePayroll(): void {
    // استدعاء Preview بدل التوليد مباشرة
    this.previewSinglePayroll();
  }

  // ==================== Bulk Payroll ====================
  previewBulkPayroll(): void {
    if (!this.bulkMonth || !this.bulkYear) {
      Swal.fire('خطأ', 'يرجى اختيار الشهر والسنة', 'error');
      return;
    }

    this.bulkPayrollPreviewLoading = true;

    const request: GenerateBulkPayrollRequestDto = {
      month: this.bulkMonth,
      year: this.bulkYear,
      userCodes: this.selectedEmployeeCodes.length > 0 ? this.selectedEmployeeCodes : null,
      includeActiveOnly: this.bulkIncludeActiveOnly,
      autoPostToAccounting: this.bulkAutoPost,
      paymentMethod: this.bulkPaymentMethod,
      payLoansFromSalary: this.bulkPayLoansFromSalary,
      confirmLoans: false
    };

    this.payrollService.previewBulkPayroll(request).subscribe({
      next: (res: any) => {
        this.bulkPayrollPreviewLoading = false;
        const preview = res?.data ?? res;
        
        if (preview?.totalEmployees !== undefined) {
          this.bulkPayrollPreview = preview;
          this.bulkPreviewShowDialog = true;
        } else {
          Swal.fire('خطأ', res?.message ?? 'فشل في معاينة الرواتب', 'error');
        }
      },
      error: (err) => {
        this.bulkPayrollPreviewLoading = false;
        Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ في المعاينة', 'error');
      }
    });
  }

  confirmAndGenerateBulkPayroll(): void {
    if (!this.bulkPayrollPreview) return;

    this.isLoading = true;

    const request: GenerateBulkPayrollRequestDto = {
      month: this.bulkMonth,
      year: this.bulkYear,
      userCodes: this.selectedEmployeeCodes.length > 0 ? this.selectedEmployeeCodes : null,
      includeActiveOnly: this.bulkIncludeActiveOnly,
      autoPostToAccounting: this.bulkAutoPost,
      paymentMethod: this.bulkPaymentMethod,
      payLoansFromSalary: this.bulkPayLoansFromSalary,
      confirmLoans: false
    };

    this.payrollService.generateBulkPayroll(request).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        const isSuccess = res?.isSuccess ?? res?.success ?? res?.totalEmployees !== undefined;
        const bulkData = res?.data ?? res;
        
        if (isSuccess && bulkData?.totalEmployees !== undefined) {
          this.bulkResult = bulkData;
          this.closeBulkPreviewDialog();
          Swal.fire('نجاح', `تم إنشاء ${bulkData.processedSuccessfully} كشف مرتب بنجاح`, 'success');
          this.loadSummary();
        } else {
          Swal.fire('خطأ', res?.message ?? 'فشل في الإنشاء الجماعي', 'error');
        }
      },
      error: (err) => {
        this.isLoading = false;
        Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ', 'error');
      }
    });
  }

  closeBulkPreviewDialog(): void {
    this.bulkPreviewShowDialog = false;
    this.bulkPayrollPreview = null;
    this.expandedPayrollIndex = null;
  }

  togglePayrollExpand(index: number): void {
    this.expandedPayrollIndex = this.expandedPayrollIndex === index ? null : index;
  }

  toggleLoanDeduction(index: number, event: any): void {
    if (!this.bulkPayrollPreview || !this.bulkPayrollPreview.successPreviews) return;
    
    const payroll = this.bulkPayrollPreview.successPreviews[index];
    if (payroll) {
      payroll.deductLoan = event.checked;
      // إعادة حساب الرواتب الكلية
      this.recalculateBulkTotals();
    }
  }

  private recalculateBulkTotals(): void {
    if (!this.bulkPayrollPreview) return;
    
    let totalNet = 0;
    this.bulkPayrollPreview.successPreviews.forEach(p => {
      const netSalary = p.deductLoan ? p.netSalaryAfterLoan : p.netSalaryBeforeLoan;
      totalNet += netSalary;
    });
    
    this.bulkPayrollPreview.totalNetSalary = totalNet;
  }

  generateBulkPayroll(): void {
    // استدعاء Preview بدل التوليد مباشرة
    this.previewBulkPayroll();
  }

  // ==================== Payroll List ====================
  loadPayrolls(): void {
    this.isLoading = true;

    const filter: PayrollFilterDto = {
      month: this.filterMonth,
      year: this.filterYear,
      status: this.filterStatus || undefined,
      fromDate: this.filterFromDate,
      toDate: this.filterToDate,
      minNetSalary: this.filterMinSalary,
      maxNetSalary: this.filterMaxSalary,
      departments: this.filterDepartments.length > 0 ? this.filterDepartments : undefined
    };

    this.payrollService.getPayrolls(filter).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        try {
          // التعامل مع البيانات سواء كانت wrapped في Result أو مباشرة
          let payrolls: PayrollResponseDto[] = [];
          
          if (Array.isArray(res)) {
            // البيانات مباشرة كـ array
            payrolls = res;
          } else if (res?.isSuccess && res?.data && Array.isArray(res.data)) {
            // البيانات wrapped في Result
            payrolls = res.data;
          } else if (res?.success && res?.data && Array.isArray(res.data)) {
            // صيغة alternative للـ Result
            payrolls = res.data;
          } else if (res?.message && !Array.isArray(res)) {
            // رسالة خطأ
            Swal.fire('خطأ', res.message, 'error');
            return;
          }

          if (payrolls && payrolls.length > 0) {
            // Status is already a string from API ("Created", "Approved", "Paid", etc.)
            this.allPayrolls = payrolls;
            this.totalPages = Math.ceil(this.allPayrolls.length / this.pageSize);
            this.currentPage = 1;
            this.updatePaginatedList();
          } else {
            Swal.fire('تنبيه', 'لم يتم العثور على بيانات', 'info');
          }
        } catch (error) {
          Swal.fire('خطأ', 'حدث خطأ في معالجة البيانات', 'error');
        }
      },
      error: (err) => {
        this.isLoading = false;
        Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ', 'error');
      }
    });
  }

  updatePaginatedList(): void {
    const startIndex = (this.currentPage - 1) * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.paginatedPayrolls = this.allPayrolls.slice(startIndex, endIndex);
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.totalPages = Math.ceil(this.allPayrolls.length / this.pageSize);
    this.updatePaginatedList();
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.updatePaginatedList();
    }
  }

  resetFilters(): void {
    this.filterMonth = undefined;
    this.filterYear = undefined;
    this.filterStatus = '';
    this.filterFromDate = undefined;
    this.filterToDate = undefined;
    this.filterMinSalary = undefined;
    this.filterMaxSalary = undefined;
    this.filterDepartments = [];
    this.selectedPayrollIds = [];
    this.loadPayrolls();
  }

  exportToExcel(): void {
    if (this.allPayrolls.length === 0) {
      Swal.fire('تنبيه', 'لا توجد بيانات للتصدير', 'warning');
      return;
    }

    this.isLoading = true;

    const filter: PayrollFilterDto = {
      month: this.filterMonth,
      year: this.filterYear,
      status: this.filterStatus || undefined,
      departments: this.filterDepartments.length > 0 ? this.filterDepartments : undefined
    };

    this.payrollService.exportToExcel(filter).subscribe({
      next: (blob: Blob) => {
        this.isLoading = false;
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `كشوف_المرتبات_${new Date().getTime()}.xlsx`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        this.isLoading = false;
        Swal.fire('خطأ', 'فشل في تصدير البيانات', 'error');
      }
    });
  }

  // ==================== Payroll Management ====================
  selectPayroll(payroll: PayrollResponseDto): void {
    this.selectedPayroll = payroll;
  }

  togglePayrollSelection(payrollId: number): void {
    const index = this.selectedPayrollIds.indexOf(payrollId);
    if (index > -1) {
      this.selectedPayrollIds.splice(index, 1);
    } else {
      this.selectedPayrollIds.push(payrollId);
    }
  }

  isPayrollSelected(payrollId: number): boolean {
    return this.selectedPayrollIds.includes(payrollId);
  }

  selectAllPayrolls(): void {
    this.paginatedPayrolls.forEach(payroll => {
      if (this.canSelectPayroll(payroll) && !this.selectedPayrollIds.includes(payroll.id)) {
        this.selectedPayrollIds.push(payroll.id);
      }
    });
  }

  deselectAllPayrolls(): void {
    this.selectedPayrollIds = [];
  }

  // ==================== Delete Payroll ====================
  deletePayroll(payrollId: number): void {
    const payroll = this.allPayrolls.find(p => p.id === payrollId);
    if (!payroll) return;

    Swal.fire({
      title: 'تأكيد الحذف',
      html: `<div style="text-align: right">
              <p>هل تريد حذف كشف الراتب لـ <strong>${payroll.employeeName}</strong>؟</p>
              <p style="color: #f44336; font-weight: bold;">هذه العملية لا يمكن التراجع عنها</p>
            </div>`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'نعم، احذف',
      confirmButtonColor: '#f44336',
      cancelButtonText: 'إلغاء'
    }).then((result) => {
      if (result.isConfirmed) {
        this.isLoading = true;
        this.payrollService.deletePayroll(payrollId).subscribe({
          next: (res: any) => {
            this.isLoading = false;
            if (res?.isSuccess) {
              Swal.fire('تم', 'تم حذف الكشف بنجاح', 'success');
              this.loadPayrolls();
              this.loadSummary();
            } else {
              Swal.fire('خطأ', res?.message ?? 'فشل الحذف', 'error');
            }
          },
          error: (err) => {
            this.isLoading = false;
            Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ', 'error');
          }
        });
      }
    });
  }

  // ==================== Accounting Operations ====================
  postToAccounting(payrollId: number): void {
    const payroll = this.allPayrolls.find(p => p.id === payrollId);
    const hasLoans = payroll?.hasPendingLoans ?? false;

    Swal.fire({
      title: 'تأكيد التسجيل المحاسبي',
      html: hasLoans 
        ? `<div style="text-align: right">
            <p>هل تريد تسجيل هذا الكشف في المحاسبة؟</p>
            <p style="color: var(--gold); font-weight: bold;">هذا الكشف يحتوي على قروض مستحقة</p>
            <label style="margin-top: 15px; display: flex; align-items: center; justify-content: center; gap: 10px;">
              <input type="checkbox" id="confirmLoans" />
              <span id="confirmLoansLabel" >تأكيد سداد القروض من الراتب</span>
            </label>
          </div>`
        : `<p style="text-align: right">هل تريد تسجيل هذا الكشف في المحاسبة؟</p>`,
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: 'تأكيد',
      cancelButtonText: 'إلغاء',
      didOpen: () => {
        if (hasLoans) {
          const checkbox = document.getElementById('confirmLoans') as HTMLInputElement;
          if (checkbox) {
            checkbox.checked = false;
          }
        }
      }
    }).then((result) => {
      if (result.isConfirmed) {
        let confirmLoans = false;
        if (hasLoans) {
          const checkbox = document.getElementById('confirmLoans') as HTMLInputElement;
          confirmLoans = checkbox?.checked ?? false;
        }

        this.isLoading = true;
        this.payrollService.postToAccounting(payrollId, confirmLoans).subscribe({
          next: (res: any) => {
            this.isLoading = false;
            if (res?.isSuccess) {
              Swal.fire('نجاح', res.data ?? 'تم التسجيل بنجاح', 'success');
              this.loadPayrolls();
              this.loadSummary();
            } else {
              Swal.fire('خطأ', res?.message ?? 'فشل التسجيل', 'error');
            }
          },
          error: (err) => {
            this.isLoading = false;
            Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ', 'error');
          }
        });
      }
    });
  }

  postBulkToAccounting(): void {
    if (this.selectedPayrollIds.length === 0) {
      Swal.fire('تنبيه', 'يرجى تحديد كشوف للتسجيل', 'warning');
      return;
    }

    const hasLoansPayroll = this.allPayrolls
      .filter(p => this.selectedPayrollIds.includes(p.id))
      .some(p => p.hasPendingLoans);

    Swal.fire({
      title: 'تأكيد التسجيل المحاسبي الجماعي',
      html: hasLoansPayroll 
        ? `<div style="text-align: right">
            <p>هل تريد تسجيل ${this.selectedPayrollIds.length} كشف في المحاسبة؟</p>
            <p style="color: var(--gold); font-weight: bold;">عدد من الكشوف يحتوي على قروض مستحقة</p>
            <label style="margin-top: 15px; display: flex; align-items: center; justify-content: center; gap: 10px;">
              <input type="checkbox" id="confirmBulkLoans" />
              <span>تأكيد سداد القروض من الرواتب</span>
            </label>
          </div>`
        : `<p style="text-align: right">هل تريد تسجيل ${this.selectedPayrollIds.length} كشف في المحاسبة؟</p>`,
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: 'تأكيد',
      cancelButtonText: 'إلغاء',
      didOpen: () => {
        if (hasLoansPayroll) {
          const checkbox = document.getElementById('confirmBulkLoans') as HTMLInputElement;
          if (checkbox) {
            checkbox.checked = false;
          }
        }
      }
    }).then((result) => {
      if (result.isConfirmed) {
        let confirmLoans = false;
        if (hasLoansPayroll) {
          const checkbox = document.getElementById('confirmBulkLoans') as HTMLInputElement;
          confirmLoans = checkbox?.checked ?? false;
        }

        this.isLoading = true;
        this.payrollService.postBulkToAccounting(this.selectedPayrollIds, confirmLoans).subscribe({
          next: (res: any) => {
            this.isLoading = false;
            if (res?.isSuccess) {
              Swal.fire('نجاح', res.data ?? 'تم التسجيل الجماعي بنجاح', 'success');
              this.selectedPayrollIds = [];
              this.loadPayrolls();
              this.loadSummary();
            } else {
              Swal.fire('خطأ', res?.message ?? 'فشل التسجيل', 'error');
            }
          },
          error: (err) => {
            this.isLoading = false;
            Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ', 'error');
          }
        });
      }
    });
  }

  // Post specific ids (used by selection header buttons)
  postBulkToAccountingForIds(ids: number[]): void {
    if (!ids || ids.length === 0) return;

    const hasLoansPayroll = this.allPayrolls.filter(p => ids.includes(p.id)).some(p => p.hasPendingLoans);

    Swal.fire({
      title: 'تأكيد التسجيل المحاسبي الجماعي',
      html: hasLoansPayroll
        ? `<div style="text-align: right">
            <p>هل تريد تسجيل ${ids.length} كشف في المحاسبة؟</p>
            <p style="color: var(--gold); font-weight: bold;">بعض الكشوف تحتوي على قروض مستحقة</p>
            <label style="margin-top: 15px; display: flex; align-items: center; justify-content: center; gap: 10px;">
              <input type="checkbox" id="confirmBulkLoans" />
              <span >تأكيد سداد القروض من الرواتب</span>
            </label>
          </div>`
        : `<p style="text-align: right">هل تريد تسجيل ${ids.length} كشف في المحاسبة؟</p>`,
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: 'تأكيد',
      cancelButtonText: 'إلغاء',
      didOpen: () => {
        // nothing special
      }
    }).then((result) => {
      if (result.isConfirmed) {
        const confirmLoans = (document.getElementById('confirmBulkLoans') as HTMLInputElement)?.checked ?? false;
        this.payrollService.postBulkToAccounting(ids, confirmLoans).subscribe({
          next: (res: any) => {
            Swal.fire('نجاح', res?.message ?? 'تم التسجيل المحاسبي', 'success');
            this.selectedPayrollIds = this.selectedPayrollIds.filter(id => !ids.includes(id));
            this.loadPayrolls();
            this.loadSummary();
          },
          error: (err) => Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ', 'error')
        });
      }
    });
  }

  // Mark specific ids as paid (used by selection header buttons)
  markBulkAsPaidForIds(ids: number[]): void {
    if (!ids || ids.length === 0) return;

    Swal.fire({
      title: 'تأكيد الدفع',
      text: `هل تريد تعيين ${ids.length} كشف كمدفوع؟`,
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: 'نعم',
      cancelButtonText: 'إلغاء'
    }).then((result) => {
      if (result.isConfirmed) {
        const dto = { payrollIds: ids, paymentMethod: this.bulkPaymentMethod || 'BankTransfer' } as any;
        this.payrollService.markBulkAsPaid(dto).subscribe({
          next: (res: any) => {
            Swal.fire('نجاح', res?.message ?? 'تم التعيين كمدفوع', 'success');
            this.selectedPayrollIds = this.selectedPayrollIds.filter(id => !ids.includes(id));
            this.loadPayrolls();
            this.loadSummary();
          },
          error: (err) => Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ', 'error')
        });
      }
    });
  }

  // Smart combined action: post Created ones, pay Approved ones
  processSelectedActions(): void {
    if (this.selectedPayrollIds.length === 0) {
      Swal.fire('تنبيه', 'يرجى تحديد كشوف أولاً', 'warning');
      return;
    }

    const createdIds = this.selectedCreatedIds;
    const approvedIds = this.selectedApprovedIds;
    const others = this.selectedPayrollIds.filter(id => !createdIds.includes(id) && !approvedIds.includes(id));

    const hasLoans = this.allPayrolls.filter(p => createdIds.includes(p.id)).some(p => p.hasPendingLoans);

    let html = `<div style="text-align: right">`;
    html += `<p>سيتم تنفيذ العمليات التالية على الاختيارات:</p>`;
    if (createdIds.length) html += `<p>تسجيل محاسبي: ${createdIds.length} كشف</p>`;
    if (approvedIds.length) html += `<p>دفع: ${approvedIds.length} كشف</p>`;
    if (others.length) html += `<p>باقي الحالات: ${others.length} (لن يتم تعديلها)</p>`;
    if (hasLoans) {
      html += `<label style="margin-top: 10px; display:flex; gap:8px; align-items:center; justify-content:flex-end;"><input type="checkbox" id="confirmMixedLoans"/> <span>تأكيد سداد القروض من الرواتب</span></label>`;
    }
    html += `</div>`;

    Swal.fire({
      title: 'تأكيد تنفيذ العمليات',
      html,
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: 'تنفيذ',
      cancelButtonText: 'إلغاء'
    }).then((result) => {
      if (!result.isConfirmed) return;
      const confirmLoans = (document.getElementById('confirmMixedLoans') as HTMLInputElement)?.checked ?? false;

      const results: { posted?: number[]; paid?: number[]; errors?: string[] } = {};

      const tasks: Array<Promise<any>> = [];

      if (createdIds.length) {
        tasks.push(new Promise((resolve) => {
          this.payrollService.postBulkToAccounting(createdIds, confirmLoans).subscribe({
            next: (res: any) => { results.posted = createdIds; resolve({ ok: true, res }); },
            error: (err) => { results.errors = results.errors ?? []; results.errors.push(err?.error?.message ?? 'خطأ في التسجيل المحاسبي'); resolve({ ok: false, err }); }
          });
        }));
      }

      if (approvedIds.length) {
        tasks.push(new Promise((resolve) => {
          const dto = { payrollIds: approvedIds, paymentMethod: this.bulkPaymentMethod || 'BankTransfer' } as any;
          this.payrollService.markBulkAsPaid(dto).subscribe({
            next: (res: any) => { results.paid = approvedIds; resolve({ ok: true, res }); },
            error: (err) => { results.errors = results.errors ?? []; results.errors.push(err?.error?.message ?? 'خطأ في الدفع'); resolve({ ok: false, err }); }
          });
        }));
      }

      Promise.all(tasks).then(() => {
        this.loadPayrolls();
        this.loadSummary();
        this.selectedPayrollIds = [];
        let summary = '';
        if (results.posted && results.posted.length) summary += `تم تسجيل محاسبياً: ${results.posted.length} كشف\n`;
        if (results.paid && results.paid.length) summary += `تم دفع: ${results.paid.length} كشف\n`;
        if (results.errors && results.errors.length) summary += `أخطاء: ${results.errors.join('; ')}`;
        Swal.fire('النتيجة', summary || 'تمت العملية', 'info');
      });
    });
  }

  // ==================== Payment Operations ====================
  markAsPaid(payrollId: number, paymentMethod: string): void {
    Swal.fire({
      title: 'تأكيد الدفع',
      text: 'هل تريد تعيين هذا الكشف كمدفوع؟',
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: 'نعم',
      cancelButtonText: 'إلغاء'
    }).then((result) => {
      if (result.isConfirmed) {
        this.isLoading = true;
        this.payrollService.markAsPaid(payrollId, paymentMethod).subscribe({
          next: (res: any) => {
            this.isLoading = false;
            if (res?.isSuccess) {
              Swal.fire('نجاح', res.data ?? 'تم تعيين الكشف كمدفوع', 'success');
              this.loadPayrolls();
              this.loadSummary();
            } else {
              Swal.fire('خطأ', res?.message ?? 'فشل التحديث', 'error');
            }
          },
          error: (err) => {
            this.isLoading = false;
            Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ', 'error');
          }
        });
      }
    });
  }

  markBulkAsPaid(): void {
    if (this.selectedPayrollIds.length === 0) {
      Swal.fire('تنبيه', 'يرجى تحديد كشوف للدفع', 'warning');
      return;
    }

    const paymentMethod = this.bulkPaymentMethod || 'BankTransfer';

    Swal.fire({
      title: 'تأكيد الدفع',
      text: `هل تريد تعيين ${this.selectedPayrollIds.length} كشف كمدفوع؟`,
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: 'نعم',
      cancelButtonText: 'إلغاء'
    }).then((result) => {
      if (result.isConfirmed) {
        this.isLoading = true;
        const dto: MarkPayrollPaidDto = {
          payrollIds: this.selectedPayrollIds,
          paymentMethod: paymentMethod
        };

        this.payrollService.markBulkAsPaid(dto).subscribe({
          next: (res: any) => {
            this.isLoading = false;
            if (res?.isSuccess) {
              Swal.fire('نجاح', res.data ?? 'تم تعيين الكشوف كمدفوع', 'success');
              this.selectedPayrollIds = [];
              this.loadPayrolls();
              this.loadSummary();
            } else {
              Swal.fire('خطأ', res?.message ?? 'فشل التحديث', 'error');
            }
          },
          error: (err) => {
            this.isLoading = false;
            Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ', 'error');
          }
        });
      }
    });
  }

  // ==================== Summary ====================
  loadSummary(): void {
    this.payrollService.getSummary(this.selectedMonth, this.selectedYear).subscribe({
      next: (res: any) => {
        // التعامل مع البيانات سواء كانت wrapped أو مباشرة
        const isSuccess = res?.isSuccess ?? res?.success ?? res?.month !== undefined;
        const summaryData = res?.data ?? res;
        
        if (isSuccess && summaryData?.month !== undefined) {
          this.summary = summaryData;
        }
      },
      error: (err) => console.error('Summary error:', err)
    });
  }

  onMonthChange(): void {
    this.loadSummary();
  }

  onYearChange(): void {
    this.loadSummary();
  }

  // ==================== Formatting Utilities ====================
  formatCurrency(value: number | null | undefined): string {
    if (!value && value !== 0) {
      return '0 ج.م';
    }
    // Format with 2 decimal places only
    return value.toLocaleString('ar-EG', { 
      style: 'currency', 
      currency: 'EGP',
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    });
  }

  formatDate(date: Date | string | null | undefined): string {
    if (!date) return 'لم يتم الدفع';
    const d = new Date(date);
    return d.toLocaleDateString('ar-EG');
  }

 
    getStatusColor(status: string): string {
      // Status is always a string from API ("Created", "Approved", "Paid", etc.)
      switch (status) {
        case PayrollStatus.Created:
          return '#ff9800'; // برتقالي - الإنشاء
        case PayrollStatus.Approved:
          return '#2196f3'; // أزرق - الموافقة
        case PayrollStatus.Paid:
          return '#4caf50'; // أخضر - الدفع
        case PayrollStatus.Rejected:
          return '#f44336'; // أحمر - المرفوض
        case PayrollStatus.Cancelled:
          return '#9e9e9e'; // رمادي - الملغى
        default:
          return '#666';
      }
    }


    getStatusText(status: string): string {
      // Status is always a string from API ("Created", "Approved", "Paid", etc.)
      switch (status) {
        case PayrollStatus.Created:
          return 'تم الإنشاء';
        case PayrollStatus.Approved:
          return 'تمت الموافقة';
        case PayrollStatus.Paid:
          return 'تم الدفع';
        case PayrollStatus.Rejected:
          return 'مرفوض';
        case PayrollStatus.Cancelled:
          return 'ملغى';
        default:
          return 'غير محدد';
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
    this.router.navigate(['/hr']);
  }

  printPayroll(): void {
    window.print();
  }
}

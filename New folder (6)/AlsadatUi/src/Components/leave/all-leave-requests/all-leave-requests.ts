import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatCardModule } from '@angular/material/card';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import Swal from 'sweetalert2';
import { EmployeeLeaveService } from '../../../app/Services/employee-leave.service';
import { EmployeeLeaveRequestDto, ApproveRejectLeaveDto } from '../../../app/models/leave/employee-leave-request.model';

import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment.development';
import { EmployeeService } from '../../../app/Services/employee.service';
import { RepresentativeService } from '../../../app/Services/representative-service';
import { PaginationParams } from '../../../app/models/IEmployee';

@Component({
  selector: 'app-all-leave-requests',
  standalone: true,
  imports: [CommonModule, FormsModule, MatTableModule, MatPaginatorModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatDatepickerModule, MatNativeDateModule, MatCardModule, MatCheckboxModule, MatAutocompleteModule, MatButtonToggleModule],
  templateUrl: './all-leave-requests.html',
  styleUrls: ['./all-leave-requests.css']
})
export class AllLeaveRequestsComponent implements OnInit {
  private leaveService = inject(EmployeeLeaveService);
  private http = inject(HttpClient);
  private employeeService = inject(EmployeeService);
  private representativeService = inject(RepresentativeService);

  items: EmployeeLeaveRequestDto[] = [];
  isLoading = false;
  pageNumber = 1;
  pageSize = 20;
  totalCount = 0;

  filters: any = {
    employeeCode: '',
    representativeCode: '',
    leaveTypeId: null,
    status: null,
    fromDate: null,
    toDate: null
  };

  // user type: 'employee' | 'representative'
  userType: 'employee' | 'representative' = 'employee';

  leaveTypes: any[] = [];

  // employee autocomplete
  employees: any[] = [];
  representatives: any[] = [];
  employeeFilter = '';
  employeesLoading = false;
  representativesLoading = false;
  _selectedEmployee: any = null;
  pagination: PaginationParams = { pageNumber: 1, pageSize: 1000 };

  selectedIds = new Set<number>();

  // @ViewChild(MatPaginator) paginator!: MatPaginator;

  ngOnInit(): void {
    this.loadLeaveTypes();
    this.loadAllEmployees();
    this.loadAllRepresentatives();
    this.load();
  }

  private normalizeToArray(res: any): any[] {
    if (!res) return [];
    if (Array.isArray(res)) return res;
    if (Array.isArray(res.items)) return res.items;
    if (Array.isArray(res.data)) return res.data;
    if (Array.isArray(res.data?.items)) return res.data.items;
    // if it's an object whose values are the list (rare), try that
    const vals = Object.values(res || {});
    if (vals.length > 0 && Array.isArray(vals[0])) return vals[0];
    return [];
  }

  private loadLeaveTypes() {
    this.http.get<any>(`${environment.apiUrl}LeaveType/GetAllLeaveTypes`).subscribe({
      next: (res) => { this.leaveTypes = this.normalizeToArray(res); },
      error: () => { this.leaveTypes = []; }
    });
  }

  async loadAllEmployees(): Promise<void> {
    this.employeesLoading = true;
    return new Promise((resolve) => {
      this.employeeService.getEmployeesByFilter(this.pagination, {}).subscribe({
        next: (res: any) => {
          let loaded: any[] = [];
          if (res?.items) loaded = res.items;
          else if (res?.data?.items) loaded = res.data.items;
          else if (Array.isArray(res)) loaded = res;
          else if (res?.data && Array.isArray(res.data)) loaded = res.data;

          this.employees = this.removeDuplicates(loaded);
          this.employeesLoading = false;
          resolve();
        },
        error: () => { this.employees = []; this.employeesLoading = false; resolve(); }
      });
    });
  }

async loadAllRepresentatives(): Promise<void> {
    return new Promise((resolve) => {
      const paginationWithDefaults = {
        ...this.pagination,
        pageNumber: this.pagination?.pageNumber ?? 1,
        pageSize: this.pagination?.pageSize ?? 10
      };

      this.representativeService.getRepresentativesByFilter(paginationWithDefaults, { representativeCode: '', representativeName: '', cityName: '', isActive: true, representiveType: 0 }).subscribe({
        next: (res: any) => {
          let loaded: any[] = [];
          if (res?.items) loaded = res.items;
          else if (res?.data?.items) loaded = res.data.items;
          else if (Array.isArray(res)) loaded = res;
          else if (res?.data && Array.isArray(res.data)) loaded = res.data;

          this.representatives = this.removeDuplicates(loaded);
              // normalize representatives to employee-like shape so templates can reuse same fields
              this.representatives = this.representatives.map((r: any) => ({
                ...r,
                employeeCode: r.representativeCode || r.code || r.userId || r.id || r.representativeId || '',
                fullName: r.fullName || r.name || r.user?.fullName || r.user?.name || ''
              }));
          resolve();
        },
        error: () => { this.representatives = []; resolve(); }
      });
    });
  }



removeDuplicates(employees: any[]): any[] {
    const unique: any[] = [];
    const map = new Map();
    for (const e of employees) {
      const key = e.employeeCode || e.code || e.representativeCode || e.representiveCode || e.userId || e.id || e.representativeId;
      if (key && !map.has(key)) { map.set(key, true); unique.push(e); }
    }
    return unique;
  }

 get filteredEmployees(): any[] {
      const list = this.userType === 'employee' ? this.employees : this.representatives;
      if (!this.employeeFilter || this.employeeFilter.trim().length === 0) return list.slice(0,20);
      const q = this.employeeFilter.toLowerCase().trim();
      return list.filter((e: any) => {
        const name = (e.fullName || e.name || '').toLowerCase();
        const code = (e.employeeCode || e.code || e.representativeCode || e.representiveCode || '').toString().toLowerCase();
        return name.includes(q) || code.includes(q);
      }).slice(0,20);
  }

  formatPerson(e: any): string {
    const name = e?.employeeName || e?.representativeName || e?.fullName || e?.name || e?.user?.fullName || e?.user?.name || '';
    const code = e?.employeeCode || e?.code || e?.representativeCode || e?.representiveCode || e?.userId || e?.id || e?.representativeId || '';
    if (name && code) return `${name} — ${code}`;
    if (name) return name;
    return code || '';
  }

  set selectedEmployee(value: any) {
    this._selectedEmployee = value;
    if (value) {
      if (this.userType === 'employee') {
        this.filters.employeeCode = value.employeeCode || value.code || '';
        this.filters.representativeCode = '';
        this.filters.employeeEmail = value.email || value.employeeEmail || this.filters.employeeEmail;
      } else {
        this.filters.representativeCode = value.employeeCode || value.code || value.userId || value.id || '';
        this.filters.employeeCode = '';
      }
    }
  }
  get selectedEmployee() { return this._selectedEmployee; }



  onEmployeeFilterChange(q: string) {
    this.employeeFilter = q;
    if (!q || q.trim().length === 0) { this.selectedEmployee = null; this.filters.employeeCode = ''; }
  }

  onEmployeeSelected(event: any) {
    const selectedValue = event.option.value;
    const list = this.userType === 'employee' ? this.employees : this.representatives;
    const emp = list.find((e: any) => (e.employeeCode && e.employeeCode === selectedValue) || (e.code && e.code === selectedValue) || (e.representativeCode && e.representativeCode === selectedValue) || (e.representiveCode && e.representiveCode === selectedValue) || (e.userId && e.userId === selectedValue) || (e.id && e.id === selectedValue));
    if (emp) {
      this.selectedEmployee = emp;
      const empName = emp.fullName || emp.name || emp.user?.fullName || '';
      const empCode = emp.employeeCode || emp.code || emp.representativeCode || emp.representiveCode || '';
      this.employeeFilter = `${empName} (${empCode})`;
    }
  }
  load(page: number = 1) {
    this.isLoading = true;
    const dto: any = {
      pageNumber: page,
      pageSize: this.pageSize,
      employeeCode: this.filters.employeeCode || undefined,
      representativeCode: this.filters.representativeCode || undefined,
      leaveTypeId: this.filters.leaveTypeId != null ? this.filters.leaveTypeId : undefined,
      status: this.filters.status != null ? this.filters.status : undefined,
      fromDate: this.normalizeDate(this.filters.fromDate),
      toDate: this.normalizeDate(this.filters.toDate)
    };

    this.leaveService.searchLeaveRequests(dto).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        let arr = this.normalizeToArray(res?.items ?? res?.data ?? res);
        // normalize each item so representatives display like employees in the table
        arr = (arr || []).map((it: any) => {
          const name = it.employeeName || it.representativeName || it.fullName || it.name || (it.user && (it.user.fullName || it.user.name)) || '';
          const code = it.employeeCode || it.representativeCode || it.code || it.userId || it.id || it.representativeId || '';
          return { ...it, employeeName: name, fullName: name, employeeCode: code };
        });
        this.items = arr;
        this.totalCount = res?.totalCount ?? res?.data?.totalCount ?? res?.items?.length ?? (Array.isArray(res?.data) ? res.data.length : (arr.length ?? 0));
      },
      error: (err) => { this.isLoading = false; Swal.fire('خطأ', err?.error?.message ?? 'فشل تحميل الطلبات', 'error'); }
    });
  }

  applyFilters() { this.pageNumber = 1; this.load(1); }
  clearFilters() { this.filters = { employeeCode: '', representativeCode: '', leaveTypeId: null, status: null, fromDate: null, toDate: null }; this.pageNumber = 1; this.load(1); }

  onPageChange(e: PageEvent) { this.pageSize = e.pageSize; this.pageNumber = e.pageIndex + 1; this.load(this.pageNumber); }

  private normalizeDate(d: any): string | undefined {
    if (!d) return undefined;
    const dt = (d instanceof Date) ? d : new Date(d);
    if (isNaN(dt.getTime())) return undefined;
    return dt.toISOString().split('T')[0];
  }

  toggleSelect(id: number, checked: boolean) { if (checked) this.selectedIds.add(id); else this.selectedIds.delete(id); }

  approve(item: EmployeeLeaveRequestDto) {
    Swal.fire({ title: 'تأكيد', text: 'هل تريد الموافقة على الطلب؟', icon: 'question', showCancelButton: true }).then(r => {
      if (r.isConfirmed) {
        const dto: ApproveRejectLeaveDto = { leaveRequestId: item.id, reason: '' };
        this.leaveService.approveLeaveRequest(item.id, dto, item.employeeCode || '').subscribe({ next: () => { Swal.fire('تم', 'تمت الموافقة', 'success'); this.load(this.pageNumber); }, error: (e) => Swal.fire('خطأ', e?.error?.message ?? 'فشل العملية', 'error') });
      }
    });
  }

  reject(item: EmployeeLeaveRequestDto) {
    Swal.fire({ title: 'سبب الرفض', input: 'text' }).then(r => {
      if (r.value !== undefined) {
        const dto: ApproveRejectLeaveDto = { leaveRequestId: item.id, reason: r.value };
        this.leaveService.rejectLeaveRequest(item.id, dto, item.employeeCode || '').subscribe({ next: () => { Swal.fire('تم', 'تم الرفض', 'success'); this.load(this.pageNumber); }, error: (e) => Swal.fire('خطأ', e?.error?.message ?? 'فشل العملية', 'error') });
      }
    });
  }

  cancel(item: EmployeeLeaveRequestDto) {
    Swal.fire({ title: 'تأكيد الإلغاء', text: 'هل تريد إلغاء الطلب؟', icon: 'warning', showCancelButton: true }).then(r => {
      if (r.isConfirmed) {
        this.leaveService.cancelLeaveRequest(item.id, item.employeeCode || '').subscribe({ next: () => { Swal.fire('تم', 'تم الإلغاء', 'success'); this.load(this.pageNumber); }, error: (e) => Swal.fire('خطأ', e?.error?.message ?? 'فشل العملية', 'error') });
      }
    });
  }

  approveSelected() {
    const ids = Array.from(this.selectedIds);
    if (ids.length === 0) { Swal.fire('تنبيه', 'لم تختر أي طلبات', 'info'); return; }
    Swal.fire({ title: 'تأكيد', text: 'هل تريد الموافقة على الطلبات المحددة؟', icon: 'question', showCancelButton: true }).then(r => {
      if (r.isConfirmed) {
        this.leaveService.bulkApproveRequests(ids, '').subscribe({ next: () => { Swal.fire('تم', 'تمت الموافقة على الطلبات', 'success'); this.selectedIds.clear(); this.load(this.pageNumber); }, error: (e) => Swal.fire('خطأ', e?.error?.message ?? 'فشل العملية', 'error') });
      }
    });
  }

  getStatusText(e: EmployeeLeaveRequestDto): string {
    const s = e?.status;
    switch (s) {
      case 1: return 'معلق';
      case 2: return 'معتمد';
      case 3: return 'مرفوض';
      case 4: return 'ملغى';
      default: return e?.statusName ?? '-';
    }
  }
}

import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment.development';
import { LeaveTypeDto } from '../../../app/models/leave/leave-type.model';
import Swal from 'sweetalert2';
import { LeaveTypeDialogComponent } from './leave-type-dialog/leave-type-dialog.component';
import { MatCard, MatCardContent } from "@angular/material/card";
import { MatProgressSpinner } from "@angular/material/progress-spinner";

@Component({
  selector: 'app-leave-types',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule, MatIconModule, MatDialogModule, MatPaginatorModule, FormsModule, MatCardModule, MatTooltipModule, MatProgressSpinner],
  templateUrl: './leave-types.component.html',
  styleUrls: ['./leave-types.component.css']
})
export class LeaveTypesComponent implements OnInit {
  private http = inject(HttpClient);
  private dialog = inject(MatDialog);

  columns = ['name','isPaid','actions'];
  items: LeaveTypeDto[] = [];
  pageNumber = 1;
  pageSize = 20;
  total = 0;
  isLoading = false;

  ngOnInit(): void { this.load(); }

  load() {
    this.isLoading = true;
    let params = new HttpParams().set('pageNumber', String(this.pageNumber)).set('pageSize', String(this.pageSize));
    this.http.get<any>(`${environment.apiUrl}LeaveType/GetAllLeaveTypes`, { params }).subscribe({
      next: (res) => {
        const payload = res?.data ?? res ?? res;
        this.items = payload.items ?? payload.data ?? payload;
        this.total = Number(payload.totalCount ?? payload.total ?? this.items.length) || 0;
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; Swal.fire('خطأ','فشل جلب الأنواع','error'); }
    });
  }

  onPage(e: PageEvent) {
    this.pageNumber = e.pageIndex + 1;
    this.pageSize = e.pageSize;
    this.load();
  }

  openCreate() {
    const ref = this.dialog.open(LeaveTypeDialogComponent, { data: { mode: 'create' }, width: '420px' });
    ref.afterClosed().subscribe(r => { if (r) this.create(r); });
  }

  openEdit(item: LeaveTypeDto) {
    const ref = this.dialog.open(LeaveTypeDialogComponent, { data: { mode: 'edit', model: item }, width: '420px' });
    ref.afterClosed().subscribe(r => { if (r) this.update(item.id, r); });
  }

  create(dto: Partial<LeaveTypeDto>) {
    this.http.post(`${environment.apiUrl}LeaveType/AddLeaveType`, dto, { responseType: 'text' as 'json' }).subscribe({
      next: () => { Swal.fire('تم','تمت الإضافة','success'); this.load(); },
      error: (e) => Swal.fire('خطأ', e?.error?.message ?? 'فشل الإضافة','error')
    });
  }

  update(id: number, dto: Partial<LeaveTypeDto>) {
    this.http.put(`${environment.apiUrl}LeaveType/UpdateLeaveType?id=${id}`, dto, { responseType: 'text' as 'json' }).subscribe({
      next: () => { Swal.fire('تم','تم التحديث','success'); this.load(); },
      error: (e) => Swal.fire('خطأ', e?.error?.message ?? 'فشل التحديث','error')
    });
  }

  softDelete(id: number) {
    Swal.fire({ title: 'تأكيد', text: 'هل تريد حذف النوع؟', showCancelButton: true }).then(r => {
      if (!r.isConfirmed) return;
      this.http.delete(`${environment.apiUrl}LeaveType/SoftDeleteLeaveType?id=${id}`, { responseType: 'text' as 'json' }).subscribe({
        next: () => { Swal.fire('تم','تم الحذف','success'); this.load(); },
        error: (e) => Swal.fire('خطأ', e?.error?.message ?? 'فشل الحذف','error')
      });
    });
  }

  restore(id: number) {
    this.http.put(`${environment.apiUrl}LeaveType/RestoreLeaveType?id=${id}`, null, { responseType: 'text' as 'json' }).subscribe({
      next: () => { Swal.fire('تم','تمت الاستعادة','success'); this.load(); },
      error: (e) => Swal.fire('خطأ', e?.error?.message ?? 'فشل الاستعادة','error')
    });
  }

  checkUsage(id: number) {
    this.http.get<any>(`${environment.apiUrl}LeaveType/CheckLeaveTypeUsage`, { params: new HttpParams().set('id', String(id)) }).subscribe({
      next: (r) => Swal.fire('معلومة', r?.data ? 'هذا النوع مستخدم' : 'غير مستخدم','info'),
      error: () => Swal.fire('خطأ','فشل التحقق','error')
    });
  }
}

import { Component, ViewChild, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, PageEvent, MatPaginatorModule } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { EmployeeLeaveService } from '../../../app/Services/employee-leave.service';
import { EmployeeLeaveRequestDto, LeaveRequestFilterDto } from '../../../app/models/leave/employee-leave-request.model';
import { PaginationParams, PagedList } from '../../../app/models/IEmployee';
import Swal from 'sweetalert2';
import { CreateLeaveRequestComponent } from '../create-leave-request/create-leave-request';
import { MatCard, MatCardContent } from "@angular/material/card";

@Component({
  selector: 'app-leave-requests',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatPaginatorModule, MatButtonModule, MatIconModule, MatTooltipModule, FormsModule, RouterModule],
  templateUrl: './leave-requests-component.html',
  styleUrls: ['./leave-requests-component.css']
})
export class LeaveRequestsComponent implements OnInit {
  private leaveService = inject(EmployeeLeaveService);
  private dialog = inject(MatDialog);

  columns = ['leaveTypeName', 'fromDate', 'toDate', 'daysRequested', 'statusName', 'actions'];
  dataSource = new MatTableDataSource<EmployeeLeaveRequestDto>([]);
  totalCount = 0;
  isLoading = false;

  paginationParams: PaginationParams = { pageNumber: 1, pageSize: 10 };

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  ngOnInit(): void { this.load(); }

  load(): void {
    this.isLoading = true;
    const email = localStorage.getItem('userEmail') || '';
    this.leaveService.getMyLeaveRequests(this.paginationParams, email).subscribe({
      next: (res: PagedList<EmployeeLeaveRequestDto>) => {
        const items = (res as any).items ?? (res as any).data ?? res as any;
        this.dataSource = new MatTableDataSource(items || []);
        try {
          this.paginator.pageIndex = Math.max(((res as any).currentPage || this.paginationParams.pageNumber) - 1, 0);
          this.paginator.length = Number((res as any).totalCount ?? (res as any).total ?? items.length) || 0;
          this.paginator.pageSize = (res as any).pageSize || this.paginationParams.pageSize || 10;
        } catch (e) { /* ignore */ }
        this.totalCount = Number((res as any).totalCount ?? items.length) || 0;
        this.isLoading = false;
      },
      error: (err) => { this.isLoading = false; Swal.fire('خطأ', err?.error?.message ?? 'فشل تحميل الطلبات','error'); }
    });
  }

  onPageChange(event: PageEvent) {
    this.paginationParams.pageNumber = event.pageIndex + 1;
    this.paginationParams.pageSize = event.pageSize;
    this.load();
  }

  cancel(request: EmployeeLeaveRequestDto) {
    if (!request || !request.id) return;
    Swal.fire({ title: 'تأكيد', text: 'هل تريد إلغاء الطلب؟', showCancelButton: true }).then(r => {
      if (r.isConfirmed) {
        this.leaveService.cancelLeaveRequest(request.id, request.employeeCode).subscribe({
          next: () => { Swal.fire('تم','تم إلغاء الطلب','success'); this.load(); },
          error: (err) => Swal.fire('خطأ', err?.error?.message ?? 'فشل الإلغاء','error')
        });
      }
    });
  }

  openCreateDialog() {
    const email = localStorage.getItem('userEmail') || '';
    const dialogRef = this.dialog.open(CreateLeaveRequestComponent, {
      width: '720px',
      data: { employeeEmail: email }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === 'saved') this.load();
    });
  }
}

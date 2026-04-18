import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment.development';
import { EmployeeLeaveRequestDto, ApproveRejectLeaveDto } from '../../../app/models/leave/employee-leave-request.model';
import { EmployeeLeaveService } from '../../../app/Services/employee-leave.service';
import Swal from 'sweetalert2';
import { MatPaginator, PageEvent } from "@angular/material/paginator";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-pending-requests',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule, MatIconModule, MatPaginator, MatProgressSpinnerModule, MatCardModule, MatTooltipModule],
  templateUrl: './pending-leave-requests.html',
  styleUrls: ['./pending-leave-requests.css']
})
export class PendingLeaveRequestsComponent implements OnInit {
  private leaveService = inject(EmployeeLeaveService);
  private http = inject(HttpClient);

  items: EmployeeLeaveRequestDto[] = [];
  isLoading = false;
  totalCount = 0;
  pageSize = 10;

  ngOnInit(): void { this.load(); }

  load() {
    this.isLoading = true;
    this.leaveService.getPendingRequests().subscribe({
      next: (res: any) => {
        this.isLoading = false;
        let arr: any[] = [];
        if (Array.isArray(res)) arr = res;
        else if (res?.items) arr = res.items;
        else if (res?.data) arr = Array.isArray(res.data) ? res.data : (res.data.items ?? []);

        arr = (arr || []).map((it: any) => {
          const name = it.employeeName || it.representativeName || it.fullName || it.name || it.user?.fullName || it.user?.name || '';
          const code = it.employeeCode || it.representativeCode || it.code || it.userId || it.id || it.representativeId || '';
          return { ...it, employeeName: name || code, fullName: name, employeeCode: code };
        });

        this.items = arr;
        this.totalCount = arr.length || 0;
      },
      error: (e) => { this.isLoading = false; Swal.fire('خطأ','فشل تحميل القوائم','error'); }
    });
  }

  approve(item: EmployeeLeaveRequestDto) {
    const dto: ApproveRejectLeaveDto = { leaveRequestId: item.id, reason: '' };
    this.leaveService.approveLeaveRequest(item.id, dto, item.employeeCode || '').subscribe({ next: () => { Swal.fire('تم','تمت الموافقة','success'); this.load(); }, error: (e) => Swal.fire('خطأ','فشل العملية','error') });
  }

  reject(item: EmployeeLeaveRequestDto) {
    Swal.fire({ title: 'سبب الرفض', input: 'text', inputPlaceholder: 'السبب' }).then(r => {
      if (r.value !== undefined) {
        const dto: ApproveRejectLeaveDto = { leaveRequestId: item.id, reason: r.value };
        this.leaveService.rejectLeaveRequest(item.id, dto, '').subscribe({ next: () => { Swal.fire('تم','تم الرفض','success'); this.load(); }, error: () => Swal.fire('خطأ','فشل العملية','error') });
      }
    });
  }

  onPageChange(event: any): void {
    // Pagination handler
  }
}

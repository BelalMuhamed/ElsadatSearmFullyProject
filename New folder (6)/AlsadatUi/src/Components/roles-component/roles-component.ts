import { Component, OnInit, inject, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatPaginator } from '@angular/material/paginator';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from '../../app/Services/auth-service';
import { RoleDTO } from '../../app/models/IAuthModels';
import { RoleDialogComponent } from './role-dialog/role-dialog.component';
import { MatSlideToggle } from '@angular/material/slide-toggle';
import Swal from 'sweetalert2';
import { MatCard, MatCardContent } from "@angular/material/card";

@Component({
  selector: 'app-roles-component',
  standalone: true,
  imports: [CommonModule, FormsModule, MatTableModule, MatPaginatorModule, MatButtonModule, MatIconModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatCard, MatCardContent, MatSlideToggleModule, MatTooltipModule],
  templateUrl: './roles-component.html',
  styleUrls: ['./roles-component.css']
})
export class RolesComponent implements OnInit, AfterViewInit {
  private authService = inject(AuthService);
  private dialog = inject(MatDialog);
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  roles: RoleDTO[] = [];
  dataSource = new MatTableDataSource<RoleDTO>([]);
  displayedColumns = ['roleName','createdAt','status','actions'];
  pageSizeOptions = [5,10,25,50];
  isLoading = false;

  ngOnInit(): void {
    this.loadRoles();
  }

  ngAfterViewInit(): void {
    try { this.dataSource.paginator = this.paginator; } catch { /* ignore during server-side rendering */ }
  }

  toggleRoleStatus(role: RoleDTO): void {
    if (!role) return;
    if (role.isDeleted) {
      this.authService.restoreRole(role.roleID).subscribe({ next: () => { role.isDeleted = false; Swal.fire('تم', 'تم إستعادة الدور', 'success'); this.loadRoles(); }, error: (e) => Swal.fire('خطأ', e?.message ?? 'فشل', 'error') });
    } else {
      this.authService.softDeleteRole(role.roleID).subscribe({ next: () => { role.isDeleted = true; Swal.fire('تم', 'تم تعطيل الدور', 'success'); this.loadRoles(); }, error: (e) => Swal.fire('خطأ', e?.message ?? 'فشل', 'error') });
    }
  }

  loadRoles(): void {
    this.isLoading = true;
    this.authService.getAllRoles().subscribe({
      next: (res: any) => {
        if (res?.isSuccess) {
          this.roles = res.data ?? [];
          this.dataSource.data = this.roles;
        } else {
          Swal.fire('خطأ', res?.message ?? 'فشل تحميل الأدوار', 'error');
        }
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        // error handled silently
        Swal.fire('خطأ', err?.message ?? 'حدث خطأ', 'error');
      }
    });
  }

  openAdd(): void {
    const ref = this.dialog.open(RoleDialogComponent, { data: null, width: '420px' });
    ref.afterClosed().subscribe((saved: any) => { if (saved) this.loadRoles(); });
  }

  openEdit(role: RoleDTO): void {
    const ref = this.dialog.open(RoleDialogComponent, { data: role, width: '420px' });
    ref.afterClosed().subscribe((saved: any) => { if (saved) this.loadRoles(); });
  }

  softDelete(role: RoleDTO): void {
    Swal.fire({ title: 'تأكيد', text: `هل تريد تعطيل الدور ${role.roleName}?`, icon: 'warning', showCancelButton: true }).then(r => {
      if (r.isConfirmed) {
        this.authService.softDeleteRole(role.roleID).subscribe({ next: () => { Swal.fire('تم', 'تم تعطيل الدور', 'success'); this.loadRoles(); }, error: (e) => Swal.fire('خطأ', e?.message ?? 'فشل', 'error') });
      }
    });
  }

  restore(role: RoleDTO): void {
    Swal.fire({ title: 'تأكيد', text: `هل تريد استعادة الدور ${role.roleName}?`, icon: 'question', showCancelButton: true }).then(r => {
      if (r.isConfirmed) {
        this.authService.restoreRole(role.roleID).subscribe({ next: () => { Swal.fire('تم', 'تم استعادة الدور', 'success'); this.loadRoles(); }, error: (e) => Swal.fire('خطأ', e?.message ?? 'فشل', 'error') });
      }
    });
  }

  hardDelete(role: RoleDTO): void {
    Swal.fire({ title: 'حذف نهائي', text: `هل تريد حذف الدور نهائياً ${role.roleName}?`, icon: 'warning', showCancelButton: true }).then(r => {
      if (r.isConfirmed) {
        this.authService.hardDeleteRole(role.roleID).subscribe({ next: () => { Swal.fire('تم', 'تم الحذف نهائياً', 'success'); this.loadRoles(); }, error: (e) => Swal.fire('خطأ', e?.message ?? 'فشل', 'error') });
      }
    });
  }
}

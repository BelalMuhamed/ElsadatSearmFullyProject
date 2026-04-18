import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { Department } from '../../../app/models/IDepartment';
import { DepartmentService } from '../../../app/Services/department.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-department-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatCardModule],
  templateUrl: './department-dialog.component.html',
  styleUrls: ['./department-dialog.component.css']
})
export class DepartmentDialogComponent implements OnInit {
  department: Department = {
    id: 0,
    name: '',
    createdAt: new Date(),
    isDeleted: false
  };
  isLoading = false;
  isEdit = false;

  constructor(
    public dialogRef: MatDialogRef<DepartmentDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Department | null,
    private departmentService: DepartmentService
  ) {
    // incoming dialog data (add/edit)
    if (data) {
      // merge incoming data, keeping defaults for missing fields
      this.department = { ...this.department, ...data };
      // Consider it's an edit when an id property exists and is a positive number
      const incomingId = (data as any).id;
      this.isEdit = incomingId != null && !isNaN(Number(incomingId)) && Number(incomingId) > 0;
    }
  }

  ngOnInit(): void {}

  save(): void {
    if (!this.department.name || this.department.name.trim() === '') {
      Swal.fire('خطأ', 'الرجاء إدخال اسم القسم', 'error');
      return;
    }

    this.isLoading = true;

    // saving department (add/edit)

    const request = this.isEdit
      ? this.departmentService.update(this.department)
      : this.departmentService.create(this.department);

    request.subscribe({
      next: (res: any) => {
        this.isLoading = false;
        if (res?.isSuccess) {
          Swal.fire('نجح', res?.message ?? 'تم الحفظ بنجاح', 'success');
          this.dialogRef.close(true);
        } else {
          Swal.fire('خطأ', res?.message ?? 'حدث خطأ', 'error');
        }
      },
      error: (err) => {
        this.isLoading = false;
        Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ', 'error');
      }
    });
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}

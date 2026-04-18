import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { LeaveTypeDto } from '../../../../app/models/leave/leave-type.model';
import { MatCard, MatCardTitle, MatCardActions, MatCardModule } from "@angular/material/card";
import { MatIcon, MatIconModule } from "@angular/material/icon";

@Component({
  selector: 'app-leave-type-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, FormsModule, MatFormFieldModule, MatInputModule, MatCheckboxModule, MatButtonModule, MatCardModule, MatIconModule],
  templateUrl: './leave-type-dialog.component.html',
  styleUrls: ['./leave-type-dialog.component.css']
})
export class LeaveTypeDialogComponent {
  dialogRef = inject(MatDialogRef);
  data = inject(MAT_DIALOG_DATA) as any;
  model: Partial<LeaveTypeDto> = { name: '', isPaid: false };
isLoading: unknown;

  constructor() {
    if (this.data?.mode === 'edit' && this.data?.model) {
      this.model = { ...this.data.model };
    }
  }

  save() {
    if (!this.model.name || this.model.name.trim().length === 0) return;
    this.dialogRef.close(this.model);
  }

  cancel() {
    this.dialogRef.close(null);
  }
}

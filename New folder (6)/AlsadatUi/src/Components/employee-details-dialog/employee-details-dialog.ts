import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { EmployeeDTo } from '../../app/models/IEmployee';

@Component({
  selector: 'app-employee-details-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule, MatCardModule],
  templateUrl: './employee-details-dialog.html',
  styleUrls: ['./employee-details-dialog.css']
})
export class EmployeeDetailsDialog {
  constructor(
    public dialogRef: MatDialogRef<EmployeeDetailsDialog>,
    @Inject(MAT_DIALOG_DATA) public data: { employee: EmployeeDTo }
  ) {}

  close() {
    this.dialogRef.close();
  }
}

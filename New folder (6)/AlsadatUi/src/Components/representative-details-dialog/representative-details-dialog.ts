import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { RepresentativeDTo } from '../../app/models/IRepresentative';

@Component({
  selector: 'app-representative-details-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule, MatCardModule],
  templateUrl: './representative-details-dialog.html',
  styleUrls: ['./representative-details-dialog.css']
})
export class RepresentativeDetailsDialog {
  constructor(
    public dialogRef: MatDialogRef<RepresentativeDetailsDialog>,
    @Inject(MAT_DIALOG_DATA) public data: { representative: RepresentativeDTo }
  ) {}

  close() {
    this.dialogRef.close();
  }
}

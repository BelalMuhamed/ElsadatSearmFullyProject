import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { CoponCollectionRepresentiveRateService } from '../../../app/Services/copon-collection-representive-rate.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-copon-rate-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatCardModule],
  templateUrl: './copon-rate-dialog.component.html',
  styleUrls: ['./copon-rate-dialog.component.css']
})
export class CoponRateDialogComponent implements OnInit {
  model: any = { id: 0, numberOfCopons: 0, cashed: 0, createdAt: new Date(), isDeleted: false };
  isLoading = false;
  isEdit = false;

  constructor(
    public dialogRef: MatDialogRef<CoponRateDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private service: CoponCollectionRepresentiveRateService
  ) {
    if (data) { this.model = { ...this.model, ...data }; const incomingId = (data as any).id; this.isEdit = incomingId != null && !isNaN(Number(incomingId)) && Number(incomingId) > 0; }
  }

  ngOnInit(): void {}

  save(): void {
    if (this.model.numberOfCopons == null || this.model.cashed == null) { Swal.fire('خطأ', 'الرجاء إدخال جميع الحقول', 'error'); return; }
    this.isLoading = true;
    const req = this.isEdit ? this.service.update(this.model) : this.service.create(this.model);
    req.subscribe({ next: (res:any) => { this.isLoading = false; Swal.fire('نجح', (res?.message) ?? 'تم الحفظ', 'success'); this.dialogRef.close(true); }, error: (e) => { this.isLoading = false; Swal.fire('خطأ', e?.error?.message ?? 'حدث خطأ', 'error'); } });
  }

  cancel(): void { this.dialogRef.close(false); }
}

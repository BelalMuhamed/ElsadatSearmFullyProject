import { BillDiscountsService } from './../../app/Services/bill-discounts-service';
import { Component, inject } from '@angular/core';
import { Subscription } from 'rxjs';
import { MatFormField, MatInputModule, MatLabel } from "@angular/material/input";
import { MatIcon, MatIconModule } from "@angular/material/icon";
import { BillDiscount } from '../../app/models/IBillDiscount';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatProgressSpinner, MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCellDef, MatColumnDef, MatHeaderCellDef, MatHeaderRowDef, MatRowDef, MatTableModule } from '@angular/material/table';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDialogModule } from '@angular/material/dialog';
import { HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-bill-discount-component',
  imports: [MatFormField, MatLabel, MatIcon, FormsModule, ReactiveFormsModule
    , MatIcon,
    MatTableModule,
    MatIcon,
    MatSlideToggleModule, MatFormField, MatLabel, FormsModule, MatTableModule,
  MatSlideToggleModule,
  MatFormFieldModule,
  MatInputModule,
  MatIconModule,
  MatProgressSpinnerModule,
  MatDialogModule,HttpClientModule
  ],
  standalone:true,
  templateUrl: './bill-discount-component.html',
  styleUrl: './bill-discount-component.css'
})
export class BillDiscountComponent {
  private service = inject(BillDiscountsService);
  private fb = inject(FormBuilder);

  form!: FormGroup;
  private sub = new Subscription();

  ngOnInit(): void {
    this.form = this.fb.group({
      firstDiscount: [0],
      secondDiscount: [0],
      thirdDiscount: [0],
    });

    this.loadData();
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  loadData() {
    this.sub.add(
      this.service.getBillDiscounts().subscribe({
        next: (res) => {
          if (res) {
            this.form.patchValue(res);
          }
        },
        error: (err) => console.log(err)
      })
    );
  }

  save() {
    const body = this.form.value;
    console.log("Saving:", body);

    this.sub.add(
      this.service.updateBillDiscount(body).subscribe({
        next: () => alert("✅ تم الحفظ بنجاح"),
        error: (err) => console.log(err)
      })
    );
  }
}

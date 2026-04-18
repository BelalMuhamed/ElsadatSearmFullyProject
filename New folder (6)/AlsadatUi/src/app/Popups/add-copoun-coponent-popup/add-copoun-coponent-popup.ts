import { Component, Inject, inject } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { CopounRespDto } from '../../models/Icopoun';
import { MatFormField, MatLabel, MatError, MatInputModule } from "@angular/material/input";
import { MatSelect, MatOption, MatSelectModule } from "@angular/material/select";
import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { CopounService } from '../../Services/copoun-service';
import { Subscription } from 'rxjs';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-add-copoun-coponent-popup',
  imports: [MatFormField, MatLabel, MatError, MatSelect, MatOption,
     CommonModule,
  ReactiveFormsModule,
  FormsModule,
  MatDialogModule,
  MatFormFieldModule,
  MatInputModule,
  MatSelectModule,
  MatButtonModule
  ],
  standalone:true,
  templateUrl: './add-copoun-coponent-popup.html',
  styleUrl: './add-copoun-coponent-popup.css'
})
export class AddCopounCoponentPopup {
  constructor(
  @Inject(MAT_DIALOG_DATA) public data: any
) {}
    copounPoints:number|null=null;
    private fb=inject(FormBuilder);
    private dialogRef=inject(MatDialogRef<AddCopounCoponentPopup>);
     private CopounService = inject(CopounService);
      private CopounSubscription = new Subscription();
form!: FormGroup;

    ngOnDestroy():void{
      this.CopounSubscription?.unsubscribe();
    }

ngOnInit(): void {

   // 1️⃣ انشئ فورم مبدئي فاضي
  this.form = this.fb.group({
    copounDesc: ['', Validators.required],
    copounPaiedType: [0, Validators.required],
    pointsToCollectCopoun: [0, Validators.required],
    stars: [0, Validators.required],
    paiedCash: [0, Validators.required],
  });

  // 2️⃣ لو Edit → اعمل load
  if (this.data?.mode === "edit") {
    this.initEditForm();
  }
  else {
    this.GetCopounPointsToCollect();
  }
}
initEditForm() {
  const c = this.data.item as CopounRespDto;

  this.form = this.fb.group({
    copounDesc: [ c.copounDesc, Validators.required],
    copounPaiedType: [c.copounPaiedType, Validators.required],
    pointsToCollectCopoun: [c.pointsToCollectCopoun, Validators.required],
    stars: [c.stars, Validators.required],
    paiedCash: [c.paiedCash, Validators.required],
  });

  this.copounPoints = c.pointsToCollectCopoun;
}

submit() {
  if (this.form.invalid) return;

  const dto: CopounRespDto = {
    ...this.data?.item,
    ...this.form.getRawValue(), // بما فيها الحقول disabled
  };

  dto.pointsToCollectCopoun = this.copounPoints;

  if (this.data?.mode === "edit") {
    this.update(dto);
  } else {
    this.add(dto);
  }
}
add(dto: CopounRespDto) {
  this.CopounSubscription.add(
    this.CopounService.addNewCopoun(dto).subscribe({
      next: () => {
        Swal.fire({
          icon: "success",
          title: "تمت الإضافة بنجاح",
        }).then(() => window.location.reload());
      },
      error: err => this.handleError(err)
    })
  );
}

update(dto: CopounRespDto) {
  this.CopounSubscription.add(
    this.CopounService.updateCopoun(dto).subscribe({
      next: () => {
        Swal.fire({
          icon: "success",
          title: "تم التعديل بنجاح",
        }).then(() => window.location.reload());
      },
      error: err => this.handleError(err)
    })
  );
}

handleError(err: any) {
  Swal.fire({
    icon: "error",
    title: "خطأ",
    text: err?.error?.message ?? "هناك مشكلة في الخادم",
  });
}

GetCopounPointsToCollect()
{
      this.CopounService.GetAllCopouns().subscribe({
          next:(res)=>{
            if(res.length>0)
            {
           this.copounPoints= res[0].pointsToCollectCopoun
  this.form = this.fb.group({
  copounDesc: ['', Validators.required],
  copounPaiedType: [0, Validators.required],
  pointsToCollectCopoun: [this.copounPoints, Validators.required],
  stars: [0, Validators.required],
  paiedCash: [0, Validators.required],
});

            }
            else
            {
                Swal.fire({
                    icon: 'error',
                    title: 'حدث خطأ',
                    text: `${"حاول الاتصال مجددا هناك مشكلة بالخادم !"}`,
                    confirmButtonText: 'موافق',
                    confirmButtonColor: '#d33'
                  });
            }
          },
          error:(err)=>{
            Swal.fire({
                    icon: 'error',
                    title: 'حدث خطأ',
                    text: `${err.message}`,
                    confirmButtonText: 'موافق',
                    confirmButtonColor: '#d33'
                  });
        }})

}}


import { Component, inject, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { GovernrateDto } from '../../models/IGovernrateVM';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatError, MatFormField, MatFormFieldModule, MatLabel } from "@angular/material/form-field";
import { MatOption, MatSelect, MatSelectModule } from '@angular/material/select';
import { CommonModule } from '@angular/common';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-add-edit-governrate-popup',
  standalone: true,
  imports: [MatFormField, MatLabel,MatFormField, MatLabel,
       CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule],
  templateUrl: './add-edit-governrate-popup.component.html',
  styleUrl: './add-edit-governrate-popup.component.css'
})
export class AddEditGovernratePopupComponent {
  isEditMode = false;

  constructor(
    private dialogRef: MatDialogRef<AddEditGovernratePopupComponent>,
    @Inject(MAT_DIALOG_DATA) public data: GovernrateDto | null
  ) {}
private fb = inject(FormBuilder);

  form!: FormGroup;
  ngOnInit(): void {
    this.initForm();
  }

  ngOnDestroy(): void {
  }
   initForm() {
    this.form = this.fb.group({
      name: [this.data?.name || '', Validators.required],
    });

    this.isEditMode = !!this.data;
  }
  submit() {
    if (!this.form.valid) return;

    const result: GovernrateDto = {
      id: this.isEditMode ? this.data!.id : null,
      name: this.form.value.name
    };

    this.dialogRef.close(result);
  }

  close() {
    this.dialogRef.close(null);
  }
}

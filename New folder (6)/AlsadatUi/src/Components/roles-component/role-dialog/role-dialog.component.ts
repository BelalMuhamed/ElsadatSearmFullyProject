import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../../app/Services/auth-service';
import { CreateRoleRequestDTO, RoleDTO } from '../../../app/models/IAuthModels';

@Component({
  selector: 'app-role-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatCardModule, MatIconModule],
  templateUrl: './role-dialog.component.html',
  styleUrls: ['./role-dialog.component.css']
})
export class RoleDialogComponent {
  private authService = inject(AuthService);
  dialogRef = inject(MatDialogRef<RoleDialogComponent>);
  data = inject(MAT_DIALOG_DATA) as RoleDTO | null;

  roleName: string = '';
  isSaving = false;

  constructor() {
    if (this.data) this.roleName = this.data.roleName || '';
  }

  save() {
    if (!this.roleName || this.roleName.trim().length === 0) {
      return;
    }
    this.isSaving = true;
    const dto: CreateRoleRequestDTO = { roleName: this.roleName };
    if (this.data && this.data.roleID) {
      this.authService.updateRole(this.data.roleID, dto).subscribe({ next: () => { this.isSaving = false; this.dialogRef.close(true); }, error: () => { this.isSaving = false; this.dialogRef.close(false); } });
    } else {
      this.authService.createRole(dto).subscribe({ next: () => { this.isSaving = false; this.dialogRef.close(true); }, error: () => { this.isSaving = false; this.dialogRef.close(false); } });
    }
  }

  cancel() { this.dialogRef.close(false); }
}

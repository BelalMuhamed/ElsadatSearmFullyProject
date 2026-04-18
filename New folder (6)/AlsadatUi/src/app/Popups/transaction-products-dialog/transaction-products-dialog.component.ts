import { Component, Inject } from '@angular/core';
import { StoreTransactionProductsDto } from '../../models/ITransactionVM';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatHeaderCellDef, MatCellDef, MatHeaderRowDef, MatRowDef, MatTableModule } from "@angular/material/table";
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-transaction-products-dialog',
  standalone: true,
  imports: [MatHeaderCellDef, MatCellDef, MatHeaderRowDef, MatRowDef, MatTableModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule],
  templateUrl: './transaction-products-dialog.component.html',
  styleUrl: './transaction-products-dialog.component.css'
})
export class TransactionProductsDialogComponent {
  products: StoreTransactionProductsDto[] = [];

  constructor(
    public dialogRef: MatDialogRef<TransactionProductsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { products: StoreTransactionProductsDto[] }
  ) {
    this.products = data.products;
  }

  ngOnInit(): void { }

  close() {
    this.dialogRef.close();
  }
}

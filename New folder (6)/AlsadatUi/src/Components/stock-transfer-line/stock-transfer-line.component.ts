import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  Output,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { StoreStockProductVM } from '../../app/models/IStockTransferVM';


/**
 * One editable line in the stock-transfer form.
 *
 * DUMB component contract:
 *   - INPUTS only (form group + product list + already-picked ids).
 *   - One OUTPUT: `remove` event.
 *   - No HTTP calls, no router, no business logic.
 *
 * The parent owns the data and the form state. This component just renders
 * one row and wires its inputs/outputs to the parent.
 */
@Component({
  selector: 'app-stock-transfer-line',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
  ],
  templateUrl: './stock-transfer-line.component.html',
  styleUrl: './stock-transfer-line.component.css',
})
export class StockTransferLineComponent {
  /** The line's own FormGroup, owned by the parent. */
  @Input({ required: true }) formGroup!: FormGroup;

  /** Zero-based row index — used in the row label. */
  @Input({ required: true }) index = 0;

  /** Full product catalog for the selected source warehouse. */
  @Input({ required: true }) availableProducts: StoreStockProductVM[] = [];

  /**
   * IDs already picked in OTHER lines.
   * Used to disable already-picked options so the same product can't be
   * added twice (the backend validator would also reject duplicates, but
   * preventing it in the UI is a much better experience).
   */
  @Input({ required: true }) pickedProductIds: number[] = [];

  /** Asks the parent to remove this line. */
  @Output() remove = new EventEmitter<void>();

  // ────────────────────────────────────────────────────────────────────
  // Derived helpers — pure, no side effects
  // ────────────────────────────────────────────────────────────────────

  /** The currently-selected product object, if any. */
  get selectedProduct(): StoreStockProductVM | null {
    const id = this.formGroup.get('productId')?.value as number | null;
    if (id === null || id === undefined) return null;
    return this.availableProducts.find((p) => p.productId === id) ?? null;
  }

  /** True when this option is selected in another line. */
  isOptionTaken(productId: number): boolean {
    const ownId = this.formGroup.get('productId')?.value as number | null;
    if (productId === ownId) return false;
    return this.pickedProductIds.includes(productId);
  }

  // ────────────────────────────────────────────────────────────────────
  // Event handlers
  // ────────────────────────────────────────────────────────────────────

  /**
   * When the product changes, snapshot its availableQuantity & avgCost into
   * the line's form so the qty validator works without an extra HTTP call.
   * Re-applies the max validator with the fresh ceiling.
   */
  onProductSelected(productId: number): void {
    const picked = this.availableProducts.find((p) => p.productId === productId);
    if (!picked) return;

    this.formGroup.patchValue({
      availableQuantity: picked.availableQuantity,
      avgCost: picked.avgCost,
      quantity: null, // force the user to type a fresh quantity
    });

    const qtyControl = this.formGroup.get('quantity');
    qtyControl?.setValidators([
      Validators.required,
      Validators.min(0.01),
      Validators.max(picked.availableQuantity),
    ]);
    qtyControl?.updateValueAndValidity();
  }

  onRemoveClicked(): void {
    this.remove.emit();
  }

  // ────────────────────────────────────────────────────────────────────
  // Template helpers — clarity over cleverness
  // ────────────────────────────────────────────────────────────────────

  get productControl() { return this.formGroup.get('productId')!; }
  get quantityControl() { return this.formGroup.get('quantity')!; }
  get availableQuantity(): number {
    return (this.formGroup.get('availableQuantity')?.value as number) ?? 0;
  }
  get avgCost(): number {
    return (this.formGroup.get('avgCost')?.value as number) ?? 0;
  }
}

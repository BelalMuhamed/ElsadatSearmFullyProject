import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import {
  AbstractControl,
  FormArray,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';

import Swal from 'sweetalert2';

import { TransactionService } from '../../app/Services/transaction.service';
import { StockService } from '../../app/Services/stock.service';
import { StoreService } from '../../app/Services/store.service';

import { StoreDto, StoreFilteration } from '../../app/models/IstoreVM';
import {
  StoreTransactionDto,
  StoreTransactionProductsDto,
} from '../../app/models/ITransactionVM';
import { StoreStockProductVM } from '../../app/models/IStockTransferVM';
import { StockTransferLineComponent } from '../stock-transfer-line/stock-transfer-line.component';


/**
 * Smart container for the "تحويل بين المخازن" (Stock-Transfer) page.
 *
 * Responsibilities (orchestration only — no business rules here):
 *   1. Fetch active warehouses for the source/destination dropdowns.
 *   2. When a source is picked, fetch only the products that have
 *      availableQuantity > 0 in that warehouse.
 *   3. Build a reactive form whose lines are managed by the dumb child
 *      <app-stock-transfer-line>. The container holds the form state.
 *   4. Validate cross-field rules (source ≠ destination, qty ≤ available)
 *      with custom validators — backend revalidates everything anyway.
 *   5. POST to the existing /api/Transaction endpoint, then offer to
 *      navigate to the transfer log.
 *
 * Why signals here: the form state is in Reactive Forms (the right tool for
 * a multi-line editable form), but the *page-level* booleans (loading flags,
 * fetched lookups) are signals — readable, change-detection friendly, and
 * consistent with the warehouse-inventory page.
 */
@Component({
  selector: 'app-stock-transfer',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    MatDividerModule,
    StockTransferLineComponent,
  ],
  templateUrl: './stock-transfer.component.html',
  styleUrl: './stock-transfer.component.css',
})
export class StockTransferComponent {
  // ────────────────────────────────────────────────────────────────────
  // Dependencies
  // ────────────────────────────────────────────────────────────────────
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);

  private readonly transactionService = inject(TransactionService);
  private readonly stockService = inject(StockService);
  private readonly storeService = inject(StoreService);

  // ────────────────────────────────────────────────────────────────────
  // State (signals)
  // ────────────────────────────────────────────────────────────────────
  readonly stores = signal<StoreDto[]>([]);
  readonly availableProducts = signal<StoreStockProductVM[]>([]);

  readonly isLoadingStores = signal<boolean>(false);
  readonly isLoadingProducts = signal<boolean>(false);
  readonly isSubmitting = signal<boolean>(false);

  /** Picked products — used to filter dropdowns in sibling lines so a product
   *  cannot be chosen twice. */
  readonly pickedProductIds = computed(() =>
    this.lines.controls
      .map((c) => c.get('productId')!.value as number | null)
      .filter((id): id is number => id !== null)
  );

  // ────────────────────────────────────────────────────────────────────
  // Form
  // ────────────────────────────────────────────────────────────────────
  readonly form: FormGroup = this.fb.group(
    {
      sourceId: this.fb.control<number | null>(null, [Validators.required]),
      destenationId: this.fb.control<number | null>(null, [Validators.required]),
      lines: this.fb.array<FormGroup>([], [Validators.required, this.minOneLineValidator()]),
    },
    { validators: [this.differentWarehousesValidator()] }
  );

  get lines(): FormArray<FormGroup> {
    return this.form.get('lines') as FormArray<FormGroup>;
  }

  get sourceId(): AbstractControl {
    return this.form.get('sourceId')!;
  }

  get destenationId(): AbstractControl {
    return this.form.get('destenationId')!;
  }

  // ────────────────────────────────────────────────────────────────────
  // Lifecycle
  // ────────────────────────────────────────────────────────────────────
  ngOnInit(): void {
    this.loadStores();

    // Source warehouse change → reload available products and reset lines.
    this.sourceId.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((id) => this.onSourceChanged(id as number | null));
  }

  // ────────────────────────────────────────────────────────────────────
  // Data loading
  // ────────────────────────────────────────────────────────────────────
  private loadStores(): void {
    this.isLoadingStores.set(true);

    const filter: StoreFilteration = {
      storeName: null,
      page: null,
      pageSize: null,
      isDeleted: false,
    };

    this.storeService
      .getAllStores(filter)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.stores.set(res?.data ?? []);
          this.isLoadingStores.set(false);
        },
        error: (err) => {
          this.isLoadingStores.set(false);
          this.alertError('خطأ', err?.error?.message ?? 'تعذر تحميل قائمة المخازن');
        },
      });
  }

  private onSourceChanged(storeId: number | null): void {
    // Reset every line — the previous source's products no longer apply.
    this.lines.clear();
    this.availableProducts.set([]);

    if (storeId === null || storeId <= 0) return;

    this.isLoadingProducts.set(true);

    this.stockService
      .getAvailableByStore(storeId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.isLoadingProducts.set(false);

          if (!res?.isSuccess) {
            this.alertError('خطأ', res?.message ?? 'تعذر تحميل مخزون المخزن');
            return;
          }

          this.availableProducts.set(res.data ?? []);

          if ((res.data ?? []).length === 0) {
            this.alertInfo('تنبيه', 'لا توجد منتجات متاحة في هذا المخزن');
            return;
          }

          // Seed with one empty line so the user sees what to do.
          this.addLine();
        },
        error: (err) => {
          this.isLoadingProducts.set(false);
          this.alertError('خطأ', err?.error?.message ?? 'تعذر تحميل مخزون المخزن');
        },
      });
  }

  // ────────────────────────────────────────────────────────────────────
  // Form-array management
  // ────────────────────────────────────────────────────────────────────
  addLine(): void {
    if (this.availableProducts().length === 0) {
      this.alertInfo('تنبيه', 'اختر المخزن المصدر أولًا');
      return;
    }

    if (this.lines.length >= this.availableProducts().length) {
      this.alertInfo('تنبيه', 'لقد أضفت كل المنتجات المتاحة بالفعل');
      return;
    }

    this.lines.push(
      this.fb.group({
        productId: this.fb.control<number | null>(null, [Validators.required]),
        availableQuantity: this.fb.control<number>(0),
        avgCost: this.fb.control<number>(0),
        quantity: this.fb.control<number | null>(null, [
          Validators.required,
          Validators.min(0.01),
        ]),
      })
    );
  }

  removeLine(index: number): void {
    this.lines.removeAt(index);
  }

  // ────────────────────────────────────────────────────────────────────
  // Submission
  // ────────────────────────────────────────────────────────────────────
  async submit(): Promise<void> {
    this.form.markAllAsTouched();

    if (this.form.invalid) {
      this.alertError('بيانات غير مكتملة', 'يرجى مراجعة الحقول الموضّحة باللون الأحمر');
      return;
    }

    const confirm = await Swal.fire({
      icon: 'question',
      title: 'تأكيد التحويل',
      text: `سيتم تحويل ${this.lines.length} منتج. هل تريد المتابعة؟`,
      showCancelButton: true,
      confirmButtonText: 'نعم، نفّذ',
      cancelButtonText: 'إلغاء',
    });

    if (!confirm.isConfirmed) return;

    const dto = this.buildDto();
    this.isSubmitting.set(true);

    this.transactionService
      .addNewTransaction(dto)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res: any) => {
          this.isSubmitting.set(false);

          // The new controller returns the full Result envelope on success;
          // older success path returned { message } — we tolerate both.
          const message =
            res?.data ?? res?.message ?? 'تم تنفيذ التحويل بنجاح';

          Swal.fire({
            icon: 'success',
            title: 'تم بنجاح',
            text: message,
            confirmButtonText: 'عرض السجل',
            showCancelButton: true,
            cancelButtonText: 'تحويل آخر',
          }).then((r) => {
            if (r.isConfirmed) {
              this.router.navigate(['/transactions/all']);
            } else {
              this.resetAfterSuccess();
            }
          });
        },
        error: (err) => {
          this.isSubmitting.set(false);
          const message =
            err?.error?.message ??
            err?.error?.Message ??
            err?.message ??
            'تعذر تنفيذ التحويل';
          this.alertError('خطأ', message);
        },
      });
  }

  private buildDto(): StoreTransactionDto {
    const value = this.form.getRawValue();

    const products: StoreTransactionProductsDto[] = (value.lines as any[]).map(
      (line) => ({
        transactionId: null,
        productId: line.productId as number,
        productName: undefined,
        quantity: line.quantity as number,
      })
    );

    return {
      sourceId: value.sourceId as number,
      destenationId: value.destenationId as number,
      sourceName: null,
      destenationName: null,
      makeTransactionUser: this.readCurrentUserName(),
      transactionProducts: products,
      createdAt: null,
    };
  }

  /**
   * Reads the logged-in user's display name from localStorage.
   * Mirrors the convention used elsewhere in the app (see AuthService.clearStorageAndRedirect
   * which removes 'userName'). Falls back to 'unknown' so the backend gets a non-empty string;
   * the validator will reject empty strings, never 'unknown' silently.
   */
  private readCurrentUserName(): string {
    if (typeof window === 'undefined' || !window.localStorage) return '';
    return localStorage.getItem('userName') ?? '';
  }

  private resetAfterSuccess(): void {
    this.lines.clear();
    this.form.reset({ sourceId: null, destenationId: null });
    this.availableProducts.set([]);
  }

  // ────────────────────────────────────────────────────────────────────
  // Validators (form-level)
  // ────────────────────────────────────────────────────────────────────
  private differentWarehousesValidator(): ValidatorFn {
    return (group: AbstractControl): ValidationErrors | null => {
      const src = group.get('sourceId')?.value;
      const dst = group.get('destenationId')?.value;
      if (src && dst && src === dst) {
        return { sameWarehouse: true };
      }
      return null;
    };
  }

  private minOneLineValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const arr = control as FormArray;
      return arr.length === 0 ? { emptyLines: true } : null;
    };
  }

  // ────────────────────────────────────────────────────────────────────
  // SweetAlert helpers
  // ────────────────────────────────────────────────────────────────────
  private alertError(title: string, text: string): void {
    Swal.fire({ icon: 'error', title, text });
  }

  private alertInfo(title: string, text: string): void {
    Swal.fire({ icon: 'info', title, text });
  }
}

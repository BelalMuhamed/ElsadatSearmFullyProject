import { CommonModule } from '@angular/common';
import {
  Component,
  Input,
  OnDestroy,
  OnInit,
  forwardRef
} from '@angular/core';
import {
  ControlValueAccessor,
  FormControl,
  FormGroup,
  NG_VALUE_ACCESSOR,
  ReactiveFormsModule
} from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatOptionModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { Subject, takeUntil } from 'rxjs';

import {
  COUNTRIES,
  Country,
  DEFAULT_COUNTRY_ISO,
  findCountryByE164,
  findCountryByIso
} from '../../assets/Countries';

/**
 * PhoneInputComponent — reusable country + national-number input.
 *
 *  Implements ControlValueAccessor → exposes a SINGLE E.164 string to parent
 *  forms (e.g. "+201008319741"). On writeValue it splits the value into
 *  { country, nationalNumber } so edit-mode hydration works correctly.
 */
@Component({
  selector: 'app-phone-input',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatOptionModule,
    MatIconModule
  ],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => PhoneInputComponent),
      multi: true
    }
  ],
  templateUrl: './phone-input.component.html',
  styleUrls: ['./phone-input.component.css']
})
export class PhoneInputComponent implements ControlValueAccessor, OnInit, OnDestroy {

  /** Default ISO2 country code (lowercase). Defaults to Egypt. */
  @Input() defaultCountry: string = DEFAULT_COUNTRY_ISO;

  /** Label displayed above the national-number input. */
  @Input() label: string = 'رقم الهاتف';

  /** Marks the field as required (visual + aria). Outer form validates. */
  @Input() required: boolean = false;

  /** Disable the control from the outside. */
  @Input() disabled: boolean = false;

  /** Optional hint text shown under the field. */
  @Input() hint: string = '';

  // -----------------------------------------------------------------------
  // Internal state
  // -----------------------------------------------------------------------

  readonly countries: readonly Country[] = COUNTRIES;

  /** Inner form: a country picker + a national-number input. */
  readonly innerForm = new FormGroup({
    country: new FormControl<Country>(
      findCountryByIso(DEFAULT_COUNTRY_ISO),
      { nonNullable: true }
    ),
    nationalNumber: new FormControl<string>('', { nonNullable: true })
  });

  searchTerm: string = '';
  filteredCountries: readonly Country[] = this.countries;

  /** True while writeValue() is hydrating us — prevents echo back to parent. */
  private isHydrating = false;

  /**
   * True once writeValue has been called with a non-empty value.
   * We use this to STOP ngOnInit from clobbering the hydrated country
   * with the @Input() defaultCountry — the bug that caused edit-mode
   * phone numbers to render with the wrong country code.
   */
  private hasHydratedFromParent = false;

  private readonly destroy$ = new Subject<void>();

  // CVA callbacks
  private onChange: (value: string | null) => void = () => {};
  private onTouched: () => void = () => {};

  // -----------------------------------------------------------------------
  // Lifecycle
  // -----------------------------------------------------------------------
  ngOnInit(): void {
    // Only seed with @Input() defaultCountry when the parent did NOT
    // already give us a value via writeValue(). This is the critical
    // guard that fixes edit-mode hydration.
    if (!this.hasHydratedFromParent) {
      this.innerForm.controls.country.setValue(
        findCountryByIso(this.defaultCountry),
        { emitEvent: false }
      );
    }

    this.filteredCountries = this.countries;

    // Push every change up to the parent as a single E.164 string.
    this.innerForm.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        if (this.isHydrating) return;
        this.onChange(this.buildE164());
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // -----------------------------------------------------------------------
  // ControlValueAccessor
  // -----------------------------------------------------------------------
  writeValue(value: string | null): void {
    this.isHydrating = true;
    try {
      if (!value) {
        // Clear the national number, keep the currently selected country.
        this.innerForm.controls.nationalNumber.setValue('', { emitEvent: false });
        return;
      }

      // Mark hydration BEFORE we mutate, so ngOnInit (which may run after
      // writeValue in some Angular versions) leaves our country alone.
      this.hasHydratedFromParent = true;

      const e164 = String(value).replace(/[^\d+]/g, '');
      const country = findCountryByE164(e164);

      // Strip the dial code from the front to get the national part.
      const digitsOnly = e164.replace(/\D/g, '');
      const national = digitsOnly.startsWith(country.dialCode)
        ? digitsOnly.slice(country.dialCode.length)
        : digitsOnly;

      this.innerForm.controls.country.setValue(country, { emitEvent: false });
      this.innerForm.controls.nationalNumber.setValue(national, { emitEvent: false });
    } finally {
      this.isHydrating = false;
    }
  }

  registerOnChange(fn: (value: string | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
    if (isDisabled) {
      this.innerForm.disable({ emitEvent: false });
    } else {
      this.innerForm.enable({ emitEvent: false });
    }
  }

  // -----------------------------------------------------------------------
  // UI event handlers
  // -----------------------------------------------------------------------

  /**
   * Filters digits-only on every keystroke + applies the country max-length cap.
   * Uses setValue with emitEvent:false to push the cleaned value back, then
   * triggers onChange manually so the parent gets exactly one update per keystroke.
   */
  onNationalInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const country = this.innerForm.controls.country.value;
    const cleaned = (input.value || '').replace(/\D/g, '').slice(0, country.maxLength);

    if (input.value !== cleaned) {
      input.value = cleaned;
    }

    if (this.innerForm.controls.nationalNumber.value !== cleaned) {
      this.innerForm.controls.nationalNumber.setValue(cleaned, { emitEvent: false });
      if (!this.isHydrating) {
        this.onChange(this.buildE164());
      }
    }
  }

  /** Filters the country dropdown by name (EN + AR) or dial code. */
  onSearchCountry(event: Event): void {
    // Prevent mat-select's native typeahead from competing with our search.
    event.stopPropagation();

    const needle = (event.target as HTMLInputElement).value.trim().toLowerCase();
    this.searchTerm = needle;

    if (!needle) {
      this.filteredCountries = this.countries;
      return;
    }

    this.filteredCountries = this.countries.filter(c =>
      c.name.toLowerCase().includes(needle) ||
      c.nameAr.includes(needle) ||
      c.dialCode.includes(needle) ||
      c.iso2.includes(needle)
    );
  }

  /** Reset the search filter when the panel closes (UX hygiene). */
  onPanelOpenedChange(opened: boolean): void {
    if (!opened) {
      this.searchTerm = '';
      this.filteredCountries = this.countries;
    }
  }

  /** Compare-by used by mat-select so country option identity works. */
  compareCountries = (a: Country | null, b: Country | null): boolean => {
    if (!a || !b) return a === b;
    return a.iso2 === b.iso2 && a.dialCode === b.dialCode;
  };

  /** TrackBy for *ngFor — keyed by ISO2 which is unique per country. */
  trackCountry = (_index: number, country: Country): string => country.iso2;

  /** Forward the blur event so the parent control's `touched` flag flips. */
  handleBlur(): void {
    this.onTouched();
  }

  // -----------------------------------------------------------------------
  // Helpers
  // -----------------------------------------------------------------------

  /** Composes the current selection into an E.164 string, or null when empty. */
  private buildE164(): string | null {
    const country = this.innerForm.controls.country.value;
    const national = (this.innerForm.controls.nationalNumber.value || '').trim();
    if (!national) return null;
    return `+${country.dialCode}${national}`;
  }
}

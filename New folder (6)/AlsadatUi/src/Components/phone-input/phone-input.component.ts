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
 * PhoneInputComponent — reusable country+phone input.
 *
 * Angular 17 compatible. ZERO external dependencies (only Angular Material
 * modules already used elsewhere in the app).
 *
 * • Implements ControlValueAccessor → usable with `formControlName`.
 * • Writes a single E.164 string (e.g. "+201012345678") to the parent form.
 * • Reads an E.164 string, splits it into {country, nationalNumber}, and
 *   populates both controls (supports edit mode).
 * • Filters digits only in the national-number input.
 * • Caps length per selected country (soft client-side guard).
 *
 * Usage:
 *   <app-phone-input formControlName="phoneNumbers"
 *                    [defaultCountry]="'eg'"
 *                    [required]="true">
 *   </app-phone-input>
 *
 * Consumers never see country/national separately — the parent form only
 * deals with the single E.164 string.
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

  /** All countries available in the dropdown. */
  readonly countries: readonly Country[] = COUNTRIES;

  /** Inner form: a country picker + a national-number input. */
  readonly innerForm = new FormGroup({
    country: new FormControl<Country>(findCountryByIso(DEFAULT_COUNTRY_ISO), { nonNullable: true }),
    nationalNumber: new FormControl<string>('', { nonNullable: true })
  });

  /** Search filter for the country dropdown. */
  searchTerm: string = '';
  filteredCountries: readonly Country[] = this.countries;

  /**
   * Tracks whether a writeValue is in progress so we don't fire onChange back
   * into the parent form while hydrating from an incoming value (prevents loops).
   */
  private isHydrating = false;

  private readonly destroy$ = new Subject<void>();

  // CVA callbacks
  private onChange: (value: string | null) => void = () => {};
  private onTouched: () => void = () => {};

  // -----------------------------------------------------------------------
  // Lifecycle
  // -----------------------------------------------------------------------
  ngOnInit(): void {
    // Seed the country with the configured default (only if we haven't
    // already been given a value via writeValue → the hydration path sets it).
    this.innerForm.controls.country.setValue(
      findCountryByIso(this.defaultCountry),
      { emitEvent: false }
    );

    this.filteredCountries = this.countries;

    // Whenever either inner field changes, recompose the E.164 string and
    // push it up to the parent form.
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
    if (isDisabled) this.innerForm.disable({ emitEvent: false });
    else this.innerForm.enable({ emitEvent: false });
  }

  // -----------------------------------------------------------------------
  // UI event handlers
  // -----------------------------------------------------------------------

  /**
   * Filters digits-only on every keystroke so the user can't enter letters.
   * Applies a soft length cap based on the selected country.
   */
  onNationalInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const country = this.innerForm.controls.country.value;
    const cleaned = (input.value || '').replace(/\D/g, '').slice(0, country.maxLength);
    if (input.value !== cleaned) {
      input.value = cleaned;
    }
    // valueChanges fires naturally from reactive forms — no manual setValue needed.
    this.innerForm.controls.nationalNumber.setValue(cleaned, { emitEvent: true });
  }

  /** Filters the country dropdown by name (EN + AR) or dial code. */
  onSearchCountry(event: Event): void {
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

  /** Compare-by used by mat-select so country option identity works. */
  compareCountries = (a: Country | null, b: Country | null): boolean => {
    if (!a || !b) return a === b;
    return a.iso2 === b.iso2 && a.dialCode === b.dialCode;
  };

  /** TrackBy for *ngFor — keyed by ISO2 which is unique per country. */
  trackCountry = (_index: number, country: Country): string => country.iso2;

  /** Invoked on blur of the national-number field — forwarded to parent. */
  handleBlur(): void {
    this.onTouched();
  }

  // -----------------------------------------------------------------------
  // Helpers
  // -----------------------------------------------------------------------

  /**
   * Builds the E.164 string from the two inner fields.
   * Returns null if the national part is empty (so the parent form can
   * reason about emptiness; backend regex enforces the +/digits format).
   */
  private buildE164(): string | null {
    const country = this.innerForm.controls.country.value;
    const national = (this.innerForm.controls.nationalNumber.value || '').replace(/\D/g, '');

    if (!national) return null;
    return `+${country.dialCode}${national}`;
  }
}

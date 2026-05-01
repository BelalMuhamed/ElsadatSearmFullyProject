/**
 * Plumber domain models (frontend).
 * Mirror the backend DTOs in Application/DTOs/PlumberDtos.cs
 * Plumber = Supplier shape + LicenseNumber + Specialty.
 * NO chart-of-accounts integration — pure master data.
 */

export interface PlumberDto {
  id?: number | null;
  name: string;
  phoneNumbers: string;            // E.164 format, e.g. +201012345678
  address?: string | null;
  cityId?: number | null;          // optional / nullable
  licenseNumber?: string | null;
  specialty?: string | null;
  cityName?: string | null;        // populated by server on read
  isDeleted: boolean;
}

export interface PlumberFilteration {
  name: string | null;
  phoneNumbers?: string | null;
  licenseNumber?: string | null;
  specialty?: string | null;
  isDeleted?: boolean | null;      // null = all, false = active, true = archived
  page?: number | null;
  pageSize?: number | null;
}

/** Lightweight payload for select-boxes across the app. */
export interface PlumberLookupDto {
  id: number;
  name: string;
  specialty?: string | null;
}

export interface PlumberLookupFilter {
  name?: string | null;
  specialty?: string | null;
}

/** Per-row error returned by the import endpoint. */
export interface PlumberImportRowError {
  rowNumber: number;
  plumberName?: string | null;
  column: string;
  message: string;
}

/** Result payload from POST /Plumber/import. */
export interface PlumberImportResult {
  totalRows: number;
  successCount: number;
  failedCount: number;
  imported: PlumberDto[];
  errors: PlumberImportRowError[];
}

/**
 * Common specialty options offered as suggestions in the form.
 * Kept frontend-side (not enum) to match the backend's free-text design —
 * adding a new specialty doesn't require a backend change.
 */
export const PLUMBER_SPECIALTY_OPTIONS: readonly string[] = [
  'سباكة منزلية',
  'سباكة تجارية',
  'سباكة صناعية',
  'تركيب سخانات',
  'صرف صحي',
  'صيانة عامة'
] as const;

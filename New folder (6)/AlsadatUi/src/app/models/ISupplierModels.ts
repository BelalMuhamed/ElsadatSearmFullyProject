/**
 * Supplier domain models (frontend).
 * Mirror the backend DTOs in Application/DTOs/SupplierDtos.cs
 * Products array removed per business decision — suppliers are standalone.
 * cityId is now OPTIONAL — suppliers can be saved without a city.
 */

export interface SupplierDto {
  id?: number | null;
  name: string;
  phoneNumbers: string;            // E.164 format, e.g. +201012345678
  address?: string | null;
  cityId?: number | null;          // ← now optional / nullable
  cityName?: string | null;        // populated by server on read
  isDeleted: boolean;
}

export interface SupplierFilteration {
  name: string | null;
  phoneNumbers?: string | null;
  isDeleted?: boolean | null;      // null = all, false = active, true = archived
  page?: number | null;
  pageSize?: number | null;
}

/** Lightweight payload for select-boxes across the app. */
export interface SupplierLookupDto {
  id: number;
  name: string;
}

export interface SupplierLookupFilter {
  name?: string | null;
}

/** Per-row error row returned by the import endpoint. */
export interface SupplierImportRowError {
  rowNumber: number;
  supplierName?: string | null;
  column: string;
  message: string;
}

/** Result payload from POST /Supplier/import. */
export interface SupplierImportResult {
  totalRows: number;
  successCount: number;
  failedCount: number;
  imported: SupplierDto[];
  errors: SupplierImportRowError[];
}

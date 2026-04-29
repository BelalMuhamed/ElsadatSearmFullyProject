/**
 * Distributors / Merchants / Agents (Clients) frontend models.
 * Mirrors the backend DTO in:
 *   Application/DTOs/Distributor_MerchantAndAgentDtos.cs
 *
 * Validation contract (must stay in sync with backend + Excel import):
 *   REQUIRED → fullName, phoneNumber, type
 *   OPTIONAL → address, gender, cityId, all audit fields, all financial fields
 *
 * Audit fields (createdAt/By, updatedAt/By, deletedAt/By) are SERVER-OWNED.
 * The frontend reads them but must NEVER send them on Add/Edit.
 *
 * `password` was removed from this VM — it leaked on GetById.
 * If you ever need it on Add, create a dedicated CreateClientCredentialsDto.
 */
export interface DistributorsAndMerchantsDto {
  // ───────── identity ─────────
  userId?: string | null;

  // ───────── REQUIRED ─────────
  fullName: string;
  phoneNumber: string;          // also acts as username / email at the backend
  type: number | null;          // 0: Distributor | 1: Merchant | 2: Agent

  // ───────── OPTIONAL ─────────
  address?: string | null;      // ✅ optional (Excel import support)
  gender?: number | null;       // ✅ optional (0: Male | 1: Female)
  cityId?: number | null;       // ✅ optional (Excel import support)

  // ───────── read-only on responses ─────────
  cityName?: string | null;     // populated by the server, never sent

  // ───────── server-owned audit fields (read-only) ─────────
  createdAt?: string | null;    // ISO date string
  createdBy?: string | null;
  updatedAt?: string | null;
  updatedBy?: string | null;
  isDelted?: boolean | null;    // toggled via the soft-delete flow
  deletedAt?: string | null;
  deletedBy?: string | null;

  // ───────── financial / read-only on the listing ─────────
  pointsBalance?: number | null;
  cashBalance?: number | null;
  indebtedness?: number | null;

  // ───────── special discounts (optional) ─────────
  firstSpecialDiscount?: number | null;
  secondSpecialDiscount?: number | null;
  thirdSpecialDiscount?: number | null;
}

/**
 * Filters used by GET /api/DistAndMerch/list.
 * All fields are optional. `page` & `pageSize` default to 1 / 10 in the component.
 */
export interface DistributorsAndMerchantsFilters {
  phoneNumber?: string | null;
  fullName?: string | null;
  cityName?: string | null;
  type?: number | null;
  isDeleted?: boolean | null;
  page?: number | null;
  pageSize?: number | null;
}

/**
 * Excel import row contract.
 * Mirrors backend DistributorMerchantExcelDto with the Arabic column names
 * exactly as they appear in the downloaded template.
 *
 * REQUIRED only: fullName, type, phoneNumber.
 */
export interface DistributorMerchantExcelRow {
  الاسم_بالكامل: string;
  النوع: 'موزع' | 'تاجر' | 'وكيل' | string;
  رقم_الهاتف: string;
}

/**
 * Per-row error returned by POST /api/DistAndMerch/import.
 * Used by ImportExcelDialogComponent to render the failed-rows table.
 */
export interface DistributorMerchantImportRowError {
  rowNumber: number;
  fullName?: string | null;
  column: string;
  message: string;
}

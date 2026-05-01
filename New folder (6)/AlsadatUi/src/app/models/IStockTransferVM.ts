/**
 * Stock-Transfer view models.
 * Mirrors:
 *   - Application/DTOs/StoreTransactionDtos.cs (StoreTransactionDto, StoreTransactionProductsDto)
 *   - Application/DTOs/Stock/StoreStockProductDto.cs
 *
 * The transfer form sends the existing StoreTransactionDto unchanged.
 * The product picker is populated from the new GET /api/Stock/by-store/{id} endpoint.
 */

/**
 * One product currently held at a specific warehouse.
 * Returned by GET /api/Stock/by-store/{storeId}.
 *
 * IMPORTANT: `avgCost` is INFORMATIONAL only. It is shown as a read-only
 * indicator in the UI; the authoritative cost calculation happens server-side
 * inside StockMovementCalculator.
 */
export interface StoreStockProductVM {
  productId: number;
  productName: string;
  productCode: string;
  availableQuantity: number;
  avgCost: number;
}

/**
 * One line in the transfer form.
 * `productId` is bound from the dropdown selection;
 * `availableQuantity` is captured at selection time so the qty validator works
 *    without having to re-query the backend per keystroke.
 */
export interface StockTransferLineVM {
  productId: number | null;
  productName: string | null;
  productCode: string | null;
  availableQuantity: number;
  avgCost: number;
  quantity: number | null;
}

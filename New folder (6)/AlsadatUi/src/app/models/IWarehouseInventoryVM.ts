/**
 * Warehouse inventory matrix — mirrors Application/DTOs/Stock/WarehouseInventoryMatrixDto.cs
 *
 * Server pre-computes:
 *  - per-product totals across warehouses
 *  - per-warehouse totals (footer row)
 *  - stock health classification
 *
 * The UI just renders. NEVER re-classify health on the frontend.
 */

export enum StockHealth {
  OutOfStock = 0,
  Critical   = 1,
  Warning    = 2,
  Healthy    = 3,
}

export interface WarehouseColumn {
  storeId: number;
  storeName: string;
  isDeleted: boolean;
}

export interface WarehouseTotal {
  storeId: number;
  totalQuantity: number;
}

export interface ProductInventoryRow {
  productId: number;
  productName: string;
  productCode: string;
  isDeleted: boolean;
  reorderThreshold: number;

  /** Map keyed by storeId. Backend always emits an entry for every active warehouse (0 if empty). */
  quantities: Record<number, number>;

  totalQuantityAcrossWarehouses: number;
  health: StockHealth;
}

export interface WarehouseInventoryMatrix {
  warehouses: WarehouseColumn[];
  products: ProductInventoryRow[];
  warehouseTotals: WarehouseTotal[];
  grandTotalQuantity: number;
}

export interface WarehouseInventoryFilter {
  productName?: string | null;
  productCode?: string | null;
  storeId?: number | null;
  lowStockOnly?: boolean | null;
  excludeDeletedProducts?: boolean;
  excludeDeletedWarehouses?: boolean;
  page: number;
  pageSize: number;
}

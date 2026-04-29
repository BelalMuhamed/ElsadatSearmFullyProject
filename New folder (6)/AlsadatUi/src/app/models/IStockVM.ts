/**
 * Stock view-models used by:
 *  - sales-invoice withdrawal flow (ProductStockDto, ProductStockPerStoreDto)
 *  - legacy store-by-store views (StockDto / StockProducts)
 *
 * `highQuantity` removed — maximum-quantity is no longer a business concept.
 * The new المخزون page uses the matrix DTO from `IWarehouseInventoryVM.ts`.
 */

export interface StockDto {
  storeID: number;
  storeName: string;
  isDeleted: boolean;
  storeStocks: StockProducts[] | null;
}

export interface StockProducts {
  productId: number;
  productName: string;
  quantity: number;
  isDeleted: boolean;
  /** Reorder threshold for low-stock alerts. */
  lowQuantity: number;
}

export interface StockFilterations {
  page: number;
  pageSize: number;
  storeName: string | null;
}

export interface ProductStockPerStoreDto {
  storeName: string | null;
  storeId: number;
  isStoreDeleted: boolean | null;
  avaliableQuantity: number | null;
  withdrawnQuantity: number | null;
}

export interface ProductStockDto {
  productId: number;
  productName: string | null;
  isProductDeleted: boolean | null;
  isCategoryDeleted: boolean | null;
  stocks: ProductStockPerStoreDto[];
}

export interface invoiceProductsStock {
  invoiceId: number;
  updateBy: string | null;
  withdrwanItemsQuantities: ProductStockDto[];
}

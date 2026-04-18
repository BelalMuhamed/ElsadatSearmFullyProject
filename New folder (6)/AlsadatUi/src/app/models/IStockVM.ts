export interface StockDto {
  storeID: number;
  storeName: string;
  isDeleted: boolean;
  storeStocks: StockProducts[]|null;
}

export interface StockProducts {
  productId: number;
  productName: string;
  quantity: number;
  isDeleted: boolean;
  lowQuantity: number;
  highQuantity: number;
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
export interface ProductStockPerStoreDto {
  storeName: string | null;
  storeId: number;
  isStoreDeleted: boolean | null;
  avaliableQuantity: number | null;
  withdrawnQuantity: number | null;
}
export interface invoiceProductsStock
{
  invoiceId:number,
  updateBy:string|null,
  withdrwanItemsQuantities :ProductStockDto[]
}

export interface ProductStockDto {
  productId: number;
  productName: string | null;
  isProductDeleted: boolean | null;
  isCategoryDeleted: boolean | null;
  stocks: ProductStockPerStoreDto[];
}

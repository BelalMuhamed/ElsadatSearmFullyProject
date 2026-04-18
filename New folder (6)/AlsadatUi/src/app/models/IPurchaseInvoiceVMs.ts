import e from "express";

export interface PurchaseInvoiceDtos {
  id?: number | null;
  createdAt?: Date | null;
  createdBy?: string | null;
  updatedBy?: string | null;
  updatedAt?: Date | null;
  totalGrowthAmount?: number | null;
  totalNetAmount?: number | null;
  invoiceNumber?: string | null;
  supplierId?: number | null;
  supplierName?: string | null;
  settledStatus?: number | null;
  deleteStatus?: number | null;
  precentageRival?: number | null;
  rivalValue?: number | null;
  totalRivalValue?: number | null;
  taxPrecentage?: number | null;
  taxValue?: number | null;
  settledStoreId?: number | null;
  settledStoreName?: string | null;

  items: PurchaseInvoiceItemsDtos[];
}
export interface PurchaseInvoiceItemsDtos {
  id?: number | null;
  itemCode?: string | null;
  buyingPricePerUnit?: number | null;
  productId?: number | null;
  productName?: string | null;
  quantity: number;
  purchaseInvoiceId?: number | null;
  purchaseInvoiceNumber?: string | null;
  precentageRival?: number | null;
  rivalValue?: number | null;
  totalRivalValue?: number | null;
  totalGrowthAmount?: number | null;
  totalNetAmount?: number | null;
}
export interface PurchaseInvoiceFilteration
{
  page:number|null;
  pageSize:number|null;
  invoiceNumber:string|null;
  supplierId:number|null;
  settledStatus:number|null;
  deleteStatus:number|null;
}


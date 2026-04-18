  // export interface SalesInvoicesResponse {
  //   id:number;
  // firstDiscount: number|null;
  // secondDiscount:number|null;
  // thirdDiscount:number|null;
  // totalBeforDiscounts: number;
  // totalAfterDiscounts: number;
  // paid:number|null;
  // residual:number|null;
  // totalPoints: number;
  // createdAt: Date;
  // relatedReturnInvoiceID:number|null;
  // distributorID: string;
  //   totalCopouns: number;
  //   createdBy: string;
  //   isReturn?: boolean;
  //   salesInvoiceStatus: number|null;
  //   distributorName: string;
  // }

import { ProductStockPerStoreDto } from "./IStockVM";


// export interface SalesInvoiceItemsResp
// {
// sellingPrice:number;
// quantity:number;
// pointEarned:number;
// productId:number|null;
// productName:string;
// discountPerItem:number;

// }


export interface InvoiceChangeStatusReq {
  id: number;
  salesInvoiceStatus: number | null;
  deleteStatus: number | null;
  updateBy:string|null;
}

 export interface SalesInvoicesResponse {
  id : number ;
  totalCopouns: number;
  distributorId: string;
  distributorName: string | null;
  firstDiscount: number| null;
  secondDiscount: number| null;
  thirdDiscount: number| null;
  invoiceNumber: string| null;
  totalPoints: number| null;
  createdAt: Date;
  createdBy: string;
  salesInvoiceStatus: number| null;
  deleteStatus: number| null;
  updateBy: string| null;
  updateAt: Date| null;
  totalGrowthAmount: number| null;
  totalNetAmount: number| null;
  taxPrecentage: number| null;
  taxValue: number| null;
  reverseJournalEntry:number| null;
  items: SalesInvoiceItemsResp[];
}

export interface SalesInvoiceItemsResp {
  id: number| null;
  sellingPrice: number;
  quantity: number;
  productID: number;
  productName: string| null;
  precentageRival: number| null;
  rivalValue: number| null;
  totalRivalValue: number| null;
  totalGrowthAmount: number| null;
  totalNetAmount: number| null;
}
export interface SalesInvoiceFilters {
  page: number | null;
  pageSize: number | null;
  invoiceNumber: string | null;
  customerId: string | null;
  createAt: string|null;
  deleteStatus: number | null;
}


 export interface salesInvoiceItemsDetails {
  id: number | null;
  sellingPrice: number;
  quantity: number;
  productID: number;
  productName: string | null;
  precentageRival: number | null;
  rivalValue: number | null;
  totalRivalValue: number | null;
  totalGrowthAmount: number | null;
  totalNetAmount: number | null;
  withdrwanStock: ProductStockPerStoreDto[];
}
  export interface SalesInvoiceDetails {
  id: number;
  totalCopouns: number | null;
  distributorId: string;
  distributorName: string | null;
  firstDiscount: number | null;
  secondDiscount: number | null;
  thirdDiscount: number | null;
  invoiceNumber: string | null;
  totalPoints: number | null;
  createdAt: string;
  createdBy: string;
  salesInvoiceStatus: number | null;
  deleteStatus: number | null;
  updateBy: string | null;
  updateAt: string | null;
  totalGrowthAmount: number | null;
  totalNetAmount: number | null;
  taxPrecentage: number | null;
  taxValue: number | null;
  withdrwanStock: salesInvoiceItemsDetails[];
}

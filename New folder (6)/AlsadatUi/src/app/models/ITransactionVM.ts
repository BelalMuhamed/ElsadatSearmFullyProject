

export interface StoreTransactionProductsDto {
    transactionId: number|null;
    productId: number;
    productName?: string;
    quantity: number;
}

export interface StoreTransactionDto {
    id?: number|null;
    sourceId?: number;
    destenationId?: number;
    sourceName:string|null;
    destenationName:string|null;
        makeTransactionUser: string;
    transactionProducts?: StoreTransactionProductsDto[];
    createdAt: string|null;
}

export interface StoreTransactionFilters {
    sourceName?: string|null;
    destenationName?: string|null;
    createdAt: string|null;
    page: number;
    pageSize: number;
}

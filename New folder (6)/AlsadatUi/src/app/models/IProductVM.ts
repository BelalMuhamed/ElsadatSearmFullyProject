export interface ProductDto {
  id: number|null;
  name: string|null;
  sellingPrice: number|null;
  pointPerUnit: number|null;
  createBy?: string | null;
  createAt?: string  | null;
  updateBy?: string | null;
  updateAt?: string | null;
  isDeleted: boolean;
  deleteBy?: string | null;
  deleteAt?: string | null;
  categoryId: number|null;

  theSmallestPossibleQuantity:number|null;

productCode:string;
}
export interface ProductFilterationDto {
  name: string | null;
  isDeleted: boolean | null;

  pageSize: number|null;
  page: number|null;
}
export interface ProductWithSupplierDto
{
   productId: number|null;
    productName: string|null
}

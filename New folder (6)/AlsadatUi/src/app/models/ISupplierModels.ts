import { ProductWithSupplierDto } from "./IProductVM"

export interface SupplierDto {
  id?: number|null
  name: string
  phoneNumbers: string
  address?: string|null
  cityId: number
  cityName: string
  isDeleted: boolean
  products: ProductWithSupplierDto[]|null
}

export interface SupplierFilteration {
  name: string|null
  phoneNumbers?: string|null
  isDeleted?: boolean|null;
  page?: number|null
  pageSize?: number|null
}

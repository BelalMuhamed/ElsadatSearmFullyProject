export interface DistributorsAndMerchantsDto {
  userId?: string | null;
  fullName: string;
  address: string;
  gender?: number | null;
  type: number; // 0: Distributor, 1: Merchant
  createdAt?: string | null; // Use string for ISO date
  createdBy?: string | null;
  updatedAt?: string | null;
  updatedBy?: string | null;
  isDelted?: boolean | null;
  deletedAt?: string | null;
  deletedBy?: string | null;
  cityId?: number | null;
  cityName?: string | null;
  phoneNumber?: string | null; // phone/email/username
  password?: string | null;
  pointsBalance?: number | null;
  cashBalance?: number | null;
  indebtedness?: number | null;
  firstSpecialDiscount?:number|null;
  secondSpecialDiscount?:number|null;
  thirdSpecialDiscount?:number|null;
}
export interface DistributorsAndMerchantsFilters {
  phoneNumber?: string | null;
  fullName?: string | null;
  cityName?: string | null;
  type?: number | null;
  isDeleted:boolean|null;
  page: number|null;
  pageSize: number|null;
}

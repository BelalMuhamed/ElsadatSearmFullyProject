export interface StoreDto {
  id: number|null;
  storeName: string;
  isDeleted:boolean|null;
}

export interface StoreDeleteDto {
  id: number;
  storeName: string|null;
  transferedToStoreDto: number;
  makeActionUser:string|null
}

export interface StoreFilteration {
  storeName: string|null;
  isDeleted:boolean|null;
  page: number|null;
  pageSize: number|null;
}

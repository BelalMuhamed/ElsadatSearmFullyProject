export interface PublicHoliday {
  id: number;
  name: string;
  date: string; // ISO format: YYYY-MM-DD
  createdAt: Date;
  createBy?: string;
  updateBy?: string;
  updateAt?: Date;
  isDeleted: boolean;
  deleteBy?: string;
  deleteAt?: Date;
}

export interface IPublicHoliday extends PublicHoliday {}

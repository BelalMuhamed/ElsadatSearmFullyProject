export enum Gender {
  Male = 0,
  Female = 1
}

export enum RepresentiveType {
  None = 0,
  Inspection = 1,
  Collection = 2,
  Mixed = 3
}

export enum WeekDays {
  Saturday = 0,
  Sunday = 1,
  Monday = 2,
  Tuesday = 3,
  Wednesday = 4,
  Thursday = 5,
  Friday = 6
}

export interface SpecialRepresentiveCityDto {
  id: number;
  cityId: number;
  cityName?: string;
}

export interface RepresentativeDTo {
  userId?: string;
  email: string;
  password: string;
  fullName: string;
  phoneNumber?: string;
  address?: string;
  sno?: string;
  gender: Gender;
  representiveType: RepresentiveType;
  createBy?: string;
  createAt?: Date;
  nameOfCreatedBy?: string;
  updateBy?: string;
  updateAt?: Date;
  isDeleted: boolean;
  deleteBy?: string;
  deleteAt?: Date;
  cityID: number;
  cityName?: string;
  representativeCode?: string;
  pointsWallet: number;
  moneyDeposit: number;
  overtimeRatePerHour: number;
  birthDate: Date | string;
  hireDate: Date | string;
  salary: number;
  timeIn: string;
  timeOut: string;
  weekHoliday1: WeekDays;
  weekHoliday2?: WeekDays;
  roleName?: string;
  roleId?: string;
  rolesName: string[];
  rolesId: string[];
  specialRepresentiveCities: SpecialRepresentiveCityDto[];
}

export interface RepresentativeHelper {
  representativeCode?: string;
  representativeName?: string;
  cityName?: string;
  isActive: boolean;
  representiveType: RepresentiveType;
}

export interface PaginationParams {
  pageNumber: number;
  pageSize: number;
}

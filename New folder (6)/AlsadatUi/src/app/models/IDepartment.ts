export interface Department {
  id: number;
  name: string;
  createdAt: Date;
  createBy?: string;
  updateBy?: string;
  updateAt?: Date;
  isDeleted: boolean;
  deleteBy?: string;
  deleteAt?: Date;
}

export interface IDepartment extends Department {}

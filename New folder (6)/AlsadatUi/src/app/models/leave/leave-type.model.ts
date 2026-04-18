export interface LeaveTypeDto {
  id: number;
  name: string;
  isPaid: boolean;
  createdAt?: string | Date;
  createdBy?: string;
  updatedAt?: string | Date | null;
  updatedBy?: string | null;
}

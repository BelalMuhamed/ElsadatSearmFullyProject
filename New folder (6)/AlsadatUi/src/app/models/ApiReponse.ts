export interface ApiResponse<T>
{
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  data: T;
}
export interface Result<T> {
  isSuccess: boolean;
  message?: string;
  statusCode: number;
  data?: T;
}

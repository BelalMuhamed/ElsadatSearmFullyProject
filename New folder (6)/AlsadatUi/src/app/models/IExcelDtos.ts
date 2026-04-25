import { Observable } from "rxjs";
import { Result } from "./ApiReponse";

export interface ExcelImportRowError {
  rowNumber: number;
  column: string;
  message: string;
}
export interface ExcelImportResult<T> {
  totalRows: number;
  successCount: number;
  failedCount: number;

  imported: T[];
  errors: ExcelImportRowError[];
}
export interface ImportExcelConfig<T> {
  title: string;
  fileHint: string;
  templateName: string;

  importFn: (file: File) => Observable<Result<ExcelImportResult<T>>>;

  columns: string[];
}

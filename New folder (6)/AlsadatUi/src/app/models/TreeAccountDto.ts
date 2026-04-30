// =============================================================================
// File: src/app/models/TreeAccountDto.ts (REPLACE the whole file)
// =============================================================================
// Adds `isSystemAccount` to TreeAccountDto and AccountDto.
// All other interfaces unchanged so nothing else breaks.

import { ApiResponse } from './ApiReponse';

export interface TreeAccountDto {
  id?: number;
  accountName: string;
  debit?: number;
  isLeaf: boolean;
  credit?: number;
  accountCode: string | null;
  parentId: number | null;
  isActive: boolean;
  isSystemAccount?: boolean;          // NEW — optional so old payloads don't break
  children: TreeAccountDto[];
  expanded?: boolean;
  level?: number;
}

export interface DisAndMerchAccountDto {
  accountCode: string;
  userId: string | null;
  accountName: string;
  type: number;
  parentAccountId: number;
  isLeaf: boolean;
  isActive: boolean;
  debit: number;
  credit: number;
}

export interface AccountDto {
  id: number;
  accountCode: string;
  userId: string | null;
  accountName: string;
  type: number;
  parentAccountId: number | null;
  isLeaf: boolean;
  isActive: boolean;
  isSystemAccount?: boolean;          // NEW — read-only on UI
}

export interface FilterationAccountsDto {
  accountCode: string | null;
  accountName: string | null;
  type: number | null;
  parentAccountId: number | null;
  isLeaf: boolean | null;
  isActive: boolean | null;
  page: number | null;
  pageSize: number | null;
}

export interface AccountDetailsDto {
  accountId: number;
  accountName: string;
  accountCode: string;
  type: number;
  isActive: boolean;
  currentBalance: number;
  movements: ApiResponse<AccountMovementDto[]>;
}

export interface AccountMovementDto {
  entryId: number;
  entryDate: string;
  description: string;
  debit: number;
  credit: number;
  runningBalance: number;
}

export interface AccountDetailsDtoReq {
  accountId: number;
  page: number;
  pageSize: number;
  entryId: number | null;
  entryDate: string | null;
}

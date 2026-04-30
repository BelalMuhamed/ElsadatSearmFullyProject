export interface DateRangeReq {
  fromDate: string | null;
  toDate: string | null;
}

export interface CashReportReq extends DateRangeReq {
  direction: 0 | 1 | 2;     // 0 = all, 1 = incoming, 2 = outgoing
  page: number;
  pageSize: number;
}

export interface CashMovement {
  journalEntryId: number;
  entryDate: string;
  description: string;
  referenceType: string | null;
  referenceNo: string | null;
  incoming: number;
  outgoing: number;
}
export interface CashReport {
  openingBalance: number;
  totalIncoming: number;
  totalOutgoing: number;
  closingBalance: number;
  movements: CashMovement[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface PartyBalance {
  accountId: number;
  accountCode: string;
  accountName: string;
  userId: string | null;
  totalDebit: number;
  totalCredit: number;
  balance: number;
  lastTransactionDate: string | null;
}
export interface PartyBalancesReport {
  totalReceivables: number;
  totalPayables: number;
  parties: PartyBalance[];
}

export interface AgingReportReq {
  asOfDate: string | null;
  bucket1Days: number;
  bucket2Days: number;
  bucket3Days: number;
}
export interface AgingRow {
  accountId: number;
  accountName: string;
  current: number;
  bucket1: number;
  bucket2: number;
  bucket3: number;
  over: number;
  total: number;
}
export interface AgingReport {
  asOfDate: string;
  rows: AgingRow[];
  totals: AgingRow;
}

export interface InventoryMovementReq extends DateRangeReq {
  page: number;
  pageSize: number;
}
export interface InventoryMovementRow {
  date: string;
  referenceType: string;
  referenceNo: string;
  description: string;
  stockIn: number;
  stockOut: number;
}
export interface InventoryMovement {
  openingValue: number;
  totalIn: number;
  totalOut: number;
  closingValue: number;
  rows: InventoryMovementRow[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface TrialBalanceRow {
  accountCode: string;
  accountName: string;
  type: number;
  debit: number;
  credit: number;
}
export interface TrialBalance {
  asOfDate: string;
  rows: TrialBalanceRow[];
  totalDebit: number;
  totalCredit: number;
  isBalanced: boolean;
}

export interface IncomeStatement {
  fromDate: string;
  toDate: string;
  totalRevenue: number;
  totalCogs: number;
  grossProfit: number;
  totalExpenses: number;
  netIncome: number;
  revenueLines: TrialBalanceRow[];
  expenseLines: TrialBalanceRow[];
}
 
export interface JournalEntriesDto {
  id: number;
  entryDate: string;
  referenceType: number | null;
  desc: string;
  referenceNo: string | null;
  isPosted: boolean | null;
  postedDate: string | null;
  entryDetails: JournalEntryDetailsDto[];
}

export interface JournalEntryDetailsDto {
  id: number|null;
  accountId: number;
  accountCode: string|null;
  accountName: string|null;
  accountType: number|null;
  isLeaf: boolean;
  debit: number;
  credit: number;
}

export interface JournalEntryFilterationReq {
  entryDate: string|null;
  referenceType: number | null;
  referenceNo: string | null;
  isPosted: boolean | null;
  postedDate: string | null;
  page: number;
  pageSize: number;
}

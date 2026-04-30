import { TestBed } from '@angular/core/testing';

import { FinancialReportsService } from './financial-reports.service';

describe('FinancialReportsService', () => {
  let service: FinancialReportsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(FinancialReportsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

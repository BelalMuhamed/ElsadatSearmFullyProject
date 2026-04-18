import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ReportsDashBoard } from './reports-dash-board';

describe('ReportsDashBoard', () => {
  let component: ReportsDashBoard;
  let fixture: ComponentFixture<ReportsDashBoard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReportsDashBoard]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ReportsDashBoard);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

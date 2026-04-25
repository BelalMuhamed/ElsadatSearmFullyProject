import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ImportExcelDialogComponent } from './import-excel-dialog.component';

describe('ImportExcelDialogComponent', () => {
  let component: ImportExcelDialogComponent;
  let fixture: ComponentFixture<ImportExcelDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ImportExcelDialogComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ImportExcelDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlumberImportDialogComponent } from './plumber-import-dialog.component';

describe('PlumberImportDialogComponent', () => {
  let component: PlumberImportDialogComponent;
  let fixture: ComponentFixture<PlumberImportDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlumberImportDialogComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(PlumberImportDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

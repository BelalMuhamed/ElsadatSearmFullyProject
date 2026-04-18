import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddEditGovernratePopupComponent } from './add-edit-governrate-popup.component';

describe('AddEditGovernratePopupComponent', () => {
  let component: AddEditGovernratePopupComponent;
  let fixture: ComponentFixture<AddEditGovernratePopupComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddEditGovernratePopupComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(AddEditGovernratePopupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

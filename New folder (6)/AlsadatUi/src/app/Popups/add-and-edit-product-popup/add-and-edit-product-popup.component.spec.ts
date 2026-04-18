import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddAndEditProductPopupComponent } from './add-and-edit-product-popup.component';

describe('AddAndEditProductPopupComponent', () => {
  let component: AddAndEditProductPopupComponent;
  let fixture: ComponentFixture<AddAndEditProductPopupComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddAndEditProductPopupComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(AddAndEditProductPopupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

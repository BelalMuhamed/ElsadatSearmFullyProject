import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BillDiscountComponent } from './bill-discount-component';

describe('BillDiscountComponent', () => {
  let component: BillDiscountComponent;
  let fixture: ComponentFixture<BillDiscountComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BillDiscountComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BillDiscountComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

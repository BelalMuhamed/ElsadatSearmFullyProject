import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PurchaseInvoiceDetailsComponent } from './purchase-invoice-details.component';

describe('PurchaseInvoiceDetailsComponent', () => {
  let component: PurchaseInvoiceDetailsComponent;
  let fixture: ComponentFixture<PurchaseInvoiceDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PurchaseInvoiceDetailsComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(PurchaseInvoiceDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

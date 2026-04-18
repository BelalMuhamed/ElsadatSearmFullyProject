import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddEditSalesInvoiceComponent } from './add-edit-sales-invoice.component';

describe('AddEditSalesInvoiceComponent', () => {
  let component: AddEditSalesInvoiceComponent;
  let fixture: ComponentFixture<AddEditSalesInvoiceComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddEditSalesInvoiceComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(AddEditSalesInvoiceComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

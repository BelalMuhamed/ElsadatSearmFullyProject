import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InvoiceConfirmationComponent } from './invoice-confirmation.component';

describe('InvoiceConfirmationComponent', () => {
  let component: InvoiceConfirmationComponent;
  let fixture: ComponentFixture<InvoiceConfirmationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InvoiceConfirmationComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(InvoiceConfirmationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

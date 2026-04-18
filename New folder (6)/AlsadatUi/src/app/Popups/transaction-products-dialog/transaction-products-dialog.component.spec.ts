import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TransactionProductsDialogComponent } from './transaction-products-dialog.component';

describe('TransactionProductsDialogComponent', () => {
  let component: TransactionProductsDialogComponent;
  let fixture: ComponentFixture<TransactionProductsDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TransactionProductsDialogComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(TransactionProductsDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

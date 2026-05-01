import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StockTransferLineComponent } from './stock-transfer-line.component';

describe('StockTransferLineComponent', () => {
  let component: StockTransferLineComponent;
  let fixture: ComponentFixture<StockTransferLineComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StockTransferLineComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(StockTransferLineComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

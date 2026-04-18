import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DisAndMerchantComponent } from './dis-and-merchant.component';

describe('DisAndMerchantComponent', () => {
  let component: DisAndMerchantComponent;
  let fixture: ComponentFixture<DisAndMerchantComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DisAndMerchantComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(DisAndMerchantComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

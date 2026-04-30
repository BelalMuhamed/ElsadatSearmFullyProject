import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PartyBalancesComponent } from './party-balances.component';

describe('PartyBalancesComponent', () => {
  let component: PartyBalancesComponent;
  let fixture: ComponentFixture<PartyBalancesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PartyBalancesComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(PartyBalancesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

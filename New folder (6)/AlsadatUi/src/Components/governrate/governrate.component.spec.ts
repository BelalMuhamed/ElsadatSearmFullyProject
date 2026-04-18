import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GovernrateComponent } from './governrate.component';

describe('GovernrateComponent', () => {
  let component: GovernrateComponent;
  let fixture: ComponentFixture<GovernrateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GovernrateComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(GovernrateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

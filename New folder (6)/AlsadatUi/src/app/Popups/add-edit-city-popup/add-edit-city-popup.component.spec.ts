import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddEditCityPopupComponent } from './add-edit-city-popup.component';

describe('AddEditCityPopupComponent', () => {
  let component: AddEditCityPopupComponent;
  let fixture: ComponentFixture<AddEditCityPopupComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddEditCityPopupComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(AddEditCityPopupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

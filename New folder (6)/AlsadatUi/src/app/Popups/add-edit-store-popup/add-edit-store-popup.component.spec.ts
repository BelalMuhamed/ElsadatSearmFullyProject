import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddEditStorePopupComponent } from './add-edit-store-popup.component';

describe('AddEditStorePopupComponent', () => {
  let component: AddEditStorePopupComponent;
  let fixture: ComponentFixture<AddEditStorePopupComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddEditStorePopupComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(AddEditStorePopupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

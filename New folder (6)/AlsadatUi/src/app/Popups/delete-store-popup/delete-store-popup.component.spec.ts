import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DeleteStorePopupComponent } from './delete-store-popup.component';

describe('DeleteStorePopupComponent', () => {
  let component: DeleteStorePopupComponent;
  let fixture: ComponentFixture<DeleteStorePopupComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DeleteStorePopupComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(DeleteStorePopupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

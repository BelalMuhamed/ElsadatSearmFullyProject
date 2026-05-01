import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddEditPlumberComponent } from './add-edit-plumber.component';

describe('AddEditPlumberComponent', () => {
  let component: AddEditPlumberComponent;
  let fixture: ComponentFixture<AddEditPlumberComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddEditPlumberComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(AddEditPlumberComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

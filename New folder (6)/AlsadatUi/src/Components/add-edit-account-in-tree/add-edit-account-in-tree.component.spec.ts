import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddEditAccountInTreeComponent } from './add-edit-account-in-tree.component';

describe('AddEditAccountInTreeComponent', () => {
  let component: AddEditAccountInTreeComponent;
  let fixture: ComponentFixture<AddEditAccountInTreeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddEditAccountInTreeComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(AddEditAccountInTreeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

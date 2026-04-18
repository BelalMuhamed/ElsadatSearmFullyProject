import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddCopounCoponentPopup } from './add-copoun-coponent-popup';

describe('AddCopounCoponentPopup', () => {
  let component: AddCopounCoponentPopup;
  let fixture: ComponentFixture<AddCopounCoponentPopup>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddCopounCoponentPopup]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddCopounCoponentPopup);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

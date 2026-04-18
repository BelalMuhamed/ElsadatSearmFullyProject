import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddEditJournalEntryComponent } from './add-edit-journal-entry.component';

describe('AddEditJournalEntryComponent', () => {
  let component: AddEditJournalEntryComponent;
  let fixture: ComponentFixture<AddEditJournalEntryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddEditJournalEntryComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(AddEditJournalEntryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

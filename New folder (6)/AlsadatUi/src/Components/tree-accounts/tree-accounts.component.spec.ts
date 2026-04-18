import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TreeAccountsComponent } from './tree-accounts.component';

describe('TreeAccountsComponent', () => {
  let component: TreeAccountsComponent;
  let fixture: ComponentFixture<TreeAccountsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TreeAccountsComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(TreeAccountsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

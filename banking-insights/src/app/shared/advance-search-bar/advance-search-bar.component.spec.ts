import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdvanceSearchBarComponent } from './advance-search-bar.component';

describe('AdvanceSearchBarComponent', () => {
  let component: AdvanceSearchBarComponent;
  let fixture: ComponentFixture<AdvanceSearchBarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdvanceSearchBarComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdvanceSearchBarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

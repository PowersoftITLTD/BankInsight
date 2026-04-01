import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ViewIconDetailsComponent } from './view-icon-details.component';

describe('ViewIconDetailsComponent', () => {
  let component: ViewIconDetailsComponent;
  let fixture: ComponentFixture<ViewIconDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ViewIconDetailsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ViewIconDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

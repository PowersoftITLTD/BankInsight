import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PrimeTreeTableComponent } from './prime-tree-table.component';

describe('PrimeTreeTableComponent', () => {
  let component: PrimeTreeTableComponent;
  let fixture: ComponentFixture<PrimeTreeTableComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PrimeTreeTableComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PrimeTreeTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

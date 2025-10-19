import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlanClosureComponent } from './plan-closure.component';

describe('PlanClosure', () => {
  let component: PlanClosureComponent;
  let fixture: ComponentFixture<PlanClosureComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlanClosureComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PlanClosureComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

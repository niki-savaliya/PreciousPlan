import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddSavingPlanComponent } from './add-saving-plan.component';

describe('AddSavingPlan', () => {
  let component: AddSavingPlanComponent;
  let fixture: ComponentFixture<AddSavingPlanComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddSavingPlanComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddSavingPlanComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

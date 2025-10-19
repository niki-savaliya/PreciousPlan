import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SavingsPlanDetailComponent } from './savings-plan-detail.component';

describe('SavingsPlanDetail', () => {
  let component: SavingsPlanDetailComponent;
  let fixture: ComponentFixture<SavingsPlanDetailComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SavingsPlanDetailComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SavingsPlanDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SavingsPlanListComponent } from './savings-plan-list.component';

describe('SavingsPlanList', () => {
  let component: SavingsPlanListComponent;
  let fixture: ComponentFixture<SavingsPlanListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SavingsPlanListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SavingsPlanListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

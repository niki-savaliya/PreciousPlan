import { ComponentFixture, TestBed } from '@angular/core/testing';

import { KpiSummary } from './kpi-summary';

describe('KpiSummary', () => {
  let component: KpiSummary;
  let fixture: ComponentFixture<KpiSummary>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [KpiSummary]
    })
    .compileComponents();

    fixture = TestBed.createComponent(KpiSummary);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

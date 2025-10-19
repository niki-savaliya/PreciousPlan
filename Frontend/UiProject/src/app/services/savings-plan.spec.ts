import { TestBed } from '@angular/core/testing';

import { SavingsPlanService } from './savings-plan';

describe('SavingsPlan', () => {
  let service: SavingsPlanService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SavingsPlanService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

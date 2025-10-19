import { TestBed } from '@angular/core/testing';

import { QuarterlyFeesService } from './quarterly-fees';

describe('QuarterlyFees', () => {
  let service: QuarterlyFeesService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(QuarterlyFeesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

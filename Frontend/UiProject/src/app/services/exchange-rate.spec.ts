import { TestBed } from '@angular/core/testing';
import { ExchangeRateService } from './exchange-rate';

describe('ExchangeRate', () => {
  let service: ExchangeRateService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ExchangeRateService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

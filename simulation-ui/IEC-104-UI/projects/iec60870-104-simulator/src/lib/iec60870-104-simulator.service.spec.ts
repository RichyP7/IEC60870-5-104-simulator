import { TestBed } from '@angular/core/testing';

import { DataPointsService } from './data/datapoints.service';

describe('Iec60870104SimulatorService', () => {
  let service: DataPointsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DataPointsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

import { TestBed } from '@angular/core/testing';

import { Iec60870104SimulatorService } from './iec60870-104-simulator.service';

describe('Iec60870104SimulatorService', () => {
  let service: Iec60870104SimulatorService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Iec60870104SimulatorService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

import { TestBed } from '@angular/core/testing';

import NewserviceService from './newservice.service';

describe('NewserviceService', () => {
  let service: NewserviceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(NewserviceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

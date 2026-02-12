import { TestBed } from '@angular/core/testing';
import { WrapperWorkAroundService } from './wrapperworkaround.service';


describe('WrapperWorkAroundService', () => {
  let service: WrapperWorkAroundService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(WrapperWorkAroundService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DatapointDetailsComponent } from './datapoint-details.component';

describe('DatapointDetailsComponent', () => {
  let component: DatapointDetailsComponent;
  let fixture: ComponentFixture<DatapointDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DatapointDetailsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DatapointDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Iec60870104SimulatorComponent } from './iec60870-104-simulator.component';

describe('Iec60870104SimulatorComponent', () => {
  let component: Iec60870104SimulatorComponent;
  let fixture: ComponentFixture<Iec60870104SimulatorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Iec60870104SimulatorComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Iec60870104SimulatorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

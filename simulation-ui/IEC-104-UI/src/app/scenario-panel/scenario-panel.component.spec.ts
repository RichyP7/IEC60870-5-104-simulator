import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ScenarioPanelComponent } from './scenario-panel.component';
import { ScenarioService, ScenarioInfo } from '../services/scenario.service';
import { SignalRService, ScenarioUpdate } from '../services/signal-r.service';
import { provideHttpClient } from '@angular/common/http';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { Subject, BehaviorSubject, of } from 'rxjs';

describe('ScenarioPanelComponent', () => {
  let component: ScenarioPanelComponent;
  let fixture: ComponentFixture<ScenarioPanelComponent>;
  let scenarioUpdate$: Subject<ScenarioUpdate>;
  let scenarioServiceStub: {
    getScenarios: ReturnType<typeof vi.fn>;
    getScenarioDefinitions: ReturnType<typeof vi.fn>;
    triggerScenario: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    scenarioUpdate$ = new Subject<ScenarioUpdate>();

    scenarioServiceStub = {
      getScenarios: vi.fn().mockReturnValue(of<ScenarioInfo[]>([
        { name: 'ca1-transformer-trip', status: 'Idle', remainingMs: 0 }
      ])),
      getScenarioDefinitions: vi.fn().mockReturnValue(of([])),
      triggerScenario: vi.fn().mockReturnValue(of({})),
    };

    const signalRStub = {
      scenarioUpdate$,
      fullSnapshot$: new Subject(),
      dataPointChanged$: new Subject(),
      clientCount$: new BehaviorSubject(0),
      connected$: new BehaviorSubject(false),
    };

    await TestBed.configureTestingModule({
      imports: [ScenarioPanelComponent],
      providers: [
        provideHttpClient(),
        provideNoopAnimations(),
        { provide: ScenarioService, useValue: scenarioServiceStub },
        { provide: SignalRService, useValue: signalRStub },
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ScenarioPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load scenarios on init', () => {
    expect(scenarioServiceStub.getScenarios).toHaveBeenCalled();
    expect(component.scenarios.length).toBe(1);
    expect(component.scenarios[0].name).toBe('ca1-transformer-trip');
  });

  it('should disable trigger button while scenario is Running', () => {
    scenarioUpdate$.next({
      name: 'ca1-transformer-trip',
      status: 'Running',
      remainingMs: 25000
    });
    fixture.detectChanges();

    expect(component.isRunning('ca1-transformer-trip')).toBe(true);
  });

  it('should show countdown when Running', () => {
    scenarioUpdate$.next({
      name: 'ca1-transformer-trip',
      status: 'Running',
      remainingMs: 15000
    });
    fixture.detectChanges();

    expect(component.getCountdownSec('ca1-transformer-trip')).toBeGreaterThan(0);
  });

  it('should re-enable trigger button after scenario Completes', () => {
    scenarioUpdate$.next({
      name: 'ca1-transformer-trip', status: 'Running', remainingMs: 1000
    });
    fixture.detectChanges();
    expect(component.isRunning('ca1-transformer-trip')).toBe(true);

    scenarioUpdate$.next({
      name: 'ca1-transformer-trip', status: 'Completed', remainingMs: 0
    });
    fixture.detectChanges();
    expect(component.isRunning('ca1-transformer-trip')).toBe(false);
  });
});

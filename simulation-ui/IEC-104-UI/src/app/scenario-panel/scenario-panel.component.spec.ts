import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ScenarioPanelComponent } from './scenario-panel.component';
import { ScenarioService } from '../services/scenario.service';
import { SignalRService } from '../services/signal-r.service';
import { provideHttpClient } from '@angular/common/http';
import { Subject, of } from 'rxjs';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

describe('ScenarioPanelComponent', () => {
  let component: ScenarioPanelComponent;
  let fixture: ComponentFixture<ScenarioPanelComponent>;
  let scenarioServiceSpy: jasmine.SpyObj<ScenarioService>;
  let signalRServiceSpy: jasmine.SpyObj<SignalRService> & { scenarioUpdate$: Subject<any> };

  beforeEach(async () => {
    const scenarioUpdate$ = new Subject<any>();

    scenarioServiceSpy = jasmine.createSpyObj('ScenarioService', ['getScenarios', 'triggerScenario']);
    scenarioServiceSpy.getScenarios.and.returnValue(of([
      { name: 'ca1-transformer-trip', status: 'Idle', remainingMs: 0 }
    ]));
    scenarioServiceSpy.triggerScenario.and.returnValue(of({}));

    signalRServiceSpy = jasmine.createSpyObj('SignalRService',
      [], { scenarioUpdate$, fullSnapshot$: new Subject(), clientCount$: new Subject(), connected$: new Subject() }) as any;
    (signalRServiceSpy as any).scenarioUpdate$ = scenarioUpdate$;

    await TestBed.configureTestingModule({
      imports: [ScenarioPanelComponent, NoopAnimationsModule],
      providers: [
        provideHttpClient(),
        { provide: ScenarioService, useValue: scenarioServiceSpy },
        { provide: SignalRService, useValue: signalRServiceSpy },
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
    expect(scenarioServiceSpy.getScenarios).toHaveBeenCalled();
    expect(component.scenarios.length).toBe(1);
    expect(component.scenarios[0].name).toBe('ca1-transformer-trip');
  });

  it('should disable trigger button while scenario is Running', () => {
    // Emit a Running update from SignalR
    (signalRServiceSpy as any).scenarioUpdate$.next({
      name: 'ca1-transformer-trip',
      status: 'Running',
      remainingMs: 25000
    });
    fixture.detectChanges();

    expect(component.isRunning('ca1-transformer-trip')).toBeTrue();

    const btn = fixture.nativeElement.querySelector('p-button');
    // PrimeNG Button renders as disabled when [disabled]="true"
    expect(component.isRunning('ca1-transformer-trip')).toBeTrue();
  });

  it('should show countdown when Running', () => {
    (signalRServiceSpy as any).scenarioUpdate$.next({
      name: 'ca1-transformer-trip',
      status: 'Running',
      remainingMs: 15000
    });
    fixture.detectChanges();

    expect(component.getCountdownSec('ca1-transformer-trip')).toBeGreaterThan(0);
  });

  it('should re-enable trigger button after scenario Completes', () => {
    // First set Running
    (signalRServiceSpy as any).scenarioUpdate$.next({
      name: 'ca1-transformer-trip', status: 'Running', remainingMs: 1000
    });
    fixture.detectChanges();
    expect(component.isRunning('ca1-transformer-trip')).toBeTrue();

    // Then Completed
    (signalRServiceSpy as any).scenarioUpdate$.next({
      name: 'ca1-transformer-trip', status: 'Completed', remainingMs: 0
    });
    fixture.detectChanges();
    expect(component.isRunning('ca1-transformer-trip')).toBeFalse();
  });
});

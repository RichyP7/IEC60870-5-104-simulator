import {
  AfterViewInit,
  ChangeDetectorRef,
  Component, Inject,
  OnDestroy,
  OnInit,
  PLATFORM_ID
} from '@angular/core';
import {Menubar} from "primeng/menubar";
import {PrimeTemplate} from "primeng/api";
import {ToggleButton} from "primeng/togglebutton";
import {Tooltip} from 'primeng/tooltip';
import {FormsModule} from '@angular/forms';
import {DataService, SimulationState} from '../list-view/DataService/data.service';
import {DatePipe, NgStyle} from '@angular/common';
import {interval, of, startWith, Subject, Subscription, tap, throttleTime} from 'rxjs';
import {CycleSent, SignalRService} from '../services/signal-r.service';

@Component({
    selector: 'app-header',
    imports: [
        Menubar,
        PrimeTemplate,
        ToggleButton,
        Tooltip,
        FormsModule,
        NgStyle,
        DatePipe,
    ],
    templateUrl: './header.component.html',
    styleUrl: './header.component.scss'
})
export class HeaderComponent implements OnInit, OnDestroy, AfterViewInit {
  isSimulating: boolean = false;
  isHealthy: boolean = false;
  isConnected: boolean = false;

  // ── "Datapoints sent" indicator ────────────────────────────────────────────
  /** Info about the most recent send cycle (null until the first cycle arrives). */
  lastCycle: CycleSent | null = null;
  /** Local timestamp of the last send, for display. */
  lastSentAt: Date | null = null;
  /** True for a short moment after each (throttled) send event, drives the pulse animation. */
  sendPulse = false;
  private pulseTimeout: ReturnType<typeof setTimeout> | null = null;
  private cycleSub: Subscription | null = null;

  INTERVAL: number = 5000;
  closeTimer$ = new Subject<any>();

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private dataService: DataService,
    private signalR: SignalRService,
    private cdr: ChangeDetectorRef,
  ) {}


  ngOnInit(): void {
    this.fetchCurrentSimulationEngineState();
    this.fetchCurrentHealthState();
    this.fetchCurrentConnectedState();

    // Cycles can be very fast (down to 10 ms); throttle so the indicator stays calm.
    this.cycleSub = this.signalR.cycleSent$
      .pipe(throttleTime(1000))
      .subscribe(info => this.onCycleSent(info));
  }

  private onCycleSent(info: CycleSent): void {
    this.lastCycle = info;
    this.lastSentAt = new Date();
    this.sendPulse = false;
    this.cdr.detectChanges(); // restart CSS animation by toggling the class off/on
    this.sendPulse = true;
    if (this.pulseTimeout) clearTimeout(this.pulseTimeout);
    this.pulseTimeout = setTimeout(() => {
      this.sendPulse = false;
      this.cdr.markForCheck();
    }, 700);
    this.cdr.markForCheck();
  }

  get sendIndicatorTooltip(): string {
    if (!this.lastCycle || !this.lastSentAt) return 'No datapoints sent yet';
    const clients = this.lastCycle.activeIecClients;
    return `${this.lastCycle.sentCount} datapoint value(s) sent to ${clients} IEC-104 client(s) at ${this.lastSentAt.toLocaleTimeString()}`;
  }

  fetchCurrentSimulationEngineState() {
    let simulationState: SimulationState | null = null;
    this.dataService.fetchSimulationEngineState()
      .subscribe({
        next: (data) => {
          simulationState = data;
          this.isSimulating = simulationState != null && simulationState === SimulationState.Running;
        },
        error: (err) => {
          console.error('Error fetching Simulation Engine State', err);
        }
      });
  }

  fetchCurrentHealthState() {
    this.dataService.fetchHealthState()
      .subscribe({
        next: (data) => {
          this.isHealthy = data === "Healthy";
          this.closeTimer$.next("");
        },
        error: (err) => {
          this.isHealthy = false;
          console.error('Error fetching Health State', err);
        }
      });
    return of(true);
  }

  fetchCurrentConnectedState() {
    this.dataService.fetchConnectionState()
      .subscribe({
        next: (data) => {
          this.isConnected = data === "Healthy";
          this.closeTimer$.next("");
        },
        error: (err) => {
          this.isConnected = false;
          console.error('Error fetching Connection State', err);
        }
      });
    return of(true);
  }


  updateSimulationState(state: boolean) {
    const wantedSimulationState: SimulationState = state ? SimulationState.Running : SimulationState.Stopped;
    this.dataService.updateSimulationEngineState(wantedSimulationState);

  }

  ngOnDestroy(): void {
    this.closeTimer$.next("stop");
    this.cycleSub?.unsubscribe();
    if (this.pulseTimeout) clearTimeout(this.pulseTimeout);
  }

  ngAfterViewInit(): void {
    interval(this.INTERVAL)
      .pipe(
        startWith(0),
        tap(() => {
          this.fetchCurrentHealthState();
          this.fetchCurrentConnectedState()
        })
      )
      .subscribe();
  }
}

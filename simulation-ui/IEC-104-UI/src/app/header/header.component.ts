import {
  AfterViewInit,
  Component, Inject,
  OnDestroy,
  OnInit,
  PLATFORM_ID
} from '@angular/core';
import {Menubar} from "primeng/menubar";
import {PrimeTemplate} from "primeng/api";
import {ToggleButton} from "primeng/togglebutton";
import {FormsModule} from '@angular/forms';
import {DataService, SimulationState} from '../list-view/DataService/data.service';
import {NgStyle} from '@angular/common';
import {interval, of, startWith, Subject, tap} from 'rxjs';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    Menubar,
    PrimeTemplate,
    ToggleButton,
    FormsModule,
    NgStyle,
  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent implements OnInit, OnDestroy, AfterViewInit {
  isSimulating: boolean = false;
  isHealthy: boolean = false;
  isConnected: boolean = false;

  INTERVAL: number = 5000;
  closeTimer$ = new Subject<any>();

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private dataService: DataService,
  ) {}


  ngOnInit(): void {
    this.fetchCurrentSimulationEngineState();
    this.fetchCurrentHealthState();

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

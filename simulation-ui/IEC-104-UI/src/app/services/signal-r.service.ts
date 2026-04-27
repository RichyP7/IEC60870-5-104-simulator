import { Injectable, PLATFORM_ID, Inject, OnDestroy } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Subject, BehaviorSubject } from 'rxjs';
import { environment } from '../../environments/environment.development';

export interface DataPointUpdate {
  stationaryAddress: number;
  objectAddress: number;
  id: string;
  iec104DataType: string;
  mode: string;
  value: unknown;
  frozen: boolean;
}

export interface ScenarioUpdate {
  name: string;
  status: string;
  remainingMs: number;
}

@Injectable({ providedIn: 'root' })
export class SignalRService implements OnDestroy {
  private connection: import('@microsoft/signalr').HubConnection | null = null;

  readonly fullSnapshot$ = new Subject<DataPointUpdate[]>();
  readonly dataPointChanged$ = new Subject<DataPointUpdate>();
  readonly scenarioUpdate$ = new Subject<ScenarioUpdate>();
  readonly clientCount$ = new BehaviorSubject<number>(0);
  readonly connected$ = new BehaviorSubject<boolean>(false);

  constructor(@Inject(PLATFORM_ID) private platformId: Object) {
    if (isPlatformBrowser(this.platformId)) {
      this.connect();
    }
  }

  private async connect(): Promise<void> {
    // Dynamic import keeps the SSR build clean (no window references on server)
    const { HubConnectionBuilder, LogLevel } = await import('@microsoft/signalr');

    this.connection = new HubConnectionBuilder()
      .withUrl(environment.HUB_ENDPOINT)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    this.connection.on('FullSnapshot', (updates: DataPointUpdate[]) => {
      this.fullSnapshot$.next(updates);
    });

    this.connection.on('DataPointChanged', (update: DataPointUpdate) => {
      this.dataPointChanged$.next(update);
    });

    this.connection.on('ScenarioUpdate', (update: ScenarioUpdate) => {
      this.scenarioUpdate$.next(update);
    });

    this.connection.on('ClientCountUpdate', (count: number) => {
      this.clientCount$.next(count);
    });

    this.connection.onclose(() => this.connected$.next(false));
    this.connection.onreconnected(() => this.connected$.next(true));

    try {
      await this.connection.start();
      this.connected$.next(true);
    } catch (err) {
      console.error('SignalR connection failed:', err);
      this.connected$.next(false);
    }
  }

  ngOnDestroy(): void {
    this.connection?.stop();
  }
}

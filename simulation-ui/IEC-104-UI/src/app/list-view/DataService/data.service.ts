import {Injectable} from '@angular/core';
import {BehaviorSubject, catchError, Observable, of} from 'rxjs';
import {HttpClient} from '@angular/common/http';
import {DataPoint} from '../list-view.component';
import {environment} from '../../../environments/environment.development';
import {MessageService} from 'primeng/api';

@Injectable({
  providedIn: 'root',
})
export class DataService {
  private dataSubject = new BehaviorSubject<DataPoint[]>([]);
  data$ : Observable<DataPoint[]> = this.dataSubject.asObservable();

  private errorSubject = new BehaviorSubject<string | null>(null);
  public error$ = this.errorSubject.asObservable();

  constructor(private http: HttpClient, private messageService: MessageService) {}

  fetchData(): void {
    this.http.get<DataPoint[]>(environment.API_ENDPOINT + 'DataPointConfigs').subscribe((data) => {
      this.dataSubject.next(data);
    });
  }

  toggleSimulationMode(dataPoint: DataPoint) {
    const update = {
      mode: dataPoint.mode,
      profileName: dataPoint.profileName ?? null
    };
    this.http.put<DataPoint>(`${environment.API_ENDPOINT}DataPointConfigs/${dataPoint.stationaryAddress}/${dataPoint.objectAddress}/simulation-mode`, update)
      .subscribe((data) => {
        const currentData = this.dataSubject.getValue();
        const updatedDataList = currentData.map((dp) =>
          dp.objectAddress === data.objectAddress && dp.stationaryAddress === data.stationaryAddress ? data : dp
        );
        this.dataSubject.next(updatedDataList);
    });
  }

  fetchProfiles(): Observable<string[]> {
    return this.http.get<string[]>(`${environment.API_ENDPOINT}Profiles`);
  }

  updateSimulationEngineState(simulationState: SimulationState) {
    let command = (simulationState === SimulationState.Stopped) ? 'Stop' : 'Start';
    this.http.post<SimulationState>(`${environment.API_ENDPOINT}SimulationEngineState?command=${command}`, null)
      .subscribe({
        next: () => {  },
        error: (err) => {
          console.error('Error while updating simulation state:', err);
        }
      });
  }

  createDataPoint(datapoint: DataPoint): Observable<DataPoint> {
    return this.http.post<DataPoint>(`${environment.API_ENDPOINT}DataPointConfigs`, datapoint);
  }

  fetchSimulationEngineState(): Observable<SimulationState> {
    return this.http.get<SimulationState>(environment.API_ENDPOINT + 'SimulationEngineState');
  }

  fetchHealthState(): Observable<String> {
    return this.http.get<String>(environment.HEALTH_ENDPOINT + 'live', { responseType: 'text' as 'json' });
  }

  fetchConnectionState(): Observable<String> {
    return this.http.get<String>(environment.HEALTH_ENDPOINT + 'ready', { responseType: 'text' as 'json' });
  }

  updateDataPointValue(dataPoint: DataPoint) {
    this.http.post<DataPoint>(`${environment.API_ENDPOINT}DataPointValues/${dataPoint.stationaryAddress}/${dataPoint.objectAddress}`, dataPoint)
      .pipe(
        catchError(error => {
          const errorMessage = error.error?.exceptionMessage || 'Failed to update data point';
          this.errorSubject.next(errorMessage);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: errorMessage,
          });
          this.fetchData();
          return of(null);
        })
      ).subscribe((data) => {
      if (data) {
        this.fetchData();
      }
    });
  }

}

export enum SimulationState {
  Running = 'Running',
  Stopped = 'Stopped'
}

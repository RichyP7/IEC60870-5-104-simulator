﻿import {Injectable} from '@angular/core';
import {BehaviorSubject, catchError, Observable, of} from 'rxjs';
import {HttpClient} from '@angular/common/http';
import {DataPoint} from '../list-view.component';
import {MessageService} from 'primeng/api';

@Injectable({
  providedIn: 'root',
})
export class DataService {
  private baseUrl = "http://localhost:8080/";
  private dataSubject = new BehaviorSubject<DataPoint[]>([]);
  data$ : Observable<DataPoint[]> = this.dataSubject.asObservable();

  private errorSubject = new BehaviorSubject<string | null>(null);
  public error$ = this.errorSubject.asObservable();

  constructor(private http: HttpClient, private messageService: MessageService) {}

  fetchData(): void {
    this.http.get<DataPoint[]>(this.baseUrl + 'DataPointConfigs').subscribe((data) => {
      this.dataSubject.next(data);
    });
  }

  toggleSimulationMode(dataPoint: DataPoint) {
    let simulationMode = dataPoint.mode
    this.http.put<DataPoint>(`${this.baseUrl}DataPointConfigs/${dataPoint.stationaryAddress}/${dataPoint.objectAddress}/simulation-mode`, JSON.stringify(simulationMode)
      ,
      {
        headers: { 'Content-Type': 'application/json' },
      })
      .subscribe((data) => {
        const currentData = this.dataSubject.getValue();
        const updatedDataList = currentData.map((dp) =>
          dp.objectAddress === data.objectAddress && dp.stationaryAddress === data.stationaryAddress ? data : dp
        );
        this.dataSubject.next(updatedDataList);
    });
  }

  updateSimulationEngineState(simulationState: SimulationState) {
    let command = (simulationState === SimulationState.Stopped) ? 'Stop' : 'Start';
    this.http.post<SimulationState>(`${this.baseUrl}SimulationEngineState?command=${command}`, null)
      .subscribe({
        next: () => {  },
        error: (err) => {
          console.error('Error while updating simulation state:', err);
        }
      });
  }

  createDataPoint(datapoint: DataPoint): Observable<DataPoint> {
    return this.http.post<DataPoint>(`${this.baseUrl}DataPointConfigs`, datapoint);
  }

  fetchSimulationEngineState(): Observable<SimulationState> {
    return this.http.get<SimulationState>(this.baseUrl + 'SimulationEngineState');
  }

  fetchHealthState(): Observable<String> {
    return this.http.get<String>(this.baseUrl + '/health/live', { responseType: 'text' as 'json' });
  }

  fetchConnectionState(): Observable<String> {
    return this.http.get<String>(this.baseUrl + 'health/ready', { responseType: 'text' as 'json' });
  }


  updateDataPointValue(dataPoint: DataPoint) {

    this.http.put<DataPoint>(`${this.baseUrl}DataPointValue/${dataPoint.stationaryAddress}/${dataPoint.objectAddress}`, JSON.stringify(dataPoint.value)
      ,
      {
        headers: { 'Content-Type': 'application/json' },
      })
      .pipe(
        catchError(error => {
          console.log(error.error.exceptionMessage)
          const errorMessage = 'Failed to update data point';
          this.errorSubject.next(errorMessage);
          this.messageService.add({
            severity: 'error',
            summary: 'Error - Bad Request',
            detail: error.error.exceptionMessage,
          });
          this.fetchData();
          return of(null);
        })
      ).subscribe((data) => {
      if (data) {
        const currentData = this.dataSubject.getValue();
        const updatedDataList = currentData.map((dp) =>
          dp.objectAddress === data.objectAddress && dp.stationaryAddress === data.stationaryAddress ? data : dp
        );
        this.dataSubject.next(updatedDataList);
      }
    });
  }

}

export enum SimulationState {
  Running = 'Running',
  Stopped = 'Stopped'
}

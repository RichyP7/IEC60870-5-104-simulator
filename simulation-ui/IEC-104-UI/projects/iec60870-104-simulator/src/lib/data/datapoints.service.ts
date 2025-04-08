import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { DataPoint, DataPointInterface, SimulationState } from './datapoints.interface';

@Injectable({
  providedIn: 'root'
})
export class DataPointsService implements DataPointInterface {
  private http = inject(HttpClient);
  private dataSubject = new BehaviorSubject<DataPoint[]>([]);
  apiEndpoint : String = "http://localhost:8080/api/";
  healthEndpoint : String = "http://localhost:8080/health/";

  private errorSubject = new BehaviorSubject<string | null>(null);
  public error$ = this.errorSubject.asObservable();

  fetchData(): Observable<DataPoint[]> {
    return this.http.get<DataPoint[]>(this.apiEndpoint + 'DataPointConfigs');
  }

  toggleSimulationMode(dataPoint: DataPoint) {
    let simulationMode = dataPoint.mode
    return this.http.put<DataPoint>(`${this.apiEndpoint}DataPointConfigs/${dataPoint.stationaryAddress}/${dataPoint.objectAddress}/simulation-mode`, JSON.stringify(simulationMode)
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
    this.http.post<SimulationState>(`${this.apiEndpoint}SimulationEngineState?command=${command}`, null)
      .subscribe({
        next: () => { },
        error: (err) => {
          console.error('Error while updating simulation state:', err);
        }
      });
  }

  createDataPoint(datapoint: DataPoint): Observable<DataPoint> {
    return this.http.post<DataPoint>(`${this.apiEndpoint}DataPointConfigs`, datapoint);
  }

  fetchSimulationEngineState(): Observable<SimulationState> {
    return this.http.get<SimulationState>(this.apiEndpoint + 'SimulationEngineState');
  }

  fetchHealthState(): Observable<String> {
    return this.http.get<String>(this.healthEndpoint + 'live', { responseType: 'text' as 'json' });
  }

   fetchConnectionState(): Observable<String> {
     return this.http.get<String>(this.healthEndpoint + 'ready', { responseType: 'text' as 'json' });
   }


  updateDataPointValue(dataPoint: DataPoint): Observable<DataPoint> {

    return this.http.put<DataPoint>(`${this.apiEndpoint}DataPointValue/${dataPoint.stationaryAddress}/${dataPoint.objectAddress}`, JSON.stringify(dataPoint.value));
  }
  //     ,
  //     {
  //       headers: { 'Content-Type': 'application/json' },
  //     })
  //     .pipe(
  //       catchError(error => {
  //         console.log(error.error.exceptionMessage)
  //         const errorMessage = 'Failed to update data point';
  //         this.errorSubject.next(errorMessage);
  //         console.log(errorMessage);
  //         this.fetchData();
  //         return of(null);
  //       })
  //     ).subscribe((data) => {
  //     if (data) {
  //       const currentData = this.dataSubject.getValue();
  //       const updatedDataList = currentData.map((dp) =>
  //         dp.objectAddress === data.objectAddress && dp.stationaryAddress === data.stationaryAddress ? data : dp
  //       );
  //       this.dataSubject.next(updatedDataList);
  //     }
  //   });
  // }

}


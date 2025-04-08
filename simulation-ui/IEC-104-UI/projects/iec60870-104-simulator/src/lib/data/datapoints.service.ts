import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { SelfDataPoint, DataPointInterface, SelfSimulationState } from './datapoints.interface';

@Injectable({
  providedIn: 'root'
})
export class DataPointsService implements DataPointInterface {
  private http = inject(HttpClient);
  private dataSubject = new BehaviorSubject<SelfDataPoint[]>([]);
  apiEndpoint : String = "http://localhost:8080/api/";
  healthEndpoint : String = "http://localhost:8080/health/";

  private errorSubject = new BehaviorSubject<string | null>(null);
  public error$ = this.errorSubject.asObservable();

  fetchData(): Observable<SelfDataPoint[]> {
    return this.http.get<SelfDataPoint[]>(this.apiEndpoint + 'DataPointConfigs');
  }

  toggleSimulationMode(dataPoint: SelfDataPoint) {
    let simulationMode = dataPoint.mode
    return this.http.put<SelfDataPoint>(`${this.apiEndpoint}DataPointConfigs/${dataPoint.stationaryAddress}/${dataPoint.objectAddress}/simulation-mode`, JSON.stringify(simulationMode)
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

  updateSimulationEngineState(simulationState: SelfSimulationState) {
    let command = (simulationState === SelfSimulationState.Stopped) ? 'Stop' : 'Start';
    this.http.post<SelfSimulationState>(`${this.apiEndpoint}SimulationEngineState?command=${command}`, null)
      .subscribe({
        next: () => { },
        error: (err) => {
          console.error('Error while updating simulation state:', err);
        }
      });
  }

  createDataPoint(datapoint: SelfDataPoint): Observable<SelfDataPoint> {
    return this.http.post<SelfDataPoint>(`${this.apiEndpoint}DataPointConfigs`, datapoint);
  }

  fetchSimulationEngineState(): Observable<SelfSimulationState> {
    return this.http.get<SelfSimulationState>(this.apiEndpoint + 'SimulationEngineState');
  }

  fetchHealthState(): Observable<String> {
    return this.http.get<String>(this.healthEndpoint + 'live', { responseType: 'text' as 'json' });
  }

   fetchConnectionState(): Observable<String> {
     return this.http.get<String>(this.healthEndpoint + 'ready', { responseType: 'text' as 'json' });
   }


  updateDataPointValue(dataPoint: SelfDataPoint): Observable<SelfDataPoint> {

    return this.http.put<SelfDataPoint>(`${this.apiEndpoint}DataPointValue/${dataPoint.stationaryAddress}/${dataPoint.objectAddress}`, JSON.stringify(dataPoint.value));
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


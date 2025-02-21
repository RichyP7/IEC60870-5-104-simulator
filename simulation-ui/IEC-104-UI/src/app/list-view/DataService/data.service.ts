import {Injectable} from '@angular/core';
import {BehaviorSubject, Observable} from 'rxjs';
import {HttpClient} from '@angular/common/http';
import {DataPoint} from '../list-view.component';
import {environment} from '../../../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class DataService {
  private dataSubject = new BehaviorSubject<DataPoint[]>([]);
  data$ : Observable<DataPoint[]> = this.dataSubject.asObservable();

  constructor(private http: HttpClient) {}

  fetchData(): void {
    this.http.get<DataPoint[]>(environment.API_ENDPOINT + 'DataPointConfigs').subscribe((data) => {
      this.dataSubject.next(data);
    });
  }

  toggleSimulationMode(dataPoint: DataPoint) {
    let simulationMode = dataPoint.mode
    this.http.put<DataPoint>(`${environment.API_ENDPOINT}DataPointConfigs/${dataPoint.stationaryAddress}/${dataPoint.objectAddress}/simulation-mode`, JSON.stringify(simulationMode)
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

  fetchSimulationEngineState(): SimulationState | null {
    let result: SimulationState | null = null;

    this.http.get<SimulationState>(environment.API_ENDPOINT + 'SimulationEngineState')
      .subscribe({
        next: (data) => result = data,
        error: (err) => {
          console.error('Error fetching SimulationState', err);
          result = null;
        }
      });

    return result;
  }




}

export enum SimulationState {
  Running = 'Running',
  Stopped = 'Stopped'
}

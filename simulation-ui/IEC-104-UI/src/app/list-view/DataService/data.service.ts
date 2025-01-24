import {Injectable} from '@angular/core';
import {BehaviorSubject, Observable} from 'rxjs';
import {HttpClient} from '@angular/common/http';
import {DataPoint, SimulationMode} from '../list-view.component';
import {environment} from '../../../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class DataService {
  private dataSubject = new BehaviorSubject<DataPoint[]>([]);
  data$ : Observable<DataPoint[]> = this.dataSubject.asObservable();

  constructor(private http: HttpClient) {}

  fetchData(): void {
    this.http.get<DataPoint[]>(environment.apiUrl + 'DataPointConfigs').subscribe((data) => {
      this.dataSubject.next(data);
    });
  }

  toggleSimulationMode(dataPoint: DataPoint) {
    let simulationMode = dataPoint.mode === SimulationMode.Cyclic ? SimulationMode.None : SimulationMode.Cyclic;
    this.http.put<DataPoint>(`${environment.apiUrl}DataPointConfigs/${dataPoint.stationaryAddress}/${dataPoint.objectAddress}/simulation-mode`, JSON.stringify(simulationMode)
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

}

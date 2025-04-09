import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { DataPoint, DataPointInterface } from './datapoints.interface';
import { DataPointConfigsService, DataPointValueService, Iec104DataPoint, Iec104DataPointDto, SimulationEngineStateService, SimulationState } from '../api/v1';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class DataPointsService implements DataPointInterface {

  public dpService = inject(DataPointConfigsService);
  protected dpValueService = inject(DataPointValueService);
  protected simEngineStateService = inject(SimulationEngineStateService);
  protected http = inject(HttpClient);

  private errorSubject = new BehaviorSubject<string | null>(null);
  public error$ = this.errorSubject.asObservable();

  fetchData(): Observable<Iec104DataPointDto[]> {
    console.log("DataPointServiceInject"+ this.dpService.configuration.basePath);
    return this.dpService.apiDataPointConfigsGet();
  }

  toggleSimulationMode(dataPoint: Iec104DataPointDto) {
    let simulationMode = dataPoint.mode
    return this.dpService.apiDataPointConfigsIdStationaryIdObjectSimulationModePut(dataPoint.stationaryAddress,dataPoint.objectAddress,JSON.stringify(simulationMode));
  }

  updateSimulationEngineState(simulationState: SimulationState) {
    const command = (simulationState === SimulationState.Stopped) ? 'Stop' : 'Start';
    this.simEngineStateService.commands(command)
      .subscribe({
        next: () => { },
        error: (err) => {
          console.error('Error while updating simulation state:', err);
        }
      });
  }

  createDataPoint(datapoint: Iec104DataPointDto): Observable<Iec104DataPoint> {
    return this.dpService.apiDataPointConfigsPost(datapoint);
  }

  fetchSimulationEngineState(): Observable<SimulationState> {
    return this.simEngineStateService.apiSimulationEngineStateGet();
  }

  fetchHealthState(): Observable<String> {
    return this.http.get<String>(this.dpService.configuration.basePath + 'live', { responseType: 'text' as 'json' });
  }

   fetchConnectionState(): Observable<String> {
     return this.http.get<String>(this.dpService.configuration.basePath  + 'ready', { responseType: 'text' as 'json' });
   }


  updateDataPointValue(dataPoint: Iec104DataPointDto): Observable<Iec104DataPointDto> {
    return this.dpValueService.apiDataPointValueIdStationaryIdObjectPut(dataPoint.stationaryAddress,dataPoint.objectAddress,JSON.stringify(dataPoint.value))
  }
}


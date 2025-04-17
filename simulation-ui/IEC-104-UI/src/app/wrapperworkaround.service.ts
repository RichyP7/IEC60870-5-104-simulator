import { inject, Injectable } from '@angular/core';
import { DataPointsService } from 'iec60870-104-simulator';
import { Observable } from 'rxjs';
import { DataPointConfigsService } from '../../projects/iec60870-104-simulator/src/lib/api/v1/api/dataPointConfigs.service';
import { DataPointValuesService, Iec104DataPoint, Iec104DataPointDto, SimulationEngineStateService, SimulationState } from '../../projects/iec60870-104-simulator/src/lib/api/v1';

@Injectable({
  providedIn: 'root'
})
//This is just a wrapper class for a workaround. The BASE_PATH for wasn't set otherwise
export class WrapperWorkAroundService extends DataPointsService {
private dpService1 = inject(DataPointConfigsService);
  protected dpValueServicewrapper = inject(DataPointValuesService);
  protected simEngineStateServicewrapper = inject(SimulationEngineStateService);


  override fetchData(): Observable<Iec104DataPointDto[]> {
    console.log("New DataPointServiceInject"+ this.dpService1.configuration.basePath);
    return this.dpService1.apiDataPointConfigsGet();
  }

  override toggleSimulationMode(dataPoint: Iec104DataPointDto) {
    let simulationMode = dataPoint.mode
    return this.dpService1.apiDataPointConfigsIdStationaryIdObjectSimulationModePut(dataPoint.stationaryAddress,dataPoint.objectAddress,JSON.stringify(simulationMode));
  }

  override updateSimulationEngineState(simulationState: SimulationState) {
    const command = (simulationState === SimulationState.Stopped) ? 'Stop' : 'Start';
    this.simEngineStateServicewrapper.commands(command)
      .subscribe({
        next: () => { },
        error: (err) => {
          console.error('Error while updating simulation state:', err);
        }
      });
  }

  override createDataPoint(datapoint: Iec104DataPointDto): Observable<Iec104DataPoint> {
    return this.dpService1.apiDataPointConfigsPost(datapoint);
  }

  override fetchSimulationEngineState(): Observable<SimulationState> {
    return this.simEngineStateServicewrapper.apiSimulationEngineStateGet();
  }

  override fetchHealthState(): Observable<String> {
    return this.http.get<String>(this.dpService1.configuration.basePath + '/health/live', { responseType: 'text' as 'json' });
  }

   override fetchConnectionState(): Observable<String> {
     return this.http.get<String>(this.dpService1.configuration.basePath  + '/health/ready', { responseType: 'text' as 'json' });
   }


  override updateDataPointValue(dataPoint: Iec104DataPointDto): Observable<Iec104DataPointDto> {
    return this.dpValueServicewrapper.apiDataPointValuesIdStationaryIdObjectPost(dataPoint.stationaryAddress,dataPoint.objectAddress,dataPoint)
  }


}


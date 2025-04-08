import { InjectionToken } from "@angular/core";
import { Observable } from "rxjs";
//export const DP_INTERFACE_TOKEN = new InjectionToken<DataPointInterface>('DataPoint_Interface');

export interface DataPointInterface {
    fetchData(): Observable<SelfDataPoint[]>;
    toggleSimulationMode(dataPoint: SelfDataPoint):void;
    createDataPoint(datapoint: SelfDataPoint): Observable<SelfDataPoint>;
    updateSimulationEngineState(simulationState: SelfSimulationState):void;
    fetchSimulationEngineState(): Observable<SelfSimulationState>;
    fetchHealthState(): Observable<String>;
    fetchConnectionState(): Observable<String>;
    updateDataPointValue(dataPoint: SelfDataPoint):Observable<SelfDataPoint>
  }
  export interface SelfDataPoint {
    id: string;
    stationaryAddress: number;
    objectAddress: number;
    iec104DataType: string;
    value: string;
    mode: SelfSimulationMode;
  }
  export enum SelfSimulationMode {
    None = 'None',
    Cyclic = 'Cyclic',
    CyclicStatic = 'CyclicStatic',
    Response = 'Response'
  }
  export enum SelfSimulationState {
    Running = 'Running',
    Stopped = 'Stopped'
  }
  
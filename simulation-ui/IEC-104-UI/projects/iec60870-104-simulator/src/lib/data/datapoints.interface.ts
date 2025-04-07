import { InjectionToken } from "@angular/core";
import { Observable } from "rxjs";
//export const DP_INTERFACE_TOKEN = new InjectionToken<DataPointInterface>('DataPoint_Interface');

export interface DataPointInterface {
    fetchData(): Observable<DataPoint[]>;
    toggleSimulationMode(dataPoint: DataPoint):void;
    createDataPoint(datapoint: DataPoint): Observable<DataPoint>;
    updateSimulationEngineState(simulationState: SimulationState):void;
    fetchSimulationEngineState(): Observable<SimulationState>;
    fetchHealthState(): Observable<String>;
    fetchConnectionState(): Observable<String>;
    updateDataPointValue(dataPoint: DataPoint):Observable<DataPoint>
  }
  export interface DataPoint {
    id: string;
    stationaryAddress: number;
    objectAddress: number;
    iec104DataType: string;
    value: string;
    mode: SimulationMode;
  }
  export enum SimulationMode {
    None = 'None',
    Cyclic = 'Cyclic',
    CyclicStatic = 'CyclicStatic',
    Response = 'Response'
  }
  export enum SimulationState {
    Running = 'Running',
    Stopped = 'Stopped'
  }
  
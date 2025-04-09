import { Observable } from "rxjs";
import { Iec104DataPoint, Iec104DataPointDto, SimulationState } from "../api/v1";

export interface DataPointInterface {
  fetchData(): Observable<Iec104DataPointDto[]>;
  toggleSimulationMode(dataPoint: Iec104DataPointDto): void;
  createDataPoint(datapoint: Iec104DataPointDto): Observable<Iec104DataPoint>;
  updateSimulationEngineState(simulationState: SimulationState): void;
  fetchSimulationEngineState(): Observable<SimulationState>;
  fetchHealthState(): Observable<String>;
  fetchConnectionState(): Observable<String>;
  updateDataPointValue(dataPoint: Iec104DataPointDto): Observable<Iec104DataPointDto>
}
export interface DataPoint {
  id: string;
  stationaryAddress: number;
  objectAddress: number;
  iec104DataType: string;
  value: string;
  mode: SimulationMode;
}
export class DataPointVis {
  constructor(
    public id: string,
    public stationaryAddress: number,
    public objectAddress: number,
    public iec104DataType: string,
    public value: string,
    public mode: SimulationMode,
  ) { }
}

export enum SimulationMode {
  None = 'None',
  Cyclic = 'Cyclic',
  CyclicStatic = 'CyclicStatic',
  Response = 'Response'
}
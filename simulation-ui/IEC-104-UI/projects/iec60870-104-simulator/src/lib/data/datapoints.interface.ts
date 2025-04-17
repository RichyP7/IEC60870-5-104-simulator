import { Observable } from "rxjs";
import { Iec104DataPoint, Iec104DataPointDto, SimulationMode, SimulationState } from "../api/v1";

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
export class DataPointVis {
  constructor(
    public id: string,
    public stationaryAddress: number,
    public objectAddress: number,
    public iec104DataType: string,
    public value: DataPointValueVis,
    public mode: SimulationMode,
  ) { }
}
export class DataPointValueVis {
  constructor(
    public numericValue?: number | null,
    public binaryValue?: boolean | null
  ) { }
}
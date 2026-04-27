import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';

export interface ScenarioInfo {
  name: string;
  status: string;  // 'Idle' | 'Running' | 'Completed' | 'Failed'
  remainingMs: number;
}

export interface ScenarioStep {
  delayMs: number;
  ca: number;
  oa: number;
  valueStr: string;
  freeze: boolean;
  description: string;
}

export interface ScenarioDefinition {
  name: string;
  recoveryMs: number;
  steps: ScenarioStep[];
  recoverySteps: ScenarioStep[];
}

@Injectable({ providedIn: 'root' })
export class ScenarioService {
  constructor(private http: HttpClient) {}

  getScenarios(): Observable<ScenarioInfo[]> {
    return this.http.get<ScenarioInfo[]>(`${environment.API_ENDPOINT}Scenarios`);
  }

  getScenarioDefinitions(): Observable<ScenarioDefinition[]> {
    return this.http.get<ScenarioDefinition[]>(`${environment.API_ENDPOINT}Scenarios/definitions`);
  }

  triggerScenario(name: string): Observable<unknown> {
    return this.http.post(`${environment.API_ENDPOINT}Scenarios/${name}/trigger`, null);
  }
}

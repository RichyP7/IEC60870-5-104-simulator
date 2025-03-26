import { Routes } from '@angular/router';
import {DatapointDetailsComponent} from './datapoint-details/datapoint-details.component';
import {Iec60870104SimulatorComponent} from './iec60870-104-simulator.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', component: Iec60870104SimulatorComponent},
  { path: 'datapoint/:stationaryId/:objectId', component: DatapointDetailsComponent },
];

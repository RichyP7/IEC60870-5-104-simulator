import { Routes } from '@angular/router';
import { Iec60870104SimulatorComponent } from 'iec60870-104-simulator';
import { DatapointDetailsComponent } from '../../projects/iec60870-104-simulator/src/lib/datapoint-details/datapoint-details.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', component: Iec60870104SimulatorComponent},
  { path: 'datapoint/:stationaryId/:objectId', component: DatapointDetailsComponent },
];

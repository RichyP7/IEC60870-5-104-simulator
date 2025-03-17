import { Routes } from '@angular/router';
import {DatapointDetailsComponent} from '../../projects/iec60870-104-simulator/src/lib/datapoint-details/datapoint-details.component';
import {AppComponent} from './app.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', component: AppComponent},
  { path: 'datapoint/:stationaryId/:objectId', component: DatapointDetailsComponent },
];

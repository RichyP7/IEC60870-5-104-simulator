import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from '../projects/iec60870-104-simulator/src/lib/iec60870-104-simulator.config';
import { AppComponent } from './app/app.component';

bootstrapApplication(AppComponent, appConfig)
  .catch((err) => console.error(err));

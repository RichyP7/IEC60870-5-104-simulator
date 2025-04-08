import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideClientHydration } from '@angular/platform-browser';
import {provideAnimations} from '@angular/platform-browser/animations';
import {provideHttpClient, withFetch} from '@angular/common/http';
import {MessageService} from 'primeng/api';
import { routes } from './app.routes';
import { DataPointsService } from 'iec60870-104-simulator';
import { NewserviceService } from './newservice.service';
import { BASE_PATH } from '../../projects/iec60870-104-simulator/src/lib/openapi';

export const appConfig: ApplicationConfig = {
  providers: [
    MessageService,
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideClientHydration(),
    provideAnimations(),
    provideHttpClient(withFetch()), 
    { provide: DataPointsService, useClass: NewserviceService },
    { provide: BASE_PATH, useValue: environment.API_BASE_PATH }],
  //   providePrimeNG({
  //     theme: {
  //       preset: Lara,
  //       options: {
  //         darkModeSelector: 'system',
  //         cssLayer: {
  //           name: 'primeng',
  //           order: 'tailwind-base, primeng, tailwind-utilities'
  //         }
  //       }
  //     }

   ]
};

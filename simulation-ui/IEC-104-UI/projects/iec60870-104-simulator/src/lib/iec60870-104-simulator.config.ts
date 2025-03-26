import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './iec60870-104--simulator.routes';
import { provideClientHydration } from '@angular/platform-browser';
import {provideAnimations} from '@angular/platform-browser/animations';
import {provideHttpClient, withFetch} from '@angular/common/http';
import {MessageService} from 'primeng/api';

export const appConfig: ApplicationConfig = {
  providers: [
    MessageService,
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideClientHydration(),
    provideAnimations(),
    provideHttpClient(withFetch())
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

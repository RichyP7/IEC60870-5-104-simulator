import { ApplicationConfig, EnvironmentProviders, importProvidersFrom, makeEnvironmentProviders, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideClientHydration } from '@angular/platform-browser';
import {provideAnimations} from '@angular/platform-browser/animations';
import {HttpClientModule, provideHttpClient, withFetch} from '@angular/common/http';
import {MessageService} from 'primeng/api';
import { routes } from './app.routes';
import { DataPointsService } from 'iec60870-104-simulator';
import { WrapperWorkAroundService } from './wrapperworkaround.service';
import { environment } from '../environments/environment';
import { ApiModule, BASE_PATH} from '../../projects/iec60870-104-simulator/src/lib/api/v1';

export const appConfig: ApplicationConfig = {
  providers: [
    MessageService,
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideClientHydration(),
    provideAnimations(),
    provideHttpClient(withFetch()), 
    { provide: BASE_PATH, useFactory:apiConfigFactory },
    { provide: DataPointsService, useClass:WrapperWorkAroundService }]
};
export function apiConfigFactory(): string {
  // const params: ConfigurationParameters = {
  //   basePath: 'http://localhost:8090',
  // };
  // return new Configuration(params);
  return  environment.API_BASE_PATH;
}

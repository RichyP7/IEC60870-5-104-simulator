import { Component } from '@angular/core';
import {FormsModule} from '@angular/forms';

import { DataPointsService, Iec60870104SimulatorComponent } from 'iec60870-104-simulator';
import { NewserviceService } from './newservice.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [ FormsModule, Iec60870104SimulatorComponent],
  providers:[{ provide: DataPointsService, useClass: NewserviceService }],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'IEC-104-UI';
}
import { Component } from '@angular/core';
import {DataPoint, ListViewComponent} from '../../projects/iec60870-104-simulator/src/lib/list-view/list-view.component';
import {FormsModule} from '@angular/forms';

import { Iec60870104SimulatorComponent } from "../../projects/iec60870-104-simulator/src/lib/iec60870-104-simulator.component";

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [ FormsModule,  Iec60870104SimulatorComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'IEC-104-UI';
}

import { Component } from '@angular/core';
import { ListViewComponent } from './list-view/list-view.component';
import { FormsModule } from '@angular/forms';
import { DatapointDetailsComponent } from './datapoint-details/datapoint-details.component';
import { HeaderComponent } from './header/header.component';
import { SelfDataPoint } from './data/datapoints.interface';
import { DataPointsService } from '../public-api';
import { provideHttpClient, withFetch } from '@angular/common/http';


@Component({
  selector: 'lib-iec60870-104-simulator',
  standalone: true,
  imports: [
    ListViewComponent,
    DatapointDetailsComponent,
    FormsModule,
    HeaderComponent,
  ],
  templateUrl: './iec60870-104-simulator.component.html',
  styles: ``
})
export class Iec60870104SimulatorComponent {

  selectedItem: SelfDataPoint | null = null;
  title = 'IEC-104-UI';

  onItemSelected(item: SelfDataPoint) {
    this.selectedItem = item
  }
}


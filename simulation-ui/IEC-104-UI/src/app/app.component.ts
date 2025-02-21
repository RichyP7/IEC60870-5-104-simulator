import { Component } from '@angular/core';
import {DataPoint, ListViewComponent} from './list-view/list-view.component';
import {DatapointDetailsComponent} from './datapoint-details/datapoint-details.component';
import {FormsModule} from '@angular/forms';
import {HeaderComponent} from './header/header.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [ListViewComponent, DatapointDetailsComponent, FormsModule, HeaderComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  selectedItem: DataPoint | null = null;
  title = 'IEC-104-UI';

  onItemSelected(item: DataPoint) {
    this.selectedItem = item
  }
}

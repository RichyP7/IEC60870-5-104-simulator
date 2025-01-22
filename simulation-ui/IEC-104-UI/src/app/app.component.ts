import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import {Button} from "primeng/button";
import {DataPoint, ListViewComponent} from './list-view/list-view.component';
import {DatapointDetailsComponent} from './datapoint-details/datapoint-details.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, Button, ListViewComponent, DatapointDetailsComponent],
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

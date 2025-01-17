import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import {Button} from "primeng/button";
import {ListViewComponent} from './list-view/list-view.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, Button, ListViewComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'IEC-104-UI';
}

import { Component } from '@angular/core';
import {ListViewComponent} from './list-view/list-view.component';
import {FormsModule} from '@angular/forms';
import {HeaderComponent} from './header/header.component';
import {ScenarioPanelComponent} from './scenario-panel/scenario-panel.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [ListViewComponent, FormsModule, HeaderComponent,
            ScenarioPanelComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'IEC-104-UI';
}

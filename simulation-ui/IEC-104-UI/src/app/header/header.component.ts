import {Component, OnInit} from '@angular/core';
import {Menubar} from "primeng/menubar";
import {PrimeTemplate} from "primeng/api";
import {ToggleButton} from "primeng/togglebutton";
import {FormsModule} from '@angular/forms';
import {Router} from '@angular/router';
import {DataService, SimulationState} from '../list-view/DataService/data.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    Menubar,
    PrimeTemplate,
    ToggleButton,
    FormsModule
  ],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent implements OnInit{
  isSimulating: boolean = false;

  constructor(
    private router: Router,
    private dataService: DataService
  ) {}

  ngOnInit(): void {
    this.fetchCurrentSimulationEngineState();
  }

  fetchCurrentSimulationEngineState() {
    let simulationState: SimulationState | null;
    simulationState = this.dataService.fetchSimulationEngineState();
    this.isSimulating = simulationState != null && simulationState === SimulationState.Running;
  }


  updateSimulationState(state: boolean) {
    const wantedSimulationState: SimulationState = state ? SimulationState.Running : SimulationState.Stopped;
    this.dataService.updateSimulationEngineState(wantedSimulationState);

  }
}

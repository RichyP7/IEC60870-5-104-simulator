import {Component, Input, OnChanges, SimpleChanges} from '@angular/core';
import {DataPoint, SimulationMode} from '../list-view/list-view.component';
import {NgIf} from '@angular/common';
import {environment} from '../../environments/environment.development';
import {tap} from 'rxjs';
import {HttpClient} from '@angular/common/http';
import {DataService} from '../list-view/DataService/data.service';
import {Button} from 'primeng/button';
import {Card} from 'primeng/card';
import {TableModule} from 'primeng/table';
import {DropdownModule} from 'primeng/dropdown';
import {FormsModule} from '@angular/forms';

@Component({
  selector: 'app-datapoint-details',
  standalone: true,
  imports: [
    NgIf,
    Button,
    Card,
    TableModule,
    DropdownModule,
    FormsModule
  ],
  templateUrl: './datapoint-details.component.html',
  styleUrl: './datapoint-details.component.scss'
})
export class DatapointDetailsComponent implements OnChanges{
  @Input()
  item: DataPoint | null = null;

  simulationModes = [
    { label: 'Cyclic Random', value: SimulationMode.Cyclic, icon: 'pi pi-sort-alt' },
    { label: 'Cyclic Static', value: SimulationMode.CyclicStatic, icon: 'pi pi-lock' },
    { label: 'None', value: SimulationMode.None, icon: 'pi pi-play-circle' }
  ];

  constructor(
    private http: HttpClient,
    private dataService: DataService
  ) {}

  toggleDoublePointValue(doublePoint: DataPoint) {
    console.log("action");
    this.http
      .get<DataPoint[]>(environment.API_ENDPOINT + 'ValueConfig/' + doublePoint.stationaryAddress + "/" + doublePoint.objectAddress)
      .pipe(
        tap(() => {
          this.dataService.fetchData();
        })
      )
      .subscribe();
  }

  toggleSimulationMode(point: DataPoint) {
    this.dataService.toggleSimulationMode(point);
  }

  private syncWithUpdatedData(): void {
    if (this.item) {
      this.dataService.data$.subscribe((data) => {
        this.item = data.find((dp) => dp.id === this.item?.id) || null;
      });
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['item'] && changes['item'].currentValue) {
      this.syncWithUpdatedData();
    }
  }
}

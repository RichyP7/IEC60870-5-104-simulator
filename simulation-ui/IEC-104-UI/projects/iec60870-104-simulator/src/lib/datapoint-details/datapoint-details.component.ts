import {Component, inject, Inject, Input, OnChanges, SimpleChanges} from '@angular/core';
import {NgIf} from '@angular/common';
import {tap} from 'rxjs';
import {HttpClient} from '@angular/common/http';
import {Button} from 'primeng/button';
import {Card, CardModule} from 'primeng/card';
import {TableModule} from 'primeng/table';
import {DropdownModule} from 'primeng/dropdown';
import {FormsModule} from '@angular/forms';
import {Toast, ToastModule} from 'primeng/toast';
import { DataPoint,   SimulationMode } from '../data/datapoints.interface';
import { DataPointsService } from '../data/datapoints.service';

@Component({
  selector: 'app-datapoint-details',
  standalone: true,
  imports: [
    NgIf,
    Button,
    CardModule,
    TableModule,
    DropdownModule,
    FormsModule,
    ToastModule
  ],
  templateUrl: './datapoint-details.component.html',
  styleUrl: './datapoint-details.component.scss'
})
export class DatapointDetailsComponent implements OnChanges{
  @Input()
  item: DataPoint | null = null;
  private dataService = inject(DataPointsService);
  isEditing = false;

  simulationModes = [
    { label: 'Cyclic Random', value: SimulationMode.Cyclic, icon: 'pi pi-sort-alt' },
    { label: 'Cyclic Static', value: SimulationMode.CyclicStatic, icon: 'pi pi-lock' },
    { label: 'None', value: SimulationMode.None, icon: 'pi pi-play-circle' }
  ];

  constructor(
    private http: HttpClient,
  ) {}

  toggleDoublePointValue(doublePoint: DataPoint) {
    console.log("action");
    this.http
      .get<DataPoint[]>("http://localhost:8080/health/" + 'ValueConfig/' + doublePoint.stationaryAddress + "/" + doublePoint.objectAddress)
      .pipe(
        tap(() => {
          //this.dataService.fetchData();
        })
      )
      .subscribe();
  }

  toggleSimulationMode(point: DataPoint) {
    this.dataService.toggleSimulationMode(point);
  }

  private syncWithUpdatedData(): void {
    if (this.item) {
      // this.dataService.data$.subscribe((data) => {
      //   this.item = data.find((dp) => dp.id === this.item?.id) || null;
     // });
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['item'] && changes['item'].currentValue) {
      this.syncWithUpdatedData();
    }
  }

  dataPointCanBeToggled(item: DataPoint) : boolean {
    return (item.iec104DataType === 'M_DP_NA_1' || item.iec104DataType === 'M_SP_NA_1')
  }

  editItem(item: DataPoint) {
    this.isEditing = true;
  }

  updateValue(item: DataPoint) {
    this.isEditing = false;
    this.dataService.updateDataPointValue(item);
  }
}

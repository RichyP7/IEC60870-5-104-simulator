import {Component, inject, Inject, Input, OnChanges, SimpleChanges} from '@angular/core';
import {NgIf} from '@angular/common';
import {Button} from 'primeng/button';
import {Card, CardModule} from 'primeng/card';
import {TableModule} from 'primeng/table';
import {DropdownModule} from 'primeng/dropdown';
import {FormsModule} from '@angular/forms';
import {Toast, ToastModule} from 'primeng/toast';
import { DataPointVis } from '../data/datapoints.interface';
import { DataPointsService } from '../data/datapoints.service';
import { Iec104DataPointDto, Iec104DataTypes, IecValueDto, IntValueDto, SimulationMode, SinglePointValueDto } from '../api/v1';
import { catchError, of } from 'rxjs';
import { MessageService } from 'primeng/api';
import { IecValueInputComponent } from '../shared/iec-value-input/iec-value-input.component';

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
    ToastModule,
    IecValueInputComponent
  ],
  templateUrl: './datapoint-details.component.html',
  styleUrl: './datapoint-details.component.scss'
})
export class DatapointDetailsComponent implements OnChanges{
  @Input()
  item: DataPointVis | null = null;
  private dpservice = inject(DataPointsService);
  private messageService = inject(MessageService)
  isEditing = false;

  simulationModes = [
    { label: 'Cyclic Random', value: SimulationMode.Cyclic, icon: 'pi pi-sort-alt' },
    { label: 'Cyclic Static', value: SimulationMode.CyclicStatic, icon: 'pi pi-lock' },
    { label: 'None', value: SimulationMode.None, icon: 'pi pi-play-circle' }
  ];


  private GetDto(point: DataPointVis): Iec104DataPointDto {
    const dtoint : IntValueDto = {value: point.value.numericValue || undefined};
    const dtosp :  SinglePointValueDto= {value: point.value.binaryValue || undefined};
    const test :IecValueDto ={ numericValue : dtoint, singlePointValue : dtosp }
    return {
      objectAddress: point.objectAddress,
      stationaryAddress: point.stationaryAddress,
      iec104DataType: point.iec104DataType as Iec104DataTypes,
      mode :point.mode,
      value :test,
      id: point.id
    };
  }

  private syncWithUpdatedData(): void {
    if (this.item) {
      this.dpservice.fetchData
      // this.dataService.data$.subscribe((data) => {
      //   this.item = data.find((dp) => dp.id === this.item?.id) || null;
     // });
    }
  }
  toggleSimulationMode(point: DataPointVis) {
    this.dpservice.toggleSimulationMode(this.GetDto(point));
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['item'] && changes['item'].currentValue) {
      this.syncWithUpdatedData();
    }
  }

  editItem(item: DataPointVis) {
    this.isEditing = true;
  }

  updateValue(item: DataPointVis) {
    this.isEditing = false;
    this.dpservice.updateDataPointValue(this.GetDto(item))
    .pipe(
      catchError(error => {
        console.log(error.error.exceptionMessage)
        this.messageService.add({
         severity: 'error',
         summary: 'Error',
         detail: error.error.exceptionMessage,
       });
        return of(null);
      })
    )
    .subscribe();
  }
}

import {Component, inject, Inject, Input, OnChanges, SimpleChanges} from '@angular/core';
import {NgIf} from '@angular/common';
import {Button} from 'primeng/button';
import {Card, CardModule} from 'primeng/card';
import {TableModule} from 'primeng/table';
import {DropdownModule} from 'primeng/dropdown';
import {FormsModule} from '@angular/forms';
import {Toast, ToastModule} from 'primeng/toast';
import { DataPointValueVis, DataPointVis, DoublePointValueVis } from '../data/datapoints.interface';
import { DataPointsService } from '../data/datapoints.service';
import { Iec104DataPointDto, Iec104DataTypes, IecDoublePointValueEnumDto, IecValueDto, IntValueDto, SimulationMode, SinglePointValueDto } from '../api/v1';
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
    console.log("syncWithUpdatedData");
    if (this.item) {
      let update :Iec104DataPointDto ={
        stationaryAddress :this.item.stationaryAddress,objectAddress: this.item.objectAddress,
        iec104DataType: this.item.iec104DataType as Iec104DataTypes
      };
      this.dpservice.fetchSingleIoPointValue(update).subscribe((data) => {
          this.item = mapDtoToInternal(data);
        });
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
function mapDtoToInternal(itemDto: Iec104DataPointDto): DataPointVis {
  return new DataPointVis(itemDto.id ? itemDto.id:"unknown",
    itemDto.stationaryAddress,
    itemDto.objectAddress,
    itemDto.iec104DataType.toString(), 
    new DataPointValueVis(itemDto.value?.numericValue? itemDto.value.numericValue.value: -1,
      itemDto.value?.singlePointValue?.value ,
      itemDto.value?.doublePointValue?.value ? (itemDto.value.doublePointValue.value as DoublePointValueVis): DoublePointValueVis.INDETERMINATE,
      itemDto.value?.floatValue?.value ? itemDto.value.floatValue.value:-1),
    SimulationMode.Cyclic);
}
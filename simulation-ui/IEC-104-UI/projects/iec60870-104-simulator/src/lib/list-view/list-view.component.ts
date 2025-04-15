import { Component, OnInit, Output, EventEmitter, inject } from '@angular/core';

import { NgClass, NgForOf } from '@angular/common';
import { Button } from 'primeng/button';
import { CreateDialogComponent } from './create-dialog/create-dialog.component';
import { Toast, ToastModule } from 'primeng/toast';
import { catchError, of } from 'rxjs';
import { AccordionModule } from 'primeng/accordion';
import {  ApiModule, DataPointConfigsService, Iec104DataPointDto, Iec104DataTypes, SimulationMode } from '../api/v1';
import { DataPoint, DataPointVis } from '../data/datapoints.interface';
import { DataPointsService } from '../data/datapoints.service';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-list-view',
  standalone: true,
  imports: [
    NgForOf,
    Button,
    NgClass,
    CreateDialogComponent,
    ToastModule,
    AccordionModule,
    ApiModule
  ],
  templateUrl: './list-view.component.html',
  styleUrl: './list-view.component.scss'
})

export class ListViewComponent implements OnInit {

  @Output()
  itemSelected = new EventEmitter<DataPointVis>();
  selectedItem: DataPointVis | null = null; // Track the selected item

  groupedData: GroupedData[] = [];
  showDialog: boolean = false;
  private dpservice = inject(DataPointsService);
  private messageService = inject(MessageService);
  ngOnInit() {
    // Fetch initial data
    //console.log('initial'+ this.openAPIService.configuration.basePath+"base");
     this.dpservice.fetchData().subscribe((data) => {
       this.groupedData = this.groupDataByStationaryAddress(data);
     });;
  };
  groupDataByStationaryAddress(data: Iec104DataPointDto[]): GroupedData[] {
    const internalItems: DataPointVis[] = data.map(mapDtoToInternal);
    const grouped = internalItems.reduce<Record<number, GroupedData>>((acc, item) => {
      const key = item.stationaryAddress;
      acc[key] = acc[key] || { stationaryAddress: key, items: [] };
      acc[key].items.push(item);
      return acc;
    }, {});
    return Object.values(grouped);
  }


  clickOnDataPoint(item: DataPointVis) {
    console.log(item);
    if(!item.id || !item.mode)
      return;
    let localDataPoint = 
    //this.router.navigate([`/datapoint/${item.stationaryAddress}/${item.objectAddress}`]);
    this.selectedItem = {
      id:item.id, 
      stationaryAddress : item.stationaryAddress ,
      objectAddress : item.objectAddress,
      iec104DataType : item.iec104DataType.toString(),
      value : item.value ? item.value : "0",
      mode : item.mode ? item.mode as SimulationMode : SimulationMode.None
     }
    this.itemSelected.emit(this.selectedItem)
  }

  reloadData() {
    this.ngOnInit();
  }

  processDialogChange($event: boolean) {
    console.log($event);
    this.showDialog = false;
  }

  createDataPoint(datapoint: DataPointVis) {
    const dto : Iec104DataPointDto= {
        id : datapoint.id,
       stationaryAddress : datapoint.stationaryAddress,
       objectAddress: datapoint.objectAddress,
       iec104DataType : Iec104DataTypes.MSpNa1, 
       value : datapoint.value,
       mode : datapoint.mode
      };
    return this.dpservice.createDataPoint(dto)
      .pipe(
        catchError(error => {
          console.log(error.error.exceptionMessage)
         this.messageService.add({
           severity: 'error',
           summary: 'Error - Bad Request',
           detail: error.error.exceptionMessage,
         });
          return of(null);
        })
      )
      .subscribe(
        response => {
          this.reloadData();
        }
      );
  }
}

function mapDtoToInternal(itemDto: Iec104DataPointDto): DataPointVis {
  return new DataPointVis(itemDto.id ? itemDto.id:"unknown",itemDto.stationaryAddress,
    itemDto.objectAddress,itemDto.iec104DataType,
    itemDto.value? itemDto.value: "empty", SimulationMode.Cyclic);
}
export interface GroupedData {
  stationaryAddress: number;
  items: DataPointVis[];
}

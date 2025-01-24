import {Component, OnInit, Output, EventEmitter} from '@angular/core';
import {Accordion, AccordionContent, AccordionHeader, AccordionPanel, AccordionTab} from 'primeng/accordion';
import {NgForOf} from '@angular/common';
import {PrimeTemplate} from 'primeng/api';
import {HttpClient} from '@angular/common/http';
import {Button} from 'primeng/button';
import {Router} from '@angular/router';
import {DataService} from './DataService/data.service';

@Component({
  selector: 'app-list-view',
  standalone: true,
  imports: [
    Accordion,
    AccordionTab,
    AccordionPanel,
    NgForOf,
    PrimeTemplate,
    AccordionHeader,
    AccordionContent,
    Button,
  ],
  templateUrl: './list-view.component.html',
  styleUrl: './list-view.component.scss'
})
export class ListViewComponent implements OnInit {

  @Output()
  itemSelected = new EventEmitter<DataPoint>();

  groupedData: GroupedData[] = [];

  constructor(
    private http: HttpClient,
    private router: Router,
    private dataService: DataService
  ) {}

  ngOnInit() {
    this.dataService.data$.subscribe((data) => {
      this.groupedData = this.groupDataByStationaryAddress(data);
    });

    // Fetch initial data
    this.dataService.fetchData();
  }

  groupDataByStationaryAddress(data: DataPoint[]): GroupedData[] {
    const grouped = data.reduce<Record<number, GroupedData>>((acc, item) => {
      const key = item.stationaryAddress;
      acc[key] = acc[key] || { stationaryAddress: key, items: [] };
      acc[key].items.push(item);
      return acc;
    }, {});
    return Object.values(grouped);
  }

  clickOnDataPoint(item: DataPoint) {
    console.log(item);
    this.router.navigate([`/datapoint/${item.stationaryAddress}/${item.objectAddress}`]);
    this.itemSelected.emit(item)
  }

  reloadData() {
    this.ngOnInit();
  }
}

export interface DataPoint {
  id: string;
  stationaryAddress: number;
  objectAddress: number;
  iec104DataType: string;
  value: string;
  mode: SimulationMode;
}

export interface GroupedData {
  stationaryAddress: number;
  items: DataPoint[];
}

export enum SimulationMode {
  None = 'None',
  Cyclic = 'Cyclic',
  Response = 'Response'
}


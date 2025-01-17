import {Component, OnInit} from '@angular/core';
import {Accordion, AccordionContent, AccordionHeader, AccordionPanel, AccordionTab} from 'primeng/accordion';
import {NgForOf} from '@angular/common';
import {PrimeTemplate} from 'primeng/api';
import {HttpClient} from '@angular/common/http';
import {tap} from 'rxjs';
import {Button} from 'primeng/button';

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

  groupedData: GroupedData[] = [];

  constructor(private http: HttpClient) {}

  ngOnInit() {
    //this.groupedData = this.groupDataByStationaryAddress(this.data);
    this.http
      .get<DataPoint[]>('http://localhost:8080/api/DataPointConfigs')
      .pipe(
        tap((data) => {
          this.groupedData = this.groupDataByStationaryAddress(data);
        })
      )
      .subscribe();
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

  logge(tab: GroupedData) {
    console.log(tab);

  }

  reloadData() {
    this.ngOnInit();
  }
}

interface DataPoint {
  id: string;
  stationaryAddress: number;
  objectAddress: number;
  iec104DataType: number;
  value: string;
  mode: number;
}

interface GroupedData {
  stationaryAddress: number;
  items: DataPoint[];
}

import {Component, Input, OnChanges, SimpleChanges} from '@angular/core';
import {DataPoint} from '../list-view/list-view.component';
import {NgIf} from '@angular/common';
import {environment} from '../../environments/environment.development';
import {tap} from 'rxjs';
import {HttpClient} from '@angular/common/http';
import {subscribe} from 'node:diagnostics_channel';
import {DataService} from '../list-view/DataService/data.service';
import {Button} from 'primeng/button';

@Component({
  selector: 'app-datapoint-details',
  standalone: true,
  imports: [
    NgIf,
    Button
  ],
  templateUrl: './datapoint-details.component.html',
  styleUrl: './datapoint-details.component.scss'
})
export class DatapointDetailsComponent implements OnChanges{
  @Input()
  item: DataPoint | null = null;

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

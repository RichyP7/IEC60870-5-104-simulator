import {Component, Input, OnChanges, OnInit, SimpleChanges} from '@angular/core';
import {
  DataPoint,
  DoublePointValue,
  getDisplayValue,
  getValueType,
  IecValue,
  SimulationMode
} from '../list-view/list-view.component';
import {NgForOf, NgIf, NgSwitch, NgSwitchCase, NgSwitchDefault} from '@angular/common';
import {environment} from '../../environments/environment.development';
import {tap} from 'rxjs';
import {HttpClient} from '@angular/common/http';
import {DataService} from '../list-view/DataService/data.service';
import {Button} from 'primeng/button';
import {Card} from 'primeng/card';
import {TableModule} from 'primeng/table';
import {DropdownModule} from 'primeng/dropdown';
import {FormsModule} from '@angular/forms';
import {Toast} from 'primeng/toast';
import {Select} from 'primeng/select';
import {InputNumber} from 'primeng/inputnumber';
import {ToggleSwitch} from 'primeng/toggleswitch';

@Component({
  selector: 'app-datapoint-details',
  standalone: true,
  imports: [
    NgIf,
    NgSwitch,
    NgSwitchCase,
    NgSwitchDefault,
    NgForOf,
    Button,
    Card,
    TableModule,
    DropdownModule,
    FormsModule,
    Toast,
    Select,
    InputNumber,
    ToggleSwitch
  ],
  templateUrl: './datapoint-details.component.html',
  styleUrl: './datapoint-details.component.scss'
})
export class DatapointDetailsComponent implements OnChanges, OnInit {
  @Input()
  item: DataPoint | null = null;

  isEditing = false;
  editValue: number | boolean | string = 0;

  simulationModes = [
    {label: 'None', value: SimulationMode.None, icon: 'pi pi-ban'},
    {label: 'Cyclic Random', value: SimulationMode.Cyclic, icon: 'pi pi-sort-alt'},
    {label: 'Cyclic Static', value: SimulationMode.CyclicStatic, icon: 'pi pi-lock'},
    {label: 'Response', value: SimulationMode.Response, icon: 'pi pi-reply'},
    {label: 'Predefined Profile', value: SimulationMode.PredefinedProfile, icon: 'pi pi-chart-line'}
  ];

  doublePointOptions = [
    {label: 'INTERMEDIATE', value: DoublePointValue.INTERMEDIATE},
    {label: 'OFF', value: DoublePointValue.OFF},
    {label: 'ON', value: DoublePointValue.ON},
    {label: 'INDETERMINATE', value: DoublePointValue.INDETERMINATE}
  ];

  availableProfiles: string[] = [];

  constructor(
    private http: HttpClient,
    private dataService: DataService
  ) {}

  ngOnInit() {
    this.dataService.fetchProfiles().subscribe(profiles => {
      this.availableProfiles = profiles;
    });
  }

  get isPredefinedProfile(): boolean {
    return this.item?.mode === SimulationMode.PredefinedProfile;
  }

  get valueType(): string {
    if (!this.item) return 'numeric';
    return getValueType(this.item.iec104DataType);
  }

  get isAutoMode(): boolean {
    if (!this.item) return false;
    return this.item.mode === SimulationMode.Cyclic
      || this.item.mode === SimulationMode.CyclicStatic
      || this.item.mode === SimulationMode.PredefinedProfile;
  }

  displayValue(): string {
    return getDisplayValue(this.item?.value);
  }

  startEditing() {
    if (!this.item) return;
    const vt = this.valueType;
    if (vt === 'singlePoint') {
      this.editValue = this.item.value?.singlePointValue?.value ?? false;
    } else if (vt === 'doublePoint') {
      this.editValue = this.item.value?.doublePointValue?.value ?? DoublePointValue.OFF;
    } else if (vt === 'float') {
      this.editValue = this.item.value?.floatValue?.value ?? 0;
    } else if (vt === 'scaled') {
      this.editValue = this.item.value?.scaledValue?.value ?? 0;
    } else {
      this.editValue = this.item.value?.numericValue?.value ?? 0;
    }
    this.isEditing = true;
  }

  cancelEditing() {
    this.isEditing = false;
  }

  saveValue() {
    if (!this.item) return;
    const vt = this.valueType;
    const newValue: IecValue = {};

    if (vt === 'singlePoint') {
      newValue.singlePointValue = {value: this.editValue as boolean};
    } else if (vt === 'doublePoint') {
      newValue.doublePointValue = {value: this.editValue as DoublePointValue};
    } else if (vt === 'float') {
      newValue.floatValue = {value: this.editValue as number};
    } else if (vt === 'scaled') {
      newValue.scaledValue = {value: this.editValue as number, shortValue: this.editValue as number};
    } else {
      newValue.numericValue = {value: this.editValue as number};
    }

    this.item.value = newValue;
    this.isEditing = false;
    this.dataService.updateDataPointValue(this.item);
  }

  toggleDoublePointValue(doublePoint: DataPoint) {
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

  onProfileNameChange(point: DataPoint) {
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
      this.isEditing = false;
      this.syncWithUpdatedData();
    }
  }
}

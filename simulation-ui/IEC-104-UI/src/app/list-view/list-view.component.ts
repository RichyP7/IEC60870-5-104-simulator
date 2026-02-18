import {Component, OnInit, Output, EventEmitter} from '@angular/core';
import {Accordion, AccordionContent, AccordionHeader, AccordionPanel} from 'primeng/accordion';
import {NgClass, NgForOf} from '@angular/common';
import {Button} from 'primeng/button';
import {Router} from '@angular/router';
import {DataService} from './DataService/data.service';
import {CreateDialogComponent} from './create-dialog/create-dialog.component';
import {Toast} from 'primeng/toast';
import {catchError, of} from 'rxjs';
import {MessageService} from 'primeng/api';

@Component({
  selector: 'app-list-view',
  standalone: true,
  imports: [
    Accordion,
    AccordionPanel,
    NgForOf,
    AccordionHeader,
    AccordionContent,
    Button,
    NgClass,
    CreateDialogComponent,
    Toast,
  ],
  templateUrl: './list-view.component.html',
  styleUrl: './list-view.component.scss'
})
export class ListViewComponent implements OnInit {

  @Output()
  itemSelected = new EventEmitter<DataPoint>();
  selectedItem: DataPoint | null = null; // Track the selected item

  groupedData: GroupedData[] = [];
  showDialog: boolean = false;

  readonly modeLabels: Record<string, string> = {
    'None': 'None',
    'Cyclic': 'Cyclic Random',
    'CyclicStatic': 'Cyclic Static',
    'Response': 'Response',
    'PredefinedProfile': 'Profile'
  };

  constructor(
    private router: Router,
    private dataService: DataService,
    private messageService: MessageService
  ) {}

  displayValue(item: DataPoint): string {
    return getDisplayValue(item.value);
  }

  modeLabel(mode: SimulationMode): string {
    return this.modeLabels[mode] || mode;
  }

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
    this.selectedItem = item;
    this.itemSelected.emit(item)
  }

  reloadData() {
    this.ngOnInit();
  }

  processDialogChange($event: boolean) {
    console.log($event);
    this.showDialog = false;
  }

  createDataPoint(datapoint: DataPoint) {
    this.dataService.createDataPoint(datapoint)
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

export interface DataPoint {
  id: string;
  stationaryAddress: number;
  objectAddress: number;
  iec104DataType: string;
  value: IecValue;
  mode: SimulationMode;
  profileName?: string;
}

export interface IecValue {
  numericValue?: { value: number } | null;
  singlePointValue?: { value: boolean } | null;
  doublePointValue?: { value: DoublePointValue } | null;
  floatValue?: { value: number } | null;
  scaledValue?: { value: number; shortValue: number } | null;
}

export enum DoublePointValue {
  INTERMEDIATE = 'INTERMEDIATE',
  OFF = 'OFF',
  ON = 'ON',
  INDETERMINATE = 'INDETERMINATE'
}

export function getDisplayValue(value: IecValue | null | undefined): string {
  if (!value) return '-';
  if (value.singlePointValue != null) return value.singlePointValue.value ? 'ON' : 'OFF';
  if (value.doublePointValue != null) return value.doublePointValue.value.toString();
  if (value.floatValue != null) return value.floatValue.value.toString();
  if (value.scaledValue != null) return value.scaledValue.value.toString();
  if (value.numericValue != null) return value.numericValue.value.toString();
  return '-';
}

export function getValueType(dataType: string): 'singlePoint' | 'doublePoint' | 'float' | 'scaled' | 'numeric' {
  if (dataType.startsWith('M_SP_') || dataType === 'C_SC_NA_1' || dataType === 'C_SC_TA_1') return 'singlePoint';
  if (dataType.startsWith('M_DP_') || dataType === 'C_DC_NA_1' || dataType === 'C_DC_TA_1') return 'doublePoint';
  if (dataType.startsWith('M_ME_NC_') || dataType.startsWith('M_ME_TC_') || dataType.startsWith('M_ME_TF_')) return 'float';
  if (dataType.startsWith('M_ME_NB_') || dataType.startsWith('M_ME_TB_') || dataType.startsWith('M_ME_TE_')
    || dataType === 'C_SE_NB_1' || dataType === 'C_SE_TB_1') return 'scaled';
  return 'numeric';
}

export interface GroupedData {
  stationaryAddress: number;
  items: DataPoint[];
}

export enum SimulationMode {
  None = 'None',
  Cyclic = 'Cyclic',
  CyclicStatic = 'CyclicStatic',
  Response = 'Response',
  PredefinedProfile = 'PredefinedProfile'
}

export enum Iec104DataTypes {
  ASDU_TYPEUNDEF = "ASDU_TYPEUNDEF",
  M_SP_NA_1 = "M_SP_NA_1",
  M_SP_TA_1 = "M_SP_TA_1",
  M_DP_NA_1 = "M_DP_NA_1",
  M_DP_TA_1 = "M_DP_TA_1",
  M_ST_NA_1 = "M_ST_NA_1",
  M_ST_TA_1 = "M_ST_TA_1",
  M_BO_NA_1 = "M_BO_NA_1",
  M_BO_TA_1 = "M_BO_TA_1",
  M_ME_NA_1 = "M_ME_NA_1",
  M_ME_TA_1 = "M_ME_TA_1",
  M_ME_NB_1 = "M_ME_NB_1",
  M_ME_TB_1 = "M_ME_TB_1",
  M_ME_NC_1 = "M_ME_NC_1",
  M_ME_TC_1 = "M_ME_TC_1",
  M_IT_NA_1 = "M_IT_NA_1",
  M_IT_TA_1 = "M_IT_TA_1",
  M_EP_TA_1 = "M_EP_TA_1",
  M_EP_TB_1 = "M_EP_TB_1",
  M_EP_TC_1 = "M_EP_TC_1",
  M_PS_NA_1 = "M_PS_NA_1",
  M_ME_ND_1 = "M_ME_ND_1",
  ASDU_TYPE_22_29 = "ASDU_TYPE_22_29",
  M_SP_TB_1 = "M_SP_TB_1",
  M_DP_TB_1 = "M_DP_TB_1",
  M_ST_TB_1 = "M_ST_TB_1",
  M_BO_TB_1 = "M_BO_TB_1",
  M_ME_TD_1 = "M_ME_TD_1",
  M_ME_TE_1 = "M_ME_TE_1",
  M_ME_TF_1 = "M_ME_TF_1",
  M_IT_TB_1 = "M_IT_TB_1",
  M_EP_TD_1 = "M_EP_TD_1",
  M_EP_TE_1 = "M_EP_TE_1",
  M_EP_TF_1 = "M_EP_TF_1",
  ASDU_TYPE_41_44 = "ASDU_TYPE_41_44",
  C_SC_NA_1 = "C_SC_NA_1",
  C_DC_NA_1 = "C_DC_NA_1",
  C_RC_NA_1 = "C_RC_NA_1",
  C_SE_NA_1 = "C_SE_NA_1",
  C_SE_NB_1 = "C_SE_NB_1",
  C_SE_NC_1 = "C_SE_NC_1",
  C_BO_NA_1 = "C_BO_NA_1",
  ASDU_TYPE_52_57 = "ASDU_TYPE_52_57",
  C_SC_TA_1 = "C_SC_TA_1",
  C_DC_TA_1 = "C_DC_TA_1",
  C_RC_TA_1 = "C_RC_TA_1",
  C_SE_TA_1 = "C_SE_TA_1",
  C_SE_TB_1 = "C_SE_TB_1",
  C_SE_TC_1 = "C_SE_TC_1",
  C_BO_TA_1 = "C_BO_TA_1",
  ASDU_TYPE_65_69 = "ASDU_TYPE_65_69",
  M_EI_NA_1 = "M_EI_NA_1",
  ASDU_TYPE_71_99 = "ASDU_TYPE_71_99",
  C_IC_NA_1 = "C_IC_NA_1",
  C_CI_NA_1 = "C_CI_NA_1",
  C_RD_NA_1 = "C_RD_NA_1",
  C_CS_NA_1 = "C_CS_NA_1",
  C_TS_NA_1 = "C_TS_NA_1",
  C_RP_NA_1 = "C_RP_NA_1",
  C_CD_NA_1 = "C_CD_NA_1",
  C_TS_TA_1 = "C_TS_TA_1",
  ASDU_TYPE_108_109 = "ASDU_TYPE_108_109",
  P_ME_NA_1 = "P_ME_NA_1",
  P_ME_NB_1 = "P_ME_NB_1",
  P_ME_NC_1 = "P_ME_NC_1",
  P_AC_NA_1 = "P_AC_NA_1",
  ASDU_TYPE_114_119 = "ASDU_TYPE_114_119",
  F_FR_NA_1 = "F_FR_NA_1",
  F_SR_NA_1 = "F_SR_NA_1",
  F_SC_NA_1 = "F_SC_NA_1",
  F_LS_NA_1 = "F_LS_NA_1",
  F_FA_NA_1 = "F_FA_NA_1",
  F_SG_NA_1 = "F_SG_NA_1",
  F_DR_TA_1 = "F_DR_TA_1",
  ASDU_TYPE_127_255 = "ASDU_TYPE_127_255"
}



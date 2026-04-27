import {Component, OnInit, OnDestroy} from '@angular/core';
import {Accordion, AccordionContent, AccordionHeader, AccordionPanel} from 'primeng/accordion';
import {NgClass, NgForOf, NgIf} from '@angular/common';
import {Button} from 'primeng/button';
import {DataService} from './DataService/data.service';
import {CreateDialogComponent} from './create-dialog/create-dialog.component';
import {Toast} from 'primeng/toast';
import {Dialog} from 'primeng/dialog';
import {catchError, of, Subscription} from 'rxjs';
import {MessageService} from 'primeng/api';
import {SignalRService, DataPointUpdate} from '../services/signal-r.service';
import {DatapointDetailsComponent} from '../datapoint-details/datapoint-details.component';

@Component({
  selector: 'app-list-view',
  standalone: true,
  imports: [
    Accordion,
    AccordionPanel,
    NgForOf,
    NgClass,
    NgIf,
    AccordionHeader,
    AccordionContent,
    Button,
    CreateDialogComponent,
    DatapointDetailsComponent,
    Dialog,
    Toast,
  ],
  templateUrl: './list-view.component.html',
  styleUrl: './list-view.component.scss'
})
export class ListViewComponent implements OnInit, OnDestroy {

  selectedItem: DataPoint | null = null;
  editItem: DataPoint | null = null;
  showEditDialog = false;
  showDeleteConfirm = false;
  deleteTarget: DataPoint | null = null;

  groupedData: GroupedData[] = [];
  activeValues: number[] = [];
  showDialog: boolean = false;

  /** Set of "ca:oa" keys for rows that recently changed — cleared after 500ms */
  recentlyChanged = new Set<string>();

  private subs: Subscription[] = [];

  constructor(
    private dataService: DataService,
    private messageService: MessageService,
    private signalR: SignalRService
  ) {}

  ngOnInit() {
    // Initial data from REST
    this.dataService.data$.subscribe((data) => {
      this.groupedData = this.groupDataByStationaryAddress(data);
      // Expand all panels (indices match groupedData order)
      this.activeValues = this.groupedData.map((_, i) => i);
    });
    this.dataService.fetchData();

    // Real-time full snapshot from SignalR
    this.subs.push(
      this.signalR.fullSnapshot$.subscribe(updates => {
        this.applySnapshot(updates);
      })
    );

    // Immediate per-datapoint updates from scenario steps (no waiting for next cyclic snapshot)
    this.subs.push(
      this.signalR.dataPointChanged$.subscribe(update => {
        this.applySnapshot([update]);
      })
    );
  }

  ngOnDestroy(): void {
    this.subs.forEach(s => s.unsubscribe());
  }

  private applySnapshot(updates: DataPointUpdate[]): void {
    const currentMap = new Map<string, DataPoint>();
    this.groupedData.forEach(g => g.items.forEach(dp => {
      currentMap.set(`${dp.stationaryAddress}:${dp.objectAddress}`, dp);
    }));

    updates.forEach(u => {
      const key = `${u.stationaryAddress}:${u.objectAddress}`;
      const existing = currentMap.get(key);

      if (existing) {
        const changed = JSON.stringify(existing.value) !== JSON.stringify(u.value) || existing.mode !== (u.mode as SimulationMode);
        existing.value = u.value;
        existing.mode = u.mode as SimulationMode;
        existing.frozen = u.frozen;
        if (changed) {
          this.recentlyChanged.add(key);
          setTimeout(() => this.recentlyChanged.delete(key), 500);
        }
      } else {
        // New datapoint appeared — rebuild groups
        const newDp: DataPoint = {
          id: u.id,
          stationaryAddress: u.stationaryAddress,
          objectAddress: u.objectAddress,
          iec104DataType: u.iec104DataType,
          value: u.value,
          mode: u.mode as SimulationMode,
          frozen: u.frozen
        };
        currentMap.set(key, newDp);
        this.groupedData = this.groupDataByStationaryAddress(Array.from(currentMap.values()));
      }
    });
  }

  private formatValue(v: unknown): string {
    if (v === null || v === undefined) return '';
    if (typeof v === 'object') return JSON.stringify(v);
    return String(v);
  }

  formatDisplayValue(value: any): string {
    if (value === null || value === undefined) return '—';
    if (typeof value === 'string') {
      try { return this.extractValueFromDto(JSON.parse(value)); } catch { return value; }
    }
    if (typeof value === 'object') return this.extractValueFromDto(value);
    return String(value);
  }

  private static readonly DPI_LABELS: Record<number, string> = {
    0: 'Intermediate',
    1: 'OFF (Opened)',
    2: 'ON (Closed)',
    3: 'Indeterminate (Bad)',
  };

  private extractValueFromDto(dto: any): string {
    if (!dto) return '—';
    if (dto.singlePointValue != null) return dto.singlePointValue.value ? 'ON' : 'OFF';
    if (dto.doublePointValue != null)
      return ListViewComponent.DPI_LABELS[dto.doublePointValue.value] ?? String(dto.doublePointValue.value);
    if (dto.floatValue != null) return (+dto.floatValue.value).toFixed(3);
    if (dto.scaledValue != null) return String(dto.scaledValue.value);
    if (dto.numericValue != null) return String(dto.numericValue.value);
    return JSON.stringify(dto);
  }

  isRecentlyChanged(item: DataPoint): boolean {
    return this.recentlyChanged.has(`${item.stationaryAddress}:${item.objectAddress}`);
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
    this.selectedItem = item;
  }

  openEdit(item: DataPoint, event: Event): void {
    event.stopPropagation();
    this.editItem = item;
    this.showEditDialog = true;
  }

  closeEditDialog(): void {
    this.showEditDialog = false;
    this.editItem = null;
  }

  onDataPointSaved(updated: DataPoint): void {
    this.showEditDialog = false;
    this.editItem = null;
    this.dataService.fetchData();
  }

  confirmDelete(item: DataPoint, event: Event): void {
    event.stopPropagation();
    this.deleteTarget = item;
    this.showDeleteConfirm = true;
  }

  cancelDelete(): void {
    this.showDeleteConfirm = false;
    this.deleteTarget = null;
  }

  executeDelete(): void {
    if (!this.deleteTarget) return;
    this.dataService.deleteDataPoint(this.deleteTarget).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Deleted', detail: `DataPoint ${this.deleteTarget!.id} removed.` });
        this.showDeleteConfirm = false;
        this.deleteTarget = null;
        this.dataService.fetchData();
      },
      error: () => {
        this.showDeleteConfirm = false;
        this.deleteTarget = null;
      }
    });
  }

  reloadData() {
    this.dataService.fetchData();
  }

  processDialogChange($event: boolean) {
    this.showDialog = false;
  }

  createDataPoint(datapoint: DataPoint) {
    this.dataService.createDataPoint(datapoint)
      .pipe(
        catchError(error => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error - Bad Request',
            detail: error.error?.exceptionMessage ?? 'Failed to create datapoint',
          });
          return of(null);
        })
      )
      .subscribe(response => {
        if (response) this.reloadData();
      });
  }
}

export interface IecValueDto {
  numericValue?: { value: number } | null;
  singlePointValue?: { value: boolean } | null;
  doublePointValue?: { value: number } | null;
  floatValue?: { value: number } | null;
  scaledValue?: { value: number; shortValue: number } | null;
}

export interface DataPoint {
  id: string;
  stationaryAddress: number;
  objectAddress: number;
  iec104DataType: string;
  value: IecValueDto | any;
  mode: SimulationMode;
  baseValue?: number | null;
  minValue?: number | null;
  maxValue?: number | null;
  fluctuationRate?: number | null;
  linkedPowerPointId?: string | null;
  profileValues?: number[] | null;
  frozen?: boolean;
}

export interface GroupedData {
  stationaryAddress: number;
  items: DataPoint[];
}

export enum SimulationMode {
  Static            = 'Static',
  Periodic          = 'Periodic',
  RandomWalk        = 'RandomWalk',
  GaussianNoise     = 'GaussianNoise',
  Solar             = 'Solar',
  Wind              = 'Wind',
  EnergyCounter     = 'EnergyCounter',
  CounterOnDemand   = 'CounterOnDemand',
  Profile           = 'Profile',
  CommandResponse   = 'CommandResponse',
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



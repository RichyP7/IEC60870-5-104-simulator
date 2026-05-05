import {Component, EventEmitter, Input, OnChanges, Output, SimpleChanges} from '@angular/core';
import {DataPoint, IecValueDto, SimulationMode} from '../list-view/list-view.component';
import {CommonModule} from '@angular/common';
import {DataService} from '../list-view/DataService/data.service';
import {Button} from 'primeng/button';
import {Select} from 'primeng/select';
import {FormBuilder, FormGroup, ReactiveFormsModule} from '@angular/forms';
import {InputText} from 'primeng/inputtext';
import {ToggleSwitch} from 'primeng/toggleswitch';
import {InputNumber} from 'primeng/inputnumber';
import {Divider} from 'primeng/divider';
import {MessageService} from 'primeng/api';

// ── Mode options with human-readable labels ──────────────────────────────────

interface ModeOption { label: string; value: SimulationMode; }

const MODE_OPTIONS: Record<SimulationMode, ModeOption> = {
  [SimulationMode.Static]:          { label: 'Static (interrogation / startup only)',   value: SimulationMode.Static },
  [SimulationMode.Periodic]:        { label: 'Periodic (fixed value, every cycle)',      value: SimulationMode.Periodic },
  [SimulationMode.RandomWalk]:      { label: 'Random Walk (incremental changes)',        value: SimulationMode.RandomWalk },
  [SimulationMode.GaussianNoise]:   { label: 'Gaussian Noise (around base value)',       value: SimulationMode.GaussianNoise },
  [SimulationMode.PeriodicWave]:    { label: 'Periodic Wave (configurable sine period)',  value: SimulationMode.PeriodicWave },
  [SimulationMode.EnergyCounter]:   { label: 'Energy Counter (accumulates + sends)',     value: SimulationMode.EnergyCounter },
  [SimulationMode.CounterOnDemand]: { label: 'Counter On Demand (silent accumulation)',  value: SimulationMode.CounterOnDemand },
  [SimulationMode.Profile]:         { label: 'Profile (predefined float array)',         value: SimulationMode.Profile },
  [SimulationMode.CommandResponse]: { label: 'Command Response (mirrors ACK)',           value: SimulationMode.CommandResponse },
};

/**
 * Maps each IEC-104 ASDU type to the simulation modes that are meaningful for
 * that value category, based on the IEC 60870-5-104 standard.
 *
 * - Single/Double Point, Step Position → boolean/discrete states only
 * - Bitstring → no meaningful simulation, only static repeat
 * - Measured Normalized/Scaled → numeric with bounded range (no Solar/Wind — those need float precision)
 * - Measured Float → full range, supports renewable energy curves
 * - Integrated Totals (BCR counter) → Counter accumulation, static
 * - Command types → only respond to control direction messages
 */
const TYPE_MODE_MAP: Record<string, SimulationMode[]> = {
  // Single Point Information
  M_SP_NA_1: [SimulationMode.Static, SimulationMode.Periodic, SimulationMode.RandomWalk],
  M_SP_TA_1: [SimulationMode.Static, SimulationMode.Periodic, SimulationMode.RandomWalk],
  M_SP_TB_1: [SimulationMode.Static, SimulationMode.Periodic, SimulationMode.RandomWalk],

  // Double Point Information
  M_DP_NA_1: [SimulationMode.Static, SimulationMode.Periodic, SimulationMode.RandomWalk],
  M_DP_TA_1: [SimulationMode.Static, SimulationMode.Periodic, SimulationMode.RandomWalk],
  M_DP_TB_1: [SimulationMode.Static, SimulationMode.Periodic, SimulationMode.RandomWalk],

  // Step Position Information
  M_ST_NA_1: [SimulationMode.Static, SimulationMode.Periodic, SimulationMode.RandomWalk],
  M_ST_TA_1: [SimulationMode.Static, SimulationMode.Periodic, SimulationMode.RandomWalk],
  M_ST_TB_1: [SimulationMode.Static, SimulationMode.Periodic, SimulationMode.RandomWalk],

  // Bitstring — no meaningful numeric simulation
  M_BO_NA_1: [SimulationMode.Static, SimulationMode.Periodic],
  M_BO_TA_1: [SimulationMode.Static, SimulationMode.Periodic],
  M_BO_TB_1: [SimulationMode.Static, SimulationMode.Periodic],

  // Measured Value, Normalized ([-1.0, 1.0]) — bounded numeric
  M_ME_NA_1: [SimulationMode.Static, SimulationMode.Periodic, SimulationMode.RandomWalk, SimulationMode.GaussianNoise, SimulationMode.Profile],
  M_ME_TA_1: [SimulationMode.Static, SimulationMode.Periodic, SimulationMode.RandomWalk, SimulationMode.GaussianNoise, SimulationMode.Profile],
  M_ME_ND_1: [SimulationMode.Static, SimulationMode.Periodic, SimulationMode.RandomWalk, SimulationMode.GaussianNoise, SimulationMode.Profile],
  M_ME_TD_1: [SimulationMode.Static, SimulationMode.Periodic, SimulationMode.RandomWalk, SimulationMode.GaussianNoise, SimulationMode.Profile],

  // Measured Value, Scaled (int16) — bounded numeric
  M_ME_NB_1: [SimulationMode.Static, SimulationMode.Periodic, SimulationMode.RandomWalk, SimulationMode.GaussianNoise, SimulationMode.Profile],
  M_ME_TB_1: [SimulationMode.Static, SimulationMode.Periodic, SimulationMode.RandomWalk, SimulationMode.GaussianNoise, SimulationMode.Profile],
  M_ME_TE_1: [SimulationMode.Static, SimulationMode.Periodic, SimulationMode.RandomWalk, SimulationMode.GaussianNoise, SimulationMode.Profile],

  // Measured Value, Short Float (IEEE754) — full float, supports energy curves
  M_ME_NC_1: [SimulationMode.Static, SimulationMode.Periodic, SimulationMode.RandomWalk, SimulationMode.GaussianNoise, SimulationMode.PeriodicWave, SimulationMode.Profile, SimulationMode.EnergyCounter, SimulationMode.CounterOnDemand],
  M_ME_TC_1: [SimulationMode.Static, SimulationMode.Periodic, SimulationMode.RandomWalk, SimulationMode.GaussianNoise, SimulationMode.PeriodicWave, SimulationMode.Profile, SimulationMode.EnergyCounter, SimulationMode.CounterOnDemand],
  M_ME_TF_1: [SimulationMode.Static, SimulationMode.Periodic, SimulationMode.RandomWalk, SimulationMode.GaussianNoise, SimulationMode.PeriodicWave, SimulationMode.Profile, SimulationMode.EnergyCounter, SimulationMode.CounterOnDemand],

  // Integrated Totals / Binary Counter Readings (BCR)
  M_IT_NA_1: [SimulationMode.Static, SimulationMode.Periodic, SimulationMode.EnergyCounter, SimulationMode.CounterOnDemand],
  M_IT_TA_1: [SimulationMode.Static, SimulationMode.Periodic, SimulationMode.EnergyCounter, SimulationMode.CounterOnDemand],
  M_IT_TB_1: [SimulationMode.Static, SimulationMode.Periodic, SimulationMode.EnergyCounter, SimulationMode.CounterOnDemand],

  // Packed Single Point with status change detection
  M_PS_NA_1: [SimulationMode.Static, SimulationMode.Periodic],

  // Single Command
  C_SC_NA_1: [SimulationMode.Static, SimulationMode.CommandResponse],
  C_SC_TA_1: [SimulationMode.Static, SimulationMode.CommandResponse],

  // Double Command
  C_DC_NA_1: [SimulationMode.Static, SimulationMode.CommandResponse],
  C_DC_TA_1: [SimulationMode.Static, SimulationMode.CommandResponse],

  // Regulating Step Command
  C_RC_NA_1: [SimulationMode.Static, SimulationMode.CommandResponse],
  C_RC_TA_1: [SimulationMode.Static, SimulationMode.CommandResponse],

  // Set Point Commands
  C_SE_NA_1: [SimulationMode.Static, SimulationMode.CommandResponse],
  C_SE_NB_1: [SimulationMode.Static, SimulationMode.CommandResponse],
  C_SE_NC_1: [SimulationMode.Static, SimulationMode.CommandResponse],
  C_SE_TA_1: [SimulationMode.Static, SimulationMode.CommandResponse],
  C_SE_TB_1: [SimulationMode.Static, SimulationMode.CommandResponse],
  C_SE_TC_1: [SimulationMode.Static, SimulationMode.CommandResponse],

  // Bitstring Command
  C_BO_NA_1: [SimulationMode.Static, SimulationMode.CommandResponse],
  C_BO_TA_1: [SimulationMode.Static, SimulationMode.CommandResponse],
};

/** Fallback for unmapped types */
const DEFAULT_MODES = [SimulationMode.Static, SimulationMode.Periodic];

// ── DPI label map shared with list view ──────────────────────────────────────

export interface DpiOption { label: string; value: number; }

/** 0=INTERMEDIATE, 1=OFF, 2=ON, 3=INDETERMINATE — matches IecDoublePointValue C# enum order */
export const DPI_OPTIONS: DpiOption[] = [
  { value: 0, label: 'Intermediate' },
  { value: 1, label: 'OFF (Opened)' },
  { value: 2, label: 'ON (Closed)' },
  { value: 3, label: 'Indeterminate (Bad)' },
];

// ─────────────────────────────────────────────────────────────────────────────

@Component({
  selector: 'app-datapoint-details',
  standalone: true,
  imports: [
    CommonModule,
    Button,
    Select,
    ReactiveFormsModule,
    InputText,
    ToggleSwitch,
    InputNumber,
    Divider,
  ],
  templateUrl: './datapoint-details.component.html',
  styleUrl: './datapoint-details.component.scss'
})
export class DatapointDetailsComponent implements OnChanges {
  @Input() item: DataPoint | null = null;
  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<DataPoint>();

  form: FormGroup;

  dpiOptions = DPI_OPTIONS;

  constructor(
    private fb: FormBuilder,
    private dataService: DataService,
    private messageService: MessageService
  ) {
    this.form = this.fb.group({
      mode: [SimulationMode.Static],
      simpleValue: [null],
      baseValue: [null],
      minValue: [null],
      maxValue: [null],
      fluctuationRate: [null],
      wavePeriodSeconds: [null],
      linkedDataPointId: [null],
      profileValues: [''],
    });
  }

  // ── Frozen state ─────────────────────────────────────────────────────────────

  get isFrozen(): boolean {
    return this.item?.frozen ?? false;
  }

  // ── Available modes for current type ─────────────────────────────────────────

  get availableSimulationModes(): ModeOption[] {
    const type = this.item?.iec104DataType ?? '';
    const allowed = TYPE_MODE_MAP[type] ?? DEFAULT_MODES;
    return allowed.map(m => MODE_OPTIONS[m]);
  }

  /** Grouped variant for the dropdown: "Every cycle" vs "On trigger only" */
  get groupedSimulationModes(): { label: string; items: ModeOption[] }[] {
    const flat = this.availableSimulationModes;
    const periodic = flat.filter(o =>
      [SimulationMode.Periodic, SimulationMode.RandomWalk, SimulationMode.GaussianNoise,
       SimulationMode.PeriodicWave, SimulationMode.EnergyCounter, SimulationMode.Profile]
      .includes(o.value));
    const triggered = flat.filter(o =>
      [SimulationMode.Static, SimulationMode.CounterOnDemand, SimulationMode.CommandResponse]
      .includes(o.value));

    const groups: { label: string; items: ModeOption[] }[] = [];
    if (periodic.length)  groups.push({ label: 'Every cycle (PERIODIC)',  items: periodic });
    if (triggered.length) groups.push({ label: 'On trigger only',          items: triggered });
    return groups;
  }

  // ── Adaptive visibility based on selected mode ──────────────────────────────

  get selectedMode(): SimulationMode {
    return this.form.get('mode')?.value as SimulationMode;
  }

  get showSimParamFields(): boolean {
    return [SimulationMode.GaussianNoise, SimulationMode.PeriodicWave, SimulationMode.RandomWalk].includes(this.selectedMode);
  }

  get showWavePeriodField(): boolean {
    return this.selectedMode === SimulationMode.PeriodicWave;
  }

  get fluctuationRateLabel(): string {
    return this.selectedMode === SimulationMode.RandomWalk ? 'Max Step Size' : 'Fluctuation Rate';
  }

  get showLinkedDataPointId(): boolean {
    return this.selectedMode === SimulationMode.EnergyCounter || this.selectedMode === SimulationMode.CounterOnDemand;
  }

  get showProfileValues(): boolean {
    return this.selectedMode === SimulationMode.Profile;
  }

  // ── Value type helpers ───────────────────────────────────────────────────────

  isSinglePoint(): boolean {
    return ['M_SP_NA_1', 'M_SP_TA_1', 'M_SP_TB_1'].includes(this.item?.iec104DataType ?? '');
  }

  isDoublePoint(): boolean {
    return ['M_DP_NA_1', 'M_DP_TA_1', 'M_DP_TB_1'].includes(this.item?.iec104DataType ?? '');
  }

  isFloatType(): boolean {
    return ['M_ME_NA_1', 'M_ME_TA_1', 'M_ME_NC_1', 'M_ME_TC_1', 'M_ME_ND_1',
            'M_ME_TD_1', 'M_ME_TF_1'].includes(this.item?.iec104DataType ?? '');
  }

  isScaledType(): boolean {
    return ['M_ME_NB_1', 'M_ME_TB_1', 'M_ME_TE_1'].includes(this.item?.iec104DataType ?? '');
  }

  // ── Value extraction / reconstruction ───────────────────────────────────────

  extractSimpleValue(value: any): any {
    if (value === null || value === undefined) return null;
    if (typeof value === 'boolean' || typeof value === 'number') return value;
    if (typeof value === 'string') {
      try { value = JSON.parse(value); } catch { return value; }
    }
    if (value.singlePointValue != null) return value.singlePointValue.value;
    if (value.doublePointValue != null) return value.doublePointValue.value;
    if (value.floatValue != null) return value.floatValue.value;
    if (value.scaledValue != null) return value.scaledValue.value;
    if (value.numericValue != null) return value.numericValue.value;
    return null;
  }

  buildValueDto(simpleValue: any): IecValueDto {
    if (this.isSinglePoint()) {
      return { singlePointValue: { value: simpleValue === true || simpleValue === 'true' } };
    }
    if (this.isDoublePoint()) {
      return { doublePointValue: { value: Number(simpleValue ?? 1) } };
    }
    if (this.isFloatType()) {
      return { floatValue: { value: parseFloat(simpleValue) || 0 } };
    }
    if (this.isScaledType()) {
      const v = parseInt(simpleValue) || 0;
      return { scaledValue: { value: v, shortValue: v } };
    }
    return { numericValue: { value: parseInt(simpleValue) || 0 } };
  }

  // ── Form population ──────────────────────────────────────────────────────────

  private populateForm(item: DataPoint): void {
    this.form.patchValue({
      mode: item.mode,
      simpleValue: this.extractSimpleValue(item.value),
      baseValue: item.baseValue ?? null,
      minValue: item.minValue ?? null,
      maxValue: item.maxValue ?? null,
      fluctuationRate: item.fluctuationRate ?? null,
      wavePeriodSeconds: item.wavePeriodSeconds ?? null,
      linkedDataPointId: item.linkedDataPointId ?? null,
      profileValues: (item.profileValues ?? []).join(', '),
    }, { emitEvent: false });

    if (item.frozen) {
      this.form.disable({ emitEvent: false });
    } else {
      this.form.enable({ emitEvent: false });
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['item'] && this.item) {
      this.populateForm(this.item);
    }
  }

  close(): void {
    this.closed.emit();
  }

  save(): void {
    if (!this.item || this.isFrozen) return;
    const fv = this.form.value;
    const payload: DataPoint = {
      ...this.item,
      mode: fv.mode,
      value: this.buildValueDto(fv.simpleValue),
      baseValue: fv.baseValue,
      minValue: fv.minValue,
      maxValue: fv.maxValue,
      fluctuationRate: fv.fluctuationRate,
      wavePeriodSeconds: fv.wavePeriodSeconds,
      linkedDataPointId: fv.linkedDataPointId,
      profileValues: fv.profileValues
        ? (fv.profileValues as string).split(',').map((s: string) => parseFloat(s.trim())).filter((n: number) => !isNaN(n))
        : [],
    };
    this.dataService.updateDataPoint(payload).subscribe({
      next: (updated: DataPoint) => {
        this.item = updated;
        this.populateForm(updated);
        this.messageService.add({ severity: 'success', summary: 'Saved', detail: 'DataPoint updated.' });
        this.saved.emit(updated);
      },
      error: () => { /* handled in service */ }
    });
  }

  reset(): void {
    if (this.item) this.populateForm(this.item);
  }
}


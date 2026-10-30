import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {InputText} from 'primeng/inputtext';
import {Dialog} from 'primeng/dialog';
import {AbstractControl, FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, ValidationErrors, Validators} from '@angular/forms';
import {Button} from 'primeng/button';
import {DataPoint, Iec104DataTypes, IecValueDto, SimulationMode} from '../../models';
import {NgClass} from '@angular/common';
import {Select} from 'primeng/select';

@Component({
    selector: 'app-create-dialog',
    imports: [
        InputText,
        Dialog,
        FormsModule,
        Button,
        ReactiveFormsModule,
        NgClass,
        Select
    ],
    templateUrl: './create-dialog.component.html',
    styleUrl: './create-dialog.component.scss'
})
export class CreateDialogComponent implements OnInit {
  @Input() visible: boolean = false;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() dataPointCreated = new EventEmitter<DataPoint>();

  profileForm: FormGroup ;

  iec104DataTypes: string[] = Object.values(Iec104DataTypes);
  simulationModes: string[] = Object.values(SimulationMode);

  constructor(private fb: FormBuilder) {
    this.profileForm = this.fb.group({
      stationaryAddress: ['', [Validators.required, Validators.pattern('^[0-9]+$')]],
      objectAddress: ['', [Validators.required, Validators.pattern('^[0-9]+$')]],
      iec104DataType: ['', Validators.required],
      mode: ['', Validators.required],
      value: [null, this.valueValidator.bind(this)]
    });

    this.profileForm.get('mode')?.markAsDirty();
    this.profileForm.get('iec104DataType')?.markAsDirty();

    // The allowed value format depends on the selected data type,
    // so re-validate the value whenever the type changes.
    this.profileForm.get('iec104DataType')?.valueChanges.subscribe(() => {
      this.profileForm.get('value')?.updateValueAndValidity();
    });
  }

  ngOnInit() {

  }

  closeDialog() {
    this.visible = false;
    this.visibleChange.emit(this.visible);
  }

  save() {
    if (this.profileForm.valid) {
      const profileData = this.profileForm.value;
      const ca = profileData.stationaryAddress;
      const ioa = profileData.objectAddress;

      let dataPoint: DataPoint = {
        id : `CA${ca}_IOA${ioa}`,
        mode: profileData.mode,
        value: this.buildValueDto(profileData.iec104DataType, profileData.value),
        stationaryAddress: profileData.stationaryAddress,
        objectAddress: profileData.objectAddress,
        iec104DataType: profileData.iec104DataType
      }
      console.log(dataPoint);
      this.dataPointCreated.emit(dataPoint);
      this.closeDialog()
    } else {
      console.log('Form is invalid');
    }
  }

  // ── Type-aware value validation ──────────────────────────────────────────────

  private static readonly SINGLE_POINT_TYPES = ['M_SP_NA_1', 'M_SP_TA_1', 'M_SP_TB_1'];
  private static readonly DOUBLE_POINT_TYPES = ['M_DP_NA_1', 'M_DP_TA_1', 'M_DP_TB_1'];
  private static readonly FLOAT_TYPES = ['M_ME_NC_1', 'M_ME_TC_1', 'M_ME_TF_1'];
  private static readonly NORMALIZED_TYPES = ['M_ME_NA_1', 'M_ME_TA_1', 'M_ME_ND_1', 'M_ME_TD_1'];
  private static readonly SCALED_TYPES = ['M_ME_NB_1', 'M_ME_TB_1', 'M_ME_TE_1'];
  private static readonly STEP_TYPES = ['M_ST_NA_1', 'M_ST_TA_1', 'M_ST_TB_1'];

  /** Which kind of value the currently selected data type expects. */
  private valueKind(dataType: string):
    'single' | 'double' | 'float' | 'normalized' | 'scaled' | 'step' | 'int' | 'unknown' {
    if (!dataType) return 'unknown';
    const c = CreateDialogComponent;
    if (c.SINGLE_POINT_TYPES.includes(dataType)) return 'single';
    if (c.DOUBLE_POINT_TYPES.includes(dataType)) return 'double';
    if (c.FLOAT_TYPES.includes(dataType)) return 'float';
    if (c.NORMALIZED_TYPES.includes(dataType)) return 'normalized';
    if (c.SCALED_TYPES.includes(dataType)) return 'scaled';
    if (c.STEP_TYPES.includes(dataType)) return 'step';
    return 'int';
  }

  /** Hint shown below the value input describing the allowed format. */
  get valueHint(): string {
    switch (this.valueKind(this.profileForm.get('iec104DataType')?.value)) {
      case 'single':
        return 'Boolean single point: enter true or false.';
      case 'double':
        return 'Double point state: 0 = intermediate, 1 = off, 2 = on, 3 = indeterminate.';
      case 'float':
        return 'Floating point number, e.g. 42.5.';
      case 'normalized':
        return 'Normalized value between -1 and 1, e.g. 0.75.';
      case 'scaled':
        return 'Whole number between -32768 and 32767.';
      case 'step':
        return 'Step position: whole number between -64 and 63.';
      case 'int':
        return 'Whole number, e.g. 100.';
      default:
        return 'Select an IEC104 data type first to see the allowed value format.';
    }
  }

  /** Validation error message for the value input (empty if valid). */
  get valueError(): string {
    const control = this.profileForm.get('value');
    if (!control || control.valid || !control.errors) return '';
    return control.errors['iecValue'] ?? '';
  }

  /** Validates the raw value input against the selected IEC 104 data type. */
  private valueValidator(control: AbstractControl): ValidationErrors | null {
    const raw = control.value;
    // Empty is allowed — buildValueDto() falls back to a default.
    if (raw === null || raw === undefined || `${raw}`.trim() === '') return null;

    const text = `${raw}`.trim();
    const dataType = this.profileForm?.get('iec104DataType')?.value;
    const fail = (msg: string): ValidationErrors => ({ iecValue: msg });

    switch (this.valueKind(dataType)) {
      case 'single':
        return ['true', 'false'].includes(text.toLowerCase())
          ? null : fail(`"${text}" is not a boolean. Use true or false.`);
      case 'double': {
        if (!/^[0-3]$/.test(text)) {
          return fail(`"${text}" is not a valid double point state. Use 0 (intermediate), 1 (off), 2 (on) or 3 (indeterminate).`);
        }
        return null;
      }
      case 'float':
        return this.isNumber(text)
          ? null : fail(`"${text}" is not a number. Use a decimal value like 42.5 (dot as decimal separator).`);
      case 'normalized': {
        if (!this.isNumber(text)) {
          return fail(`"${text}" is not a number. Use a decimal value between -1 and 1, e.g. 0.75.`);
        }
        const v = parseFloat(text);
        return v >= -1 && v <= 1
          ? null : fail(`${text} is out of range. Normalized values must be between -1 and 1.`);
      }
      case 'scaled': {
        if (!this.isInteger(text)) {
          return fail(`"${text}" is not a whole number. Use an integer like 1250.`);
        }
        const v = parseInt(text, 10);
        return v >= -32768 && v <= 32767
          ? null : fail(`${text} is out of range. Scaled values must be between -32768 and 32767.`);
      }
      case 'step': {
        if (!this.isInteger(text)) {
          return fail(`"${text}" is not a whole number. Use an integer like 10.`);
        }
        const v = parseInt(text, 10);
        return v >= -64 && v <= 63
          ? null : fail(`${text} is out of range. Step positions must be between -64 and 63.`);
      }
      case 'int':
        return this.isInteger(text)
          ? null : fail(`"${text}" is not a whole number. Use an integer like 100.`);
      default:
        return null; // no data type selected yet; required validator on the type covers this
    }
  }

  private isNumber(text: string): boolean {
    return /^-?\d+(\.\d+)?$/.test(text);
  }

  private isInteger(text: string): boolean {
    return /^-?\d+$/.test(text);
  }

  /**
   * The backend binds `value` to a complex IecValueDto object, so the raw
   * input must be wrapped according to the selected IEC 104 data type.
   * Mirrors DatapointDetailsComponent.buildValueDto().
   */
  private buildValueDto(dataType: string, simpleValue: any): IecValueDto {
    if (['M_SP_NA_1', 'M_SP_TA_1', 'M_SP_TB_1'].includes(dataType)) {
      return { singlePointValue: { value: simpleValue === true || simpleValue === 'true' } };
    }
    if (['M_DP_NA_1', 'M_DP_TA_1', 'M_DP_TB_1'].includes(dataType)) {
      const v = Number(simpleValue);
      return { doublePointValue: { value: Number.isInteger(v) && v >= 0 && v <= 3 ? v : 1 } };
    }
    if (['M_ME_NA_1', 'M_ME_TA_1', 'M_ME_NC_1', 'M_ME_TC_1', 'M_ME_ND_1',
         'M_ME_TD_1', 'M_ME_TF_1'].includes(dataType)) {
      return { floatValue: { value: parseFloat(simpleValue) || 0 } };
    }
    if (['M_ME_NB_1', 'M_ME_TB_1', 'M_ME_TE_1'].includes(dataType)) {
      const v = parseInt(simpleValue) || 0;
      return { scaledValue: { value: v, shortValue: v } };
    }
    return { numericValue: { value: parseInt(simpleValue) || 0 } };
  }

  protected readonly SimulationMode = SimulationMode;
}

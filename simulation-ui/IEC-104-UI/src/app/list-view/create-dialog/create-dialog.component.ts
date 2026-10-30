import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {InputText} from 'primeng/inputtext';
import {Dialog} from 'primeng/dialog';
import {FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
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
      value: [null]
    });

    this.profileForm.get('mode')?.markAsDirty();
    this.profileForm.get('iec104DataType')?.markAsDirty();
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
      return { doublePointValue: { value: Number(simpleValue ?? 1) || 1 } };
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

import { Component, Input, OnInit, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, Validators } from '@angular/forms';
import { DataPointValueVis } from '../../data/datapoints.interface';
import { CommonModule } from '@angular/common';
import { Iec104DataTypes } from '../../api/v1';

@Component({
  selector: 'lib-iec-value-input',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './iec-value-input.component.html',
  styleUrl: './iec-value-input.component.css'
})
export class IecValueInputComponent implements OnInit {
  @Input() valueitem: DataPointValueVis = new DataPointValueVis();
  @Input() inputIecType: string =""// = Iec104DataTypes.AsduTypeundef;
  inputType: InputType = InputType.BOOL;

  constructor() {
  }

ngOnInit(): void {
  this.inputType = this.getInputType(this.inputIecType)
  console.log(this.inputType.toString());
}
ngOnChanges(changes: SimpleChanges) {
  this.inputType = this.getInputType(this.inputIecType)
}

onSubmit() {

    console.log('Input Value:');

}
getInputType(typestring: string): InputType {
  const type = typestring as Iec104DataTypes;
  switch (type) {
    case 'M_SP_NA_1':
    case 'M_SP_TA_1':
    case 'M_SP_TB_1':
      return InputType.BOOL;
    case 'M_DP_NA_1':
    case 'M_DP_TA_1':
    case 'M_DP_TB_1':
      return InputType.DOUBLEPOINT;
    case 'M_ST_NA_1':
    case 'M_ST_TA_1':
    case 'M_ST_TB_1':
      return InputType.INTEGER;
    case 'M_BO_NA_1':
    case 'M_BO_TA_1':
    case 'M_ME_NA_1':
    case 'M_ME_TA_1':
    case 'M_ME_NB_1':
    case 'M_ME_TB_1':
    case 'M_ME_NC_1':
    case 'M_ME_TC_1':
    case 'M_IT_NA_1':
    case 'M_IT_TA_1':
    case 'M_EP_TA_1':
    case 'M_EP_TB_1':
    case 'M_EP_TC_1':
    case 'M_PS_NA_1':
    case 'M_ME_ND_1':
    case 'ASDU_TYPE_22_29':
    case 'M_BO_TB_1':
    case 'M_ME_TD_1':
    case 'M_ME_TE_1':
    case 'M_ME_TF_1':
      return InputType.FLOAT;
    case 'ASDU_TYPEUNDEF':
    case 'M_IT_TB_1':
    case 'M_EP_TD_1':
    case 'M_EP_TE_1':
    case 'M_EP_TF_1':
    case 'ASDU_TYPE_41_44':
    case 'C_SC_NA_1':
    case 'C_DC_NA_1':
    case 'C_RC_NA_1':
    case 'C_SE_NA_1':
    case 'C_SE_NB_1':
    case 'C_SE_NC_1':
    case 'C_BO_NA_1':
    case 'ASDU_TYPE_52_57':
    case 'C_SC_TA_1':
    case 'C_DC_TA_1':
    case 'C_RC_TA_1':
    case 'C_SE_TA_1':
    case 'C_SE_TB_1':
    case 'C_SE_TC_1':
    case 'C_BO_TA_1':
    case 'ASDU_TYPE_65_69':
    case 'M_EI_NA_1':
    case 'ASDU_TYPE_71_99':
    case 'C_IC_NA_1':
    case 'C_CI_NA_1':
    case 'C_RD_NA_1':
    case 'C_CS_NA_1':
    case 'C_TS_NA_1':
    case 'C_RP_NA_1':
    case 'C_CD_NA_1':
    case 'C_TS_TA_1':
    case 'ASDU_TYPE_108_109':
    case 'P_ME_NA_1':
    case 'P_ME_NB_1':
    case 'P_ME_NC_1':
    case 'P_AC_NA_1':
    case 'ASDU_TYPE_114_119':
    case 'F_FR_NA_1':
    case 'F_SR_NA_1':
    case 'F_SC_NA_1':
    case 'F_LS_NA_1':
    case 'F_FA_NA_1':
    case 'F_SG_NA_1':
    case 'F_DR_TA_1':
    case 'ASDU_TYPE_127_255':
      throw Error("input type not implemented");
  }
}

}
export enum InputType {
  INTEGER = 'INTEGER',
  FLOAT = 'FLOAT',
  BOOL = 'BOOL',
  DOUBLEPOINT = 'DOUBLEPOINT'
}   

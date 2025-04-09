import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {InputText, InputTextModule} from 'primeng/inputtext';
import {Dialog, DialogModule} from 'primeng/dialog';
import {FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {DropdownModule} from 'primeng/dropdown';
import {Button} from 'primeng/button';
import {NgClass} from '@angular/common';
import { DataPoint } from '../../data/datapoints.interface';
import { Iec104DataTypes, SimulationMode } from '../../api/v1';

@Component({
  selector: 'app-create-dialog',
  standalone: true,
  imports: [
    InputTextModule,
    DialogModule,
    FormsModule,
    DropdownModule,
    Button,
    ReactiveFormsModule,
    NgClass,
    DropdownModule
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
      id: ['', Validators.required],
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

      let dataPoint: DataPoint = {
        id : profileData.id,
        mode: profileData.mode,
        value: profileData.value,
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

  protected readonly SimulationMode = SimulationMode;
}

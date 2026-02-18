import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {InputText} from 'primeng/inputtext';
import {Dialog} from 'primeng/dialog';
import {FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {DropdownModule} from 'primeng/dropdown';
import {Button} from 'primeng/button';
import {DataPoint, Iec104DataTypes, SimulationMode} from '../list-view.component';
import {NgClass, NgIf} from '@angular/common';
import {Select} from 'primeng/select';
import {DataService} from '../DataService/data.service';

@Component({
  selector: 'app-create-dialog',
  standalone: true,
  imports: [
    InputText,
    Dialog,
    FormsModule,
    DropdownModule,
    Button,
    ReactiveFormsModule,
    NgClass,
    NgIf,
    Select
  ],
  templateUrl: './create-dialog.component.html',
  styleUrl: './create-dialog.component.scss'
})
export class CreateDialogComponent implements OnInit {
  @Input() visible: boolean = false;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() dataPointCreated = new EventEmitter<DataPoint>();

  profileForm: FormGroup;

  iec104DataTypes: string[] = Object.values(Iec104DataTypes);
  simulationModes = [
    {label: 'None', value: SimulationMode.None},
    {label: 'Cyclic Random', value: SimulationMode.Cyclic},
    {label: 'Cyclic Static', value: SimulationMode.CyclicStatic},
    {label: 'Response', value: SimulationMode.Response},
    {label: 'Predefined Profile', value: SimulationMode.PredefinedProfile}
  ];
  availableProfiles: string[] = [];

  constructor(private fb: FormBuilder, private dataService: DataService) {
    this.profileForm = this.fb.group({
      id: ['', Validators.required],
      stationaryAddress: ['', [Validators.required, Validators.pattern('^[0-9]+$')]],
      objectAddress: ['', [Validators.required, Validators.pattern('^[0-9]+$')]],
      iec104DataType: ['', Validators.required],
      mode: ['', Validators.required],
      profileName: [null]
    });

    this.profileForm.get('mode')?.markAsDirty();
    this.profileForm.get('iec104DataType')?.markAsDirty();
  }

  ngOnInit() {
    this.dataService.fetchProfiles().subscribe(profiles => {
      this.availableProfiles = profiles;
    });
  }

  get isPredefinedProfile(): boolean {
    return this.profileForm.get('mode')?.value === SimulationMode.PredefinedProfile;
  }

  closeDialog() {
    this.visible = false;
    this.visibleChange.emit(this.visible);
    this.profileForm.reset();
    this.profileForm.get('mode')?.markAsDirty();
    this.profileForm.get('iec104DataType')?.markAsDirty();
  }

  save() {
    if (this.profileForm.valid) {
      const profileData = this.profileForm.value;

      let dataPoint: DataPoint = {
        id: profileData.id,
        mode: profileData.mode,
        value: {},
        stationaryAddress: profileData.stationaryAddress,
        objectAddress: profileData.objectAddress,
        iec104DataType: profileData.iec104DataType,
        profileName: profileData.profileName
      }
      this.dataPointCreated.emit(dataPoint);
      this.closeDialog();
    }
  }

  protected readonly SimulationMode = SimulationMode;
}

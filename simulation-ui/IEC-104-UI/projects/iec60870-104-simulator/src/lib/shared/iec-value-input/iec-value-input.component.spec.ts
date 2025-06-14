import { ComponentFixture, TestBed } from '@angular/core/testing';

import { IecValueInputComponent } from './iec-value-input.component';

describe('IecValueInputComponent', () => {
  let component: IecValueInputComponent;
  let fixture: ComponentFixture<IecValueInputComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [IecValueInputComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(IecValueInputComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

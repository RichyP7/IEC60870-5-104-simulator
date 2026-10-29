import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { MessageService } from 'primeng/api';

import { DatapointDetailsComponent } from './datapoint-details.component';

describe('DatapointDetailsComponent', () => {
  let component: DatapointDetailsComponent;
  let fixture: ComponentFixture<DatapointDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DatapointDetailsComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideNoopAnimations(),
        MessageService,
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DatapointDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { MessageService } from 'primeng/api';
import { Subject, BehaviorSubject } from 'rxjs';

import { ListViewComponent } from './list-view.component';
import { SignalRService } from '../services/signal-r.service';

describe('ListViewComponent', () => {
  let component: ListViewComponent;
  let fixture: ComponentFixture<ListViewComponent>;

  beforeEach(async () => {
    const signalRStub = {
      fullSnapshot$: new Subject(),
      dataPointChanged$: new Subject(),
      scenarioUpdate$: new Subject(),
      clientCount$: new BehaviorSubject(0),
      connected$: new BehaviorSubject(false),
    };

    await TestBed.configureTestingModule({
      imports: [ListViewComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideNoopAnimations(),
        MessageService,
        { provide: SignalRService, useValue: signalRStub },
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ListViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

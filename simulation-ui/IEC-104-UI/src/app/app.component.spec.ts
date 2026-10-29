import { TestBed } from '@angular/core/testing';
import { AppComponent } from './app.component';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { MessageService } from 'primeng/api';
import { SignalRService } from './services/signal-r.service';
import { Subject, BehaviorSubject } from 'rxjs';

describe('AppComponent', () => {
  beforeEach(async () => {
    const signalRStub = {
      fullSnapshot$: new Subject(),
      dataPointChanged$: new Subject(),
      scenarioUpdate$: new Subject(),
      clientCount$: new BehaviorSubject(0),
      connected$: new BehaviorSubject(false),
    };

    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideNoopAnimations(),
        MessageService,
        { provide: SignalRService, useValue: signalRStub },
      ],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it(`should have the 'IEC-104-UI' title`, () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app.title).toEqual('IEC-104-UI');
  });
});

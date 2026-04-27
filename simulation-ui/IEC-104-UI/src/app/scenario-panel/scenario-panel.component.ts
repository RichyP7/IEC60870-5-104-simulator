import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Button } from 'primeng/button';
import { Tag } from 'primeng/tag';
import { ScenarioService, ScenarioInfo } from '../services/scenario.service';
import { SignalRService, ScenarioUpdate } from '../services/signal-r.service';
import { Subscription, interval } from 'rxjs';

@Component({
  selector: 'app-scenario-panel',
  standalone: true,
  imports: [CommonModule, Button, Tag],
  templateUrl: './scenario-panel.component.html',
})
export class ScenarioPanelComponent implements OnInit, OnDestroy {
  scenarios: ScenarioInfo[] = [];
  // Local state map updated in real-time from SignalR
  scenarioStates = new Map<string, ScenarioUpdate>();
  // Countdown timers (remainingMs decremented each second per scenario)
  countdowns = new Map<string, number>();

  private subs: Subscription[] = [];
  private countdownTimer?: ReturnType<typeof setInterval>;

  constructor(
    private scenarioService: ScenarioService,
    private signalR: SignalRService
  ) {}

  ngOnInit(): void {
    this.loadScenarios();

    // Real-time scenario status updates from SignalR
    this.subs.push(
      this.signalR.scenarioUpdate$.subscribe(update => {
        this.scenarioStates.set(update.name, update);
        this.countdowns.set(update.name, update.remainingMs);

        // Refresh scenario list when a scenario completes (to reflect final state)
        if (update.status === 'Completed' || update.status === 'Failed') {
          setTimeout(() => this.loadScenarios(), 200);
        }
      })
    );

    // Decrement countdown every second
    this.countdownTimer = setInterval(() => {
      this.countdowns.forEach((ms, name) => {
        if (ms > 0) this.countdowns.set(name, Math.max(0, ms - 1000));
      });
    }, 1000);
  }

  ngOnDestroy(): void {
    this.subs.forEach(s => s.unsubscribe());
    if (this.countdownTimer) clearInterval(this.countdownTimer);
  }

  loadScenarios(): void {
    this.scenarioService.getScenarios().subscribe({
      next: data => {
        this.scenarios = data;
        data.forEach(s => {
          if (!this.scenarioStates.has(s.name)) {
            this.scenarioStates.set(s.name, { name: s.name, status: s.status, remainingMs: s.remainingMs });
            this.countdowns.set(s.name, s.remainingMs);
          }
        });
      },
      error: err => console.error('Failed to load scenarios', err)
    });
  }

  trigger(name: string): void {
    this.scenarioService.triggerScenario(name).subscribe({
      next: () => { /* state update comes from SignalR */ },
      error: err => console.error('Failed to trigger scenario', err)
    });
  }

  isRunning(name: string): boolean {
    return this.scenarioStates.get(name)?.status === 'Running';
  }

  getStatus(name: string): string {
    return this.scenarioStates.get(name)?.status ?? 'Idle';
  }

  getCountdownSec(name: string): number {
    return Math.ceil((this.countdowns.get(name) ?? 0) / 1000);
  }

  statusSeverity(status: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    switch (status) {
      case 'Running':   return 'warn';
      case 'Completed': return 'success';
      case 'Failed':    return 'danger';
      default:          return 'secondary';
    }
  }
}

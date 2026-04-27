import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Button } from 'primeng/button';
import { Tag } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ScenarioService, ScenarioInfo, ScenarioDefinition, ScenarioStep } from '../services/scenario.service';
import { SignalRService, ScenarioUpdate } from '../services/signal-r.service';
import { Subscription, interval } from 'rxjs';

@Component({
  selector: 'app-scenario-panel',
  standalone: true,
  imports: [CommonModule, Button, Tag, TooltipModule],
  templateUrl: './scenario-panel.component.html',
})
export class ScenarioPanelComponent implements OnInit, OnDestroy {
  scenarios: ScenarioInfo[] = [];
  // Local state map updated in real-time from SignalR
  scenarioStates = new Map<string, ScenarioUpdate>();
  // Countdown timers (remainingMs decremented each second per scenario)
  countdowns = new Map<string, number>();
  // Scenario definitions loaded once for tooltip display
  private definitions = new Map<string, ScenarioDefinition>();
  scenarioTooltips = new Map<string, string>();

  private subs: Subscription[] = [];
  private countdownTimer?: ReturnType<typeof setInterval>;

  constructor(
    private scenarioService: ScenarioService,
    private signalR: SignalRService
  ) {}

  ngOnInit(): void {
    this.loadScenarios();
    this.loadDefinitions();

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

  loadDefinitions(): void {
    this.scenarioService.getScenarioDefinitions().subscribe({
      next: defs => {
        this.definitions.clear();
        this.scenarioTooltips.clear();
        defs.forEach(d => {
          this.definitions.set(d.name, d);
          this.scenarioTooltips.set(d.name, this.buildTooltip(d));
        });
      },
      error: err => console.error('Failed to load scenario definitions', err)
    });
  }

  private buildTooltip(def: ScenarioDefinition): string {
    const sortedSteps = [...def.steps].sort((a, b) => a.delayMs - b.delayMs);
    const sortedRecovery = [...def.recoverySteps].sort((a, b) => a.delayMs - b.delayMs);

    const formatStep = (s: ScenarioStep): string => {
      const freeze = s.freeze ? ' \u{1F512}' : '';
      return `+${s.delayMs}ms &nbsp; CA:${s.ca}/OA:${s.oa} &rarr; ${s.valueStr}${freeze} &mdash; ${s.description}`;
    };

    const faultLines = sortedSteps.map(s => `<li>${formatStep(s)}</li>`).join('');
    const recoveryLines = sortedRecovery.map(s => `<li>${formatStep(s)}</li>`).join('');

    return [
      '<div style="font-size:0.8rem">',
      '<b>Fault Steps</b>',
      `<ul style="margin:4px 0 8px 0;padding-left:14px">${faultLines}</ul>`,
      `<b>Recovery Steps</b> <span style="font-weight:normal;opacity:0.7">(after ${def.recoveryMs}ms)</span>`,
      `<ul style="margin:4px 0 0 0;padding-left:14px">${recoveryLines}</ul>`,
      '</div>'
    ].join('');
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

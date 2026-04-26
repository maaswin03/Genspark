import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-operator-stub-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="page-shell">
      <div class="card border-0 shadow-sm">
        <div class="card-body p-4">
          <p class="text-uppercase text-secondary small fw-semibold mb-2">Operator</p>
          <h2 class="h4 fw-bold mb-2">{{ title }}</h2>
          <p class="text-secondary mb-0">
            This section is ready in the new operator shell. You can now build business features here without touching layout wiring.
          </p>
        </div>
      </div>
    </section>
  `,
  styles: [
    `
      .page-shell {
        min-height: 100%;
      }
    `,
  ],
})
export class OperatorStubPageComponent {
  constructor(private readonly route: ActivatedRoute) {}

  get title(): string {
    return this.route.snapshot.data['title'] ?? 'Operator Page';
  }
}

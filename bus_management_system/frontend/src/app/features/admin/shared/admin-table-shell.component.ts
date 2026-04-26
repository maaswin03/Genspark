import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-admin-table-shell',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="table-responsive admin-table-wrap">
      <table class="table align-middle mb-0">
        <ng-content></ng-content>
      </table>
    </div>
  `,
  styles: [
    `
      .admin-table-wrap {
        border: 1px solid #e2e8f0;
        border-radius: 6px;
        background: #fff;
      }

      :host ::ng-deep th {
        font-size: 0.82rem;
        text-transform: uppercase;
        color: #64748b;
        white-space: nowrap;
      }
    `,
  ],
})
export class AdminTableShellComponent {}

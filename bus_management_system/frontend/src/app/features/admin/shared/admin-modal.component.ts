import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-admin-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="admin-modal-backdrop" *ngIf="open" (click)="handleBackdropClick($event)">
      <section class="admin-modal-card" role="dialog" aria-modal="true" [attr.aria-label]="title" [style.width]="'min(' + maxWidth + ', 100%)'">
        <header class="admin-modal-head">
          <div>
            <h3 class="h6 fw-bold mb-1">{{ title }}</h3>
            <p class="text-secondary mb-0 small" *ngIf="subtitle">{{ subtitle }}</p>
          </div>
          <button type="button" class="btn-close" aria-label="Close" (click)="closed.emit()"></button>
        </header>

        <div class="admin-modal-body">
          <ng-content></ng-content>
        </div>
      </section>
    </div>
  `,
  styles: [
    `
      .admin-modal-backdrop {
        position: fixed;
        inset: 0;
        background: rgba(15, 23, 42, 0.45);
        display: flex;
        align-items: center;
        justify-content: center;
        padding: 1rem;
        z-index: 1100;
      }

      .admin-modal-card {
        width: min(680px, 100%);
        max-height: calc(100vh - 2rem);
        overflow: auto;
        background: #fff;
        border: 1px solid #e2e8f0;
        border-radius: 6px;
        box-shadow: 0 18px 44px rgba(15, 23, 42, 0.2);
      }

      .admin-modal-head {
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        gap: 1rem;
        padding: 0.95rem 1rem;
        border-bottom: 1px solid #f1f5f9;
      }

      .admin-modal-body {
        padding: 1rem;
      }
    `,
  ],
})
export class AdminModalComponent {
  @Input() open = false;
  @Input() title = '';
  @Input() subtitle = '';
  @Input() maxWidth = '680px';

  @Output() closed = new EventEmitter<void>();

  handleBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.closed.emit();
    }
  }
}

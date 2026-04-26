import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

type BadgeTone = 'neutral' | 'success' | 'danger' | 'warning' | 'dark' | 'info';

@Component({
  selector: 'app-admin-badge',
  standalone: true,
  imports: [CommonModule],
  template: `<span class="badge" [class]="badgeClass">{{ text }}</span>`,
})
export class AdminBadgeComponent {
  @Input() text = '';
  @Input() tone: BadgeTone = 'neutral';

  get badgeClass(): string {
    switch (this.tone) {
      case 'success':
        return 'badge text-bg-success';
      case 'danger':
        return 'badge text-bg-danger';
      case 'warning':
        return 'badge text-bg-warning';
      case 'dark':
        return 'badge text-bg-dark';
      case 'info':
        return 'badge text-bg-info';
      default:
        return 'badge text-bg-light';
    }
  }
}

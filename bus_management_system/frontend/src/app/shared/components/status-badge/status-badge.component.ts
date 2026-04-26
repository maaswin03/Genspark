import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule],
  template: `<span class="status-badge" [ngClass]="toneClass">{{ label }}</span>`,
  styleUrl: './status-badge.component.css',
})
export class StatusBadgeComponent {
  @Input() status: number | string = '';

  get label(): string {
    const status = this.normalize(this.status);

    if (status === '0' || status === 'pending') {
      return 'Pending';
    }

    if (status === '1' || status === 'confirmed' || status === 'paid' || status === 'completed' || status === 'success') {
      return 'Confirmed';
    }

    if (status === '2' || status === 'cancelled' || status === 'failed') {
      return 'Cancelled';
    }

    if (status === 'upcoming') {
      return 'Upcoming';
    }

    return this.toLabel(status || 'Unknown');
  }

  get toneClass(): string {
    const status = this.normalize(this.status);

    if (status === '1' || status === 'confirmed' || status === 'paid' || status === 'completed' || status === 'success' || status === 'upcoming') {
      return 'status-success';
    }

    if (status === '2' || status === 'cancelled' || status === 'failed') {
      return 'status-danger';
    }

    return 'status-warning';
  }

  private normalize(value: number | string): string {
    return String(value ?? '').trim().toLowerCase();
  }

  private toLabel(value: string): string {
    if (!value) {
      return 'Unknown';
    }

    return value.charAt(0).toUpperCase() + value.slice(1);
  }
}

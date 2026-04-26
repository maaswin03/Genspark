import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { UserNotificationResponse } from '../../../core/models/user-portal.models';
import { UserPortalService } from '../../../core/services/user-portal.service';

@Component({
  selector: 'app-user-notifications',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.css',
})
export class NotificationsComponent implements OnInit {
  notifications: UserNotificationResponse[] = [];
  isLoading = true;
  isMarkingAll = false;
  errorMessage = '';

  constructor(
    private readonly userPortalService: UserPortalService,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadNotifications();
  }

  get unreadCount(): number {
    return this.notifications.filter((item) => !item.isRead).length;
  }

  markAllRead(): void {
    if (!this.unreadCount || this.isMarkingAll) {
      return;
    }

    this.isMarkingAll = true;
    this.userPortalService
      .markAllNotificationsRead()
      .pipe(finalize(() => (this.isMarkingAll = false)))
      .subscribe({
        next: () => {
          this.notifications = this.notifications.map((item) => ({ ...item, isRead: true }));
        },
        error: () => {
          this.errorMessage = 'Unable to mark all notifications as read.';
        },
      });
  }

  openNotification(item: UserNotificationResponse): void {
    if (!item.isRead) {
      this.userPortalService.markNotificationRead(item.id).subscribe({
        next: () => {
          item.isRead = true;
        },
      });
    }

    const target = this.resolveTarget(item);
    this.router.navigate(target);
  }

  iconFor(item: UserNotificationResponse): string {
    const type = this.normalize(item.type);
    if (type.includes('booking')) {
      return '🎫';
    }

    if (type.includes('payment')) {
      return '💳';
    }

    if (type.includes('trip')) {
      return '🚌';
    }

    return '🔔';
  }

  timeAgo(value: string): string {
    const createdAt = new Date(value).getTime();
    if (!Number.isFinite(createdAt)) {
      return 'Just now';
    }

    const diffMs = Math.max(0, Date.now() - createdAt);
    const minutes = Math.floor(diffMs / 60000);
    if (minutes < 1) {
      return 'Just now';
    }

    if (minutes < 60) {
      return `${minutes} minute${minutes > 1 ? 's' : ''} ago`;
    }

    const hours = Math.floor(minutes / 60);
    if (hours < 24) {
      return `${hours} hour${hours > 1 ? 's' : ''} ago`;
    }

    const days = Math.floor(hours / 24);
    return `${days} day${days > 1 ? 's' : ''} ago`;
  }

  trackByNotification(_: number, item: UserNotificationResponse): number {
    return item.id;
  }

  private loadNotifications(): void {
    this.errorMessage = '';
    this.isLoading = true;
    this.cdr.detectChanges();

    this.userPortalService
      .getNotifications()
      .pipe(finalize(() => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: (rows) => {
          this.notifications = rows;
          this.cdr.detectChanges();
        },
        error: () => {
          this.errorMessage = 'Unable to load notifications right now.';
          this.cdr.detectChanges();
        },
      });
  }

  private resolveTarget(item: UserNotificationResponse): string[] {
    const referenceType = this.normalize(item.referenceType ?? '');
    const type = this.normalize(item.type);

    if ((referenceType.includes('booking') || type.includes('booking')) && item.referenceId) {
      return ['/user/bookings', String(item.referenceId)];
    }

    if (referenceType.includes('trip') && item.referenceId) {
      return ['/trips', String(item.referenceId), 'seats'];
    }

    if ((referenceType.includes('payment') || type.includes('payment')) && item.referenceId) {
      return ['/bookings', String(item.referenceId), 'payment'];
    }

    return ['/user/notifications'];
  }

  private normalize(value: unknown): string {
    return String(value ?? '').trim().toLowerCase();
  }
}

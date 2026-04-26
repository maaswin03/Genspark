import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Component, Inject, PLATFORM_ID } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TokenService } from '../../../core/services/token.service';

type OperatorMenuItem = {
  label: string;
  route: string;
  icon: string;
};

@Component({
  selector: 'app-operator-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './operator-layout.component.html',
  styleUrl: './operator-layout.component.css',
})
export class OperatorLayoutComponent {
  isSidebarOpen = false;
  unreadCount = 0;
  operatorName = 'Operator';

  readonly menuItems: OperatorMenuItem[] = [
    { label: 'Dashboard', route: '/operator/dashboard', icon: '▣' },
    { label: 'My Buses', route: '/operator/buses', icon: '◉' },
    { label: 'Trips & Schedules', route: '/operator/trips', icon: '⇄' },
    { label: 'Bookings', route: '/operator/bookings', icon: '◌' },
    { label: 'Revenue', route: '/operator/revenue', icon: '$' },
    { label: 'Offices', route: '/operator/offices', icon: '⌂' },
    { label: 'Profile', route: '/operator/profile', icon: '%' },
  ];

  constructor(
    private readonly tokenService: TokenService,
    private readonly router: Router,
    @Inject(PLATFORM_ID) private readonly platformId: object
  ) {
    this.operatorName = this.resolveOperatorName();
  }

  toggleSidebar(): void {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  closeSidebar(): void {
    this.isSidebarOpen = false;
  }

  goToNotifications(): void {
    this.closeSidebar();
    this.router.navigateByUrl('/operator/notifications');
  }

  logout(): void {
    this.tokenService.clearAuth();
    this.router.navigateByUrl('/auth/login');
  }

  private resolveOperatorName(): string {
    const profile = this.tokenService.getUserProfile();
    if (profile?.name?.trim()) {
      return profile.name;
    }

    const token = this.tokenService.getToken();
    if (!token) {
      return 'Operator';
    }

    const jwtName = this.getNameFromJwt(token);
    return jwtName?.trim() || 'Operator';
  }

  private getNameFromJwt(token: string): string | null {
    if (!isPlatformBrowser(this.platformId)) {
      return null;
    }

    const parts = token.split('.');
    if (parts.length < 2) {
      return null;
    }

    try {
      const payload = parts[1].replace(/-/g, '+').replace(/_/g, '/');
      const decoded = JSON.parse(atob(payload));

      return (
        decoded?.name ||
        decoded?.unique_name ||
        decoded?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ||
        null
      );
    } catch {
      return null;
    }
  }
}

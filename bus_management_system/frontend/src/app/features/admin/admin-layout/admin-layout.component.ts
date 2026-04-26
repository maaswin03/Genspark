import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Component, Inject, PLATFORM_ID } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TokenService } from '../../../core/services/token.service';

type AdminMenuItem = {
  label: string;
  route: string;
  icon: string;
};

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './admin-layout.component.html',
  styleUrl: './admin-layout.component.css',
})
export class AdminLayoutComponent {
  isSidebarOpen = false;
  unreadCount = 0;
  adminName = 'Admin';

  readonly menuItems: AdminMenuItem[] = [
    { label: 'Dashboard', route: '/admin/dashboard', icon: '▣' },
    { label: 'Operators', route: '/admin/operators', icon: '◉' },
    { label: 'Locations', route: '/admin/locations', icon: '⌖' },
    { label: 'Routes', route: '/admin/routes', icon: '⇄' },
    { label: 'Users', route: '/admin/users', icon: '◌' },
    { label: 'Revenue', route: '/admin/revenue', icon: '$' },
    { label: 'Platform Fee', route: '/admin/platform-fee', icon: '%' },
  ];

  constructor(
    private readonly tokenService: TokenService,
    private readonly router: Router,
    @Inject(PLATFORM_ID) private readonly platformId: object
  ) {
    this.adminName = this.resolveAdminName();
  }

  toggleSidebar(): void {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  closeSidebar(): void {
    this.isSidebarOpen = false;
  }

  openNotifications(): void {
    this.router.navigateByUrl('/admin/dashboard');
  }

  logout(): void {
    this.tokenService.clearAuth();
    this.router.navigateByUrl('/auth/login');
  }

  private resolveAdminName(): string {
    const profile = this.tokenService.getUserProfile();
    if (profile?.name?.trim()) {
      return profile.name;
    }

    const token = this.tokenService.getToken();
    if (!token) {
      return 'Admin';
    }

    const jwtName = this.getNameFromJwt(token);
    return jwtName?.trim() || 'Admin';
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

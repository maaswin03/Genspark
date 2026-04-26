import { Component, HostListener, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { TokenService } from '../../../core/services/token.service';

@Component({
  selector: 'app-user-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './user-layout.html',
  styleUrl: './user-layout.css',
})
export class UserLayout implements OnInit {
  isLoggedIn = false;
  userName = '';
  isMenuCollapsed = true;
  dropdownOpen = false;
  currentYear = new Date().getFullYear();

  constructor(
    private readonly authService: AuthService,
    private readonly tokenService: TokenService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    const role = this.authService.getCurrentRole()?.toLowerCase();
    this.isLoggedIn = this.authService.isAuthenticated() && (role === 'user' || !role);

    if (this.isLoggedIn) {
      const profile = this.tokenService.getUserProfile();
      this.userName = profile?.name || 'User';
    }
  }

  toggleMenu(): void {
    this.isMenuCollapsed = !this.isMenuCollapsed;
  }

  toggleDropdown(): void {
    this.dropdownOpen = !this.dropdownOpen;
  }

  closeDropdown(): void {
    this.dropdownOpen = false;
    this.isMenuCollapsed = true;
  }

  // Close dropdown when clicking outside
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.bm-dropdown')) {
      this.dropdownOpen = false;
    }
  }

  logout(): void {
    this.dropdownOpen = false;
    this.authService.logout();
    this.isLoggedIn = false;
    this.router.navigate(['/home']);
  }
}

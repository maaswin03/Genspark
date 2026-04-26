import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

type UserProfile = {
  name: string;
  email: string;
};

@Injectable({ providedIn: 'root' })
export class TokenService {
  private readonly tokenKey = 'token';
  private readonly roleKey = 'role';
  private readonly userKey = 'user_profile';

  constructor(@Inject(PLATFORM_ID) private readonly platformId: object) {}

  getToken(): string | null {
    return this.getFromStorage(this.tokenKey);
  }

  setToken(token: string): void {
    this.setInStorage(this.tokenKey, token);
  }

  clearToken(): void {
    this.removeFromStorage(this.tokenKey);
  }

  getRole(): string | null {
    return this.getFromStorage(this.roleKey);
  }

  setRole(role: string): void {
    this.setInStorage(this.roleKey, role);
  }

  setUserProfile(profile: UserProfile): void {
    this.setInStorage(this.userKey, JSON.stringify(profile));
  }

  getUserProfile(): UserProfile | null {
    const value = this.getFromStorage(this.userKey);
    if (!value) {
      return null;
    }

    try {
      return JSON.parse(value) as UserProfile;
    } catch {
      return null;
    }
  }

  clearAuth(): void {
    this.removeFromStorage(this.tokenKey);
    this.removeFromStorage(this.roleKey);
    this.removeFromStorage(this.userKey);
  }

  private getFromStorage(key: string): string | null {
    if (!isPlatformBrowser(this.platformId)) {
      return null;
    }

    return localStorage.getItem(key);
  }

  private setInStorage(key: string, value: string): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    localStorage.setItem(key, value);
  }

  private removeFromStorage(key: string): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    localStorage.removeItem(key);
  }
}

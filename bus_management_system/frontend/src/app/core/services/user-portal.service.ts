import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, map, timeout } from 'rxjs';
import {
  ChangeUserPasswordRequest,
  UpdateUserProfileRequest,
  UserNotificationResponse,
  UserProfileResponse,
} from '../models/user-portal.models';

@Injectable({ providedIn: 'root' })
export class UserPortalService {
  private readonly apiBaseUrl = 'http://localhost:5131/api';

  constructor(private readonly http: HttpClient) {}

  getNotifications(): Observable<UserNotificationResponse[]> {
    return this.http.get<unknown[]>(`${this.apiBaseUrl}/notifications`).pipe(
      map((rows) => (Array.isArray(rows) ? rows.map((row) => this.mapNotification(row)) : [])),
      timeout({ first: 8000 })
    );
  }

  markNotificationRead(id: number): Observable<void> {
    return this.http.patch<void>(`${this.apiBaseUrl}/notifications/${id}/read`, {}).pipe(timeout({ first: 8000 }));
  }

  markAllNotificationsRead(): Observable<void> {
    return this.http.patch<void>(`${this.apiBaseUrl}/notifications/read-all`, {}).pipe(timeout({ first: 8000 }));
  }

  getProfile(): Observable<UserProfileResponse> {
    return this.http.get<unknown>(`${this.apiBaseUrl}/user/profile`).pipe(
      map((row) => this.mapProfile(row)),
      timeout({ first: 8000 })
    );
  }

  updateProfile(payload: UpdateUserProfileRequest): Observable<UserProfileResponse> {
    return this.http.put<unknown>(`${this.apiBaseUrl}/user/profile`, payload).pipe(
      map((row) => this.mapProfile(row)),
      timeout({ first: 8000 })
    );
  }

  changePassword(payload: ChangeUserPasswordRequest, profile: UpdateUserProfileRequest): Observable<UserProfileResponse> {
    const requestPayload = {
      ...profile,
      currentPassword: payload.currentPassword,
      newPassword: payload.newPassword,
      confirmPassword: payload.confirmPassword,
    };

    return this.http.put<unknown>(`${this.apiBaseUrl}/user/profile`, requestPayload).pipe(
      map((row) => this.mapProfile(row)),
      timeout({ first: 8000 })
    );
  }

  private mapNotification(source: unknown): UserNotificationResponse {
    const row = this.asRecord(source);
    return {
      id: this.toNumber(this.readValue(row, 'id')),
      title: this.toString(this.readValue(row, 'title')),
      message: this.toString(this.readValue(row, 'message')),
      type: this.toString(this.readValue(row, 'type')),
      referenceType: this.readValue(row, 'referenceType') as never,
      referenceId: this.toNullableNumber(this.readValue(row, 'referenceId')),
      channel: this.readValue(row, 'channel') as never,
      isRead: Boolean(this.readValue(row, 'isRead')),
      createdAt: this.toString(this.readValue(row, 'createdAt')),
    };
  }

  private mapProfile(source: unknown): UserProfileResponse {
    const row = this.asRecord(source);
    return {
      id: this.toNumber(this.readValue(row, 'id')),
      name: this.toString(this.readValue(row, 'name')),
      email: this.toString(this.readValue(row, 'email')),
      phone: this.toString(this.readValue(row, 'phone')),
      role: this.toString(this.readValue(row, 'role')),
    };
  }

  private asRecord(value: unknown): Record<string, unknown> {
    if (typeof value === 'object' && value !== null) {
      return value as Record<string, unknown>;
    }

    return {};
  }

  private readValue(source: Record<string, unknown>, key: string): unknown {
    if (key in source) {
      return source[key];
    }

    const pascal = key.charAt(0).toUpperCase() + key.slice(1);
    if (pascal in source) {
      return source[pascal];
    }

    return undefined;
  }

  private toString(value: unknown): string {
    return value == null ? '' : String(value);
  }

  private toNumber(value: unknown): number {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : 0;
  }

  private toNullableNumber(value: unknown): number | null {
    if (value == null || value === '') {
      return null;
    }

    return this.toNumber(value);
  }
}

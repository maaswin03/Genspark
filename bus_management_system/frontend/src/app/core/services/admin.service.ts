import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, timeout } from 'rxjs';
import {
  AddRouteStopsRequest,
  AdminUserResponse,
  AdminOperatorRevenueResponse,
  AdminRevenueSummaryResponse,
  BusResponse,
  CreateLocationRequest,
  CreateRouteRequest,
  LocationResponse,
  OperatorDetailResponse,
  OperatorProfileResponse,
  PlatformFeeResponse,
  RouteResponse,
  SetPlatformFeeRequest,
} from '../models/admin.models';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly apiBaseUrl = 'http://localhost:5131/api/admin';
  private readonly rootApiBaseUrl = 'http://localhost:5131/api';

  constructor(private readonly http: HttpClient) {}

  getRevenueSummary(): Observable<AdminRevenueSummaryResponse> {
    return this.http.get<AdminRevenueSummaryResponse>(`${this.apiBaseUrl}/revenue`).pipe(timeout({ first: 8000 }));
  }

  getOperators(): Observable<OperatorProfileResponse[]> {
    return this.http.get<OperatorProfileResponse[]>(`${this.apiBaseUrl}/operators`).pipe(timeout({ first: 8000 }));
  }

  getOperatorDetail(operatorId: number): Observable<OperatorDetailResponse> {
    return this.http.get<OperatorDetailResponse>(`${this.apiBaseUrl}/operators/${operatorId}`).pipe(timeout({ first: 8000 }));
  }

  getOperatorRevenue(): Observable<AdminOperatorRevenueResponse[]> {
    return this.http.get<AdminOperatorRevenueResponse[]>(`${this.apiBaseUrl}/revenue/operators`).pipe(timeout({ first: 8000 }));
  }

  approveOperator(operatorId: number, note?: string): Observable<OperatorDetailResponse> {
    return this.http
      .patch<OperatorDetailResponse>(`${this.apiBaseUrl}/operators/${operatorId}/approve`, {
        note: note?.trim() || null,
      })
      .pipe(timeout({ first: 8000 }));
  }

  rejectOperator(operatorId: number, reason: string): Observable<OperatorDetailResponse> {
    return this.http.patch<OperatorDetailResponse>(`${this.apiBaseUrl}/operators/${operatorId}/reject`, { reason }).pipe(timeout({ first: 8000 }));
  }

  blockOperator(operatorId: number, reason: string): Observable<OperatorDetailResponse> {
    return this.http.patch<OperatorDetailResponse>(`${this.apiBaseUrl}/operators/${operatorId}/block`, { reason }).pipe(timeout({ first: 8000 }));
  }

  unblockOperator(operatorId: number): Observable<OperatorDetailResponse> {
    return this.http.patch<OperatorDetailResponse>(`${this.apiBaseUrl}/operators/${operatorId}/unblock`, {}).pipe(timeout({ first: 8000 }));
  }

  verifyOperatorDocument(operatorId: number, documentId: number): Observable<void> {
    return this.http.patch<void>(`${this.apiBaseUrl}/operators/${operatorId}/documents/${documentId}/verify`, {}).pipe(timeout({ first: 8000 }));
  }

  getAllBuses(): Observable<BusResponse[]> {
    return this.http.get<BusResponse[]>(`${this.rootApiBaseUrl}/buses`).pipe(timeout({ first: 8000 }));
  }

  getLocations(): Observable<LocationResponse[]> {
    return this.http.get<LocationResponse[]>(`${this.apiBaseUrl}/locations`).pipe(timeout({ first: 8000 }));
  }

  createLocation(payload: CreateLocationRequest): Observable<LocationResponse> {
    return this.http.post<LocationResponse>(`${this.apiBaseUrl}/locations`, payload).pipe(timeout({ first: 8000 }));
  }

  updateLocation(locationId: number, payload: CreateLocationRequest): Observable<LocationResponse> {
    return this.http.put<LocationResponse>(`${this.apiBaseUrl}/locations/${locationId}`, payload).pipe(timeout({ first: 8000 }));
  }

  getRoutes(): Observable<RouteResponse[]> {
    return this.http.get<RouteResponse[]>(`${this.apiBaseUrl}/routes`).pipe(timeout({ first: 8000 }));
  }

  createRoute(payload: CreateRouteRequest): Observable<RouteResponse> {
    return this.http.post<RouteResponse>(`${this.apiBaseUrl}/routes`, payload).pipe(timeout({ first: 8000 }));
  }

  addRouteStops(routeId: number, payload: AddRouteStopsRequest): Observable<RouteResponse> {
    return this.http.post<RouteResponse>(`${this.apiBaseUrl}/routes/${routeId}/stops`, payload).pipe(timeout({ first: 8000 }));
  }

  toggleRoute(routeId: number): Observable<RouteResponse> {
    return this.http.patch<RouteResponse>(`${this.apiBaseUrl}/routes/${routeId}/toggle`, {}).pipe(timeout({ first: 8000 }));
  }

  getUsers(): Observable<AdminUserResponse[]> {
    return this.http.get<AdminUserResponse[]>(`${this.apiBaseUrl}/users`).pipe(timeout({ first: 8000 }));
  }

  deactivateUser(userId: number): Observable<void> {
    return this.http.patch<void>(`${this.apiBaseUrl}/users/${userId}/deactivate`, {}).pipe(timeout({ first: 8000 }));
  }

  getPlatformFee(): Observable<PlatformFeeResponse> {
    return this.http.get<PlatformFeeResponse>(`${this.apiBaseUrl}/platform-fee`).pipe(timeout({ first: 8000 }));
  }

  setPlatformFee(payload: SetPlatformFeeRequest): Observable<PlatformFeeResponse> {
    return this.http.post<PlatformFeeResponse>(`${this.apiBaseUrl}/platform-fee`, payload).pipe(timeout({ first: 8000 }));
  }
}

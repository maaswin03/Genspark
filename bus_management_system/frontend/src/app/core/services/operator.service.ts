import { HttpClient, HttpContext } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, map, timeout } from 'rxjs';
import {
  BusDetailResponse,
  BusResponse,
  ChangeOperatorPasswordRequest,
  CancelTripRequest,
  ChangeBusRequest,
  CreateOperatorOfficeRequest,
  CreateBusRequest,
  OperatorDocumentResponse,
  OperatorOfficeResponse,
  OperatorProfileResponse,
  OperatorRouteOption,
  OperatorBookingDetailResponse,
  OperatorBookingSummaryResponse,
  OperatorRevenueSummaryResponse,
  OperatorTripScheduleRequest,
  OperatorTripScheduleResponse,
  OperatorTripResponse,
  OperatorTripRevenueResponse,
  SeatPricingResponse,
  SetSeatPricingRequest,
  TripDetailResponse,
  UpdateOperatorOfficeRequest,
  UpdateOperatorProfileRequest,
  UpdateBusRequest,
  UploadOperatorDocumentRequest,
} from '../models/operator.models';
import { SKIP_AUTH_REDIRECT } from '../interceptors/skip-auth-redirect.token';

@Injectable({ providedIn: 'root' })
export class OperatorService {
  private readonly apiBaseUrl = 'http://localhost:5131/api/operator';
  private readonly rootApiBaseUrl = 'http://localhost:5131/api';

  constructor(private readonly http: HttpClient) {}

  getRevenue(): Observable<OperatorRevenueSummaryResponse> {
    return this.http.get<unknown>(`${this.apiBaseUrl}/revenue`).pipe(
      map((response) => this.mapRevenue(response)),
      timeout({ first: 8000 })
    );
  }

  getProfile(): Observable<OperatorProfileResponse> {
    return this.http.get<unknown>(`${this.apiBaseUrl}/profile`).pipe(
      map((response) => this.mapProfile(response)),
      timeout({ first: 8000 })
    );
  }

  updateProfile(payload: UpdateOperatorProfileRequest): Observable<OperatorProfileResponse> {
    return this.http.put<unknown>(`${this.apiBaseUrl}/profile`, payload).pipe(
      map((response) => this.mapProfile(response)),
      timeout({ first: 8000 })
    );
  }

  changePassword(payload: ChangeOperatorPasswordRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiBaseUrl}/change-password`, payload).pipe(timeout({ first: 8000 }));
  }

  getDocuments(): Observable<OperatorDocumentResponse[]> {
    return this.http.get<unknown[]>(`${this.apiBaseUrl}/documents`).pipe(
      map((rows) => (Array.isArray(rows) ? rows.map((row) => this.mapDocument(row)) : [])),
      timeout({ first: 8000 })
    );
  }

  uploadDocument(payload: UploadOperatorDocumentRequest): Observable<OperatorDocumentResponse> {
    return this.http.post<unknown>(`${this.apiBaseUrl}/documents`, payload).pipe(
      map((response) => this.mapDocument(response)),
      timeout({ first: 8000 })
    );
  }

  getOffices(): Observable<OperatorOfficeResponse[]> {
    return this.http.get<unknown[]>(`${this.apiBaseUrl}/offices`).pipe(
      map((rows) => (Array.isArray(rows) ? rows.map((row) => this.mapOffice(row)) : [])),
      timeout({ first: 8000 })
    );
  }

  createOffice(payload: CreateOperatorOfficeRequest): Observable<OperatorOfficeResponse> {
    return this.http.post<unknown>(`${this.apiBaseUrl}/offices`, payload).pipe(
      map((response) => this.mapOffice(response)),
      timeout({ first: 8000 })
    );
  }

  updateOffice(officeId: number, payload: UpdateOperatorOfficeRequest): Observable<OperatorOfficeResponse> {
    return this.http.put<unknown>(`${this.apiBaseUrl}/offices/${officeId}`, payload).pipe(
      map((response) => this.mapOffice(response)),
      timeout({ first: 8000 })
    );
  }

  getTripRevenue(): Observable<OperatorTripRevenueResponse[]> {
    return this.http.get<unknown[]>(`${this.apiBaseUrl}/revenue/trips`).pipe(
      map((rows) => (Array.isArray(rows) ? rows.map((row) => this.mapTripRevenue(row)) : [])),
      timeout({ first: 8000 })
    );
  }

  getTrips(): Observable<OperatorTripResponse[]> {
    return this.http.get<OperatorTripResponse[]>(`${this.rootApiBaseUrl}/operator/trips`).pipe(timeout({ first: 8000 }));
  }

  getSchedules(): Observable<OperatorTripScheduleResponse[]> {
    return this.http.get<OperatorTripScheduleResponse[]>(`${this.rootApiBaseUrl}/operator/trips/schedules`).pipe(timeout({ first: 8000 }));
  }

  createSchedule(payload: OperatorTripScheduleRequest): Observable<OperatorTripScheduleResponse> {
    return this.http.post<OperatorTripScheduleResponse>(`${this.rootApiBaseUrl}/operator/trips/schedules`, payload).pipe(timeout({ first: 8000 }));
  }

  updateSchedule(scheduleId: number, payload: OperatorTripScheduleRequest & { isActive: boolean }): Observable<OperatorTripScheduleResponse> {
    return this.http.put<OperatorTripScheduleResponse>(`${this.rootApiBaseUrl}/operator/trips/schedules/${scheduleId}`, payload).pipe(timeout({ first: 8000 }));
  }

  toggleSchedule(scheduleId: number): Observable<OperatorTripScheduleResponse> {
    return this.http.patch<OperatorTripScheduleResponse>(`${this.rootApiBaseUrl}/operator/trips/schedules/${scheduleId}/toggle`, {}).pipe(timeout({ first: 8000 }));
  }

  changeTripBus(tripId: number, payload: ChangeBusRequest): Observable<TripDetailResponse> {
    return this.http.post<TripDetailResponse>(`${this.rootApiBaseUrl}/operator/trips/${tripId}/change-bus`, payload).pipe(timeout({ first: 8000 }));
  }

  cancelTrip(tripId: number, payload: CancelTripRequest): Observable<TripDetailResponse> {
    return this.http.patch<TripDetailResponse>(`${this.rootApiBaseUrl}/operator/trips/${tripId}/cancel`, payload).pipe(timeout({ first: 8000 }));
  }

  generateTodayTrips(): Observable<string> {
    return this.http.post(`${this.rootApiBaseUrl}/operator/trips/generate-today`, {}, { responseType: 'text' }).pipe(timeout({ first: 30000 }));
  }

  getTripDetail(tripId: number): Observable<TripDetailResponse> {
    return this.http.get<TripDetailResponse>(`${this.rootApiBaseUrl}/trips/${tripId}`).pipe(timeout({ first: 8000 }));
  }

  getTripPricing(tripId: number): Observable<SeatPricingResponse[]> {
    return this.http.get<SeatPricingResponse[]>(`${this.rootApiBaseUrl}/operator/trips/${tripId}/pricing`).pipe(timeout({ first: 8000 }));
  }

  setTripPricing(tripId: number, payload: SetSeatPricingRequest): Observable<SeatPricingResponse[]> {
    return this.http.post<SeatPricingResponse[]>(`${this.rootApiBaseUrl}/operator/trips/${tripId}/pricing`, payload).pipe(timeout({ first: 8000 }));
  }

  getRouteOptions(): Observable<OperatorRouteOption[]> {
    return this.http
      .get<OperatorRouteOption[]>(`${this.apiBaseUrl}/routes`, {
        context: new HttpContext().set(SKIP_AUTH_REDIRECT, true),
      })
      .pipe(timeout({ first: 8000 }));
  }

  getBookings(): Observable<OperatorBookingSummaryResponse[]> {
    return this.http.get<unknown[]>(`${this.apiBaseUrl}/bookings`).pipe(
      map((rows) => (Array.isArray(rows) ? rows.map((row) => this.mapBookingSummary(row)) : [])),
      timeout({ first: 8000 })
    );
  }

  getBookingDetail(bookingId: number): Observable<OperatorBookingDetailResponse> {
    return this.http.get<unknown>(`${this.apiBaseUrl}/bookings/${bookingId}`).pipe(
      map((response) => this.mapBookingDetail(response)),
      timeout({ first: 8000 })
    );
  }

  getBuses(): Observable<BusResponse[]> {
    return this.http.get<BusResponse[]>(`${this.rootApiBaseUrl}/operator/buses`).pipe(timeout({ first: 8000 }));
  }

  getBusDetail(busId: number): Observable<BusDetailResponse> {
    return this.http.get<BusDetailResponse>(`${this.rootApiBaseUrl}/buses/${busId}`).pipe(timeout({ first: 8000 }));
  }

  getBusSeats(busId: number): Observable<BusDetailResponse> {
    return this.http.get<BusDetailResponse>(`${this.rootApiBaseUrl}/buses/${busId}`).pipe(timeout({ first: 8000 }));
  }

  createBus(payload: CreateBusRequest): Observable<BusDetailResponse> {
    return this.http.post<BusDetailResponse>(`${this.rootApiBaseUrl}/operator/buses`, payload).pipe(timeout({ first: 8000 }));
  }

  updateBus(busId: number, payload: UpdateBusRequest): Observable<BusDetailResponse> {
    return this.http.put<BusDetailResponse>(`${this.rootApiBaseUrl}/operator/buses/${busId}`, payload).pipe(timeout({ first: 8000 }));
  }

  deactivateBus(busId: number): Observable<void> {
    return this.http.patch<void>(`${this.rootApiBaseUrl}/operator/buses/${busId}/deactivate`, {}).pipe(timeout({ first: 8000 }));
  }

  private mapRevenue(source: unknown): OperatorRevenueSummaryResponse {
    const row = this.asRecord(source);
    return {
      operatorId: this.toNumber(this.readValue(row, 'operatorId')),
      companyName: this.toString(this.readValue(row, 'companyName')),
      totalRevenue: this.toNumber(this.readValue(row, 'totalRevenue')),
      totalPlatformFee: this.toNumber(this.readValue(row, 'totalPlatformFee')),
      totalOperatorEarning: this.toNumber(this.readValue(row, 'totalOperatorEarning')),
      totalTransactions: this.toNumber(this.readValue(row, 'totalTransactions')),
    };
  }

  private mapTripRevenue(source: unknown): OperatorTripRevenueResponse {
    const row = this.asRecord(source);
    return {
      tripId: this.toNumber(this.readValue(row, 'tripId')),
      departureTime: this.toString(this.readValue(row, 'departureTime')),
      arrivalTime: this.toString(this.readValue(row, 'arrivalTime')),
      routeName: this.toString(this.readValue(row, 'routeName')),
      totalRevenue: this.toNumber(this.readValue(row, 'totalRevenue')),
      totalPlatformFee: this.toNumber(this.readValue(row, 'totalPlatformFee')),
      operatorEarning: this.toNumber(this.readValue(row, 'operatorEarning')),
      totalBookings: this.toNumber(this.readValue(row, 'totalBookings')),
    };
  }

  private mapProfile(source: unknown): OperatorProfileResponse {
    const row = this.asRecord(source);
    return {
      id: this.toNumber(this.readValue(row, 'id')),
      userId: this.toNumber(this.readValue(row, 'userId')),
      name: this.toString(this.readValue(row, 'name')),
      email: this.toString(this.readValue(row, 'email')),
      phone: this.toString(this.readValue(row, 'phone')),
      companyName: this.toString(this.readValue(row, 'companyName')),
      licenseNumber: this.toString(this.readValue(row, 'licenseNumber')),
      approvalStatus: this.toStringOrNumber(this.readValue(row, 'approvalStatus')),
      createdAt: this.toString(this.readValue(row, 'createdAt')),
    };
  }

  private mapDocument(source: unknown): OperatorDocumentResponse {
    const row = this.asRecord(source);
    return {
      id: this.toNumber(this.readValue(row, 'id')),
      documentType: this.toStringOrNumber(this.readValue(row, 'documentType')),
      fileUrl: this.toString(this.readValue(row, 'fileUrl')),
      uploadedAt: this.toString(this.readValue(row, 'uploadedAt')),
      verifiedBy: this.toNullableNumber(this.readValue(row, 'verifiedBy')),
      verifiedAt: this.toNullableString(this.readValue(row, 'verifiedAt')),
    };
  }

  private mapOffice(source: unknown): OperatorOfficeResponse {
    const row = this.asRecord(source);
    return {
      id: this.toNumber(this.readValue(row, 'id')),
      locationId: this.toNumber(this.readValue(row, 'locationId')),
      locationName: this.toString(this.readValue(row, 'locationName')),
      address: this.toString(this.readValue(row, 'address')),
      isHeadOffice: this.toBoolean(this.readValue(row, 'isHeadOffice')),
    };
  }

  private mapBookingSummary(source: unknown): OperatorBookingSummaryResponse {
    const row = this.asRecord(source);
    return {
      bookingId: this.toNumber(this.readValue(row, 'bookingId')),
      tripId: this.toNumber(this.readValue(row, 'tripId')),
      userName: this.toString(this.readValue(row, 'userName')),
      userEmail: this.toString(this.readValue(row, 'userEmail')),
      routeName: this.toString(this.readValue(row, 'routeName')),
      departureTime: this.toString(this.readValue(row, 'departureTime')),
      totalAmount: this.toNumber(this.readValue(row, 'totalAmount')),
      bookingStatus: this.toStringOrNumber(this.readValue(row, 'bookingStatus')),
      seatCount: this.toNumber(this.readValue(row, 'seatCount')),
    };
  }

  private mapBookingDetail(source: unknown): OperatorBookingDetailResponse {
    const row = this.asRecord(source);
    const rawSeats = this.readValue(row, 'seats');
    const seats = Array.isArray(rawSeats)
      ? rawSeats.map((entry) => {
          const item = this.asRecord(entry);
          return {
            bookingSeatId: this.toNumber(this.readValue(item, 'bookingSeatId')),
            seatId: this.toNumber(this.readValue(item, 'seatId')),
            seatNumber: this.toString(this.readValue(item, 'seatNumber')),
            amountPaid: this.toNumber(this.readValue(item, 'amountPaid')),
            status: this.toStringOrNumber(this.readValue(item, 'status')),
            passengerName: this.toNullableString(this.readValue(item, 'passengerName')),
            passengerAge: this.toNullableNumber(this.readValue(item, 'passengerAge')),
            passengerGender: this.toNullableStringOrNumber(this.readValue(item, 'passengerGender')),
          };
        })
      : [];

    return {
      bookingId: this.toNumber(this.readValue(row, 'bookingId')),
      tripId: this.toNumber(this.readValue(row, 'tripId')),
      userName: this.toString(this.readValue(row, 'userName')),
      userEmail: this.toString(this.readValue(row, 'userEmail')),
      userPhone: this.toString(this.readValue(row, 'userPhone')),
      routeName: this.toString(this.readValue(row, 'routeName')),
      bookingDate: this.toString(this.readValue(row, 'bookingDate')),
      departureTime: this.toString(this.readValue(row, 'departureTime')),
      arrivalTime: this.toString(this.readValue(row, 'arrivalTime')),
      totalAmount: this.toNumber(this.readValue(row, 'totalAmount')),
      platformFee: this.toNumber(this.readValue(row, 'platformFee')),
      bookingStatus: this.toStringOrNumber(this.readValue(row, 'bookingStatus')),
      cancelledAt: this.toNullableString(this.readValue(row, 'cancelledAt')),
      cancelReason: this.toNullableString(this.readValue(row, 'cancelReason')),
      seats,
    };
  }

  private readValue(source: Record<string, unknown>, camelCaseKey: string): unknown {
    const pascalCaseKey = camelCaseKey.charAt(0).toUpperCase() + camelCaseKey.slice(1);
    return source[camelCaseKey] ?? source[pascalCaseKey];
  }

  private asRecord(source: unknown): Record<string, unknown> {
    return typeof source === 'object' && source !== null ? (source as Record<string, unknown>) : {};
  }

  private toString(value: unknown): string {
    return typeof value === 'string' ? value : value == null ? '' : String(value);
  }

  private toNullableString(value: unknown): string | null {
    return value == null ? null : this.toString(value);
  }

  private toNumber(value: unknown): number {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : 0;
  }

  private toNullableNumber(value: unknown): number | null {
    if (value == null) {
      return null;
    }

    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : null;
  }

  private toStringOrNumber(value: unknown): string | number {
    if (typeof value === 'number' || typeof value === 'string') {
      return value;
    }

    return this.toString(value);
  }

  private toNullableStringOrNumber(value: unknown): string | number | null {
    if (value == null) {
      return null;
    }

    return this.toStringOrNumber(value);
  }

  private toBoolean(value: unknown): boolean {
    if (typeof value === 'boolean') {
      return value;
    }

    if (typeof value === 'string') {
      return value.toLowerCase() === 'true';
    }

    return Boolean(value);
  }
}

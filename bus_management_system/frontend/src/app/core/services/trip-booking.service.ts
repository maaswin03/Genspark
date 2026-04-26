import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, map, timeout } from 'rxjs';
import {
  BookingResponse,
  CancelBookingRequest,
  CancelSeatResponse,
  CreateBookingRequest,
  InitiatePaymentRequest,
  InitiatePaymentResponse,
  PaymentStatusResponse,
  ReserveSeatRequest,
  ReserveSeatResponse,
  TripDetailResponse,
  TripPointResponse,
  TripPointsResponse,
  TripSearchRequest,
  TripSearchResponse,
  TripSeatResponse,
  UserBookingDetailResponse,
  UserBookingResponse,
} from '../models/trip-booking.models';

@Injectable({ providedIn: 'root' })
export class TripBookingService {
  private readonly apiBaseUrl = 'http://localhost:5131/api';

  constructor(private readonly http: HttpClient) { }

  searchTrips(request: TripSearchRequest): Observable<TripSearchResponse[]> {
    const params = this.toParams({
      SourceId: request.sourceId,
      DestinationId: request.destinationId,
      TravelDate: request.travelDate,
      BusType: request.busType ?? undefined,
    });

    return this.http.get<unknown[]>(`${this.apiBaseUrl}/trips/search`, { params }).pipe(
      map((rows) => (Array.isArray(rows) ? rows.map((row) => this.mapTripSearch(row)) : [])),
      timeout({ first: 8000 })
    );
  }

  getTripDetails(tripId: number): Observable<TripDetailResponse> {
    return this.http.get<unknown>(`${this.apiBaseUrl}/trips/${tripId}`).pipe(
      map((response) => this.mapTripDetail(response)),
      timeout({ first: 8000 })
    );
  }

  getTripPoints(tripId: number): Observable<TripPointsResponse> {
    return this.http.get<unknown>(`${this.apiBaseUrl}/trips/${tripId}/points`).pipe(
      map((response) => this.mapTripPoints(response)),
      timeout({ first: 8000 })
    );
  }

  reserveSeats(payload: ReserveSeatRequest): Observable<ReserveSeatResponse> {
    return this.http.post<unknown>(`${this.apiBaseUrl}/bookings/reserve`, payload).pipe(
      map((response) => this.mapReserveSeat(response)),
      timeout({ first: 8000 })
    );
  }

  confirmPayment(request: {
    paymentId: number;
    status: string;
    gatewayTransactionId: string;
  }): Observable<void> {
    return this.http.post<void>(`${this.apiBaseUrl}/payments/webhook`, {
      paymentId: request.paymentId,
      status: request.status,
      gatewayTransactionId: request.gatewayTransactionId,
      signature: 'mock-signature'
    });
  }

  createBooking(payload: CreateBookingRequest): Observable<BookingResponse> {
    return this.http.post<unknown>(`${this.apiBaseUrl}/bookings`, payload).pipe(
      map((response) => this.mapBooking(response)),
      timeout({ first: 8000 })
    );
  }

  initiatePayment(payload: InitiatePaymentRequest): Observable<InitiatePaymentResponse> {
    return this.http.post<unknown>(`${this.apiBaseUrl}/payments/initiate`, payload).pipe(
      map((response) => this.mapInitiatePayment(response)),
      timeout({ first: 8000 })
    );
  }

  getPaymentStatus(bookingId: number): Observable<PaymentStatusResponse> {
    return this.http.get<unknown>(`${this.apiBaseUrl}/payments/${bookingId}`).pipe(
      map((response) => this.mapPaymentStatus(response)),
      timeout({ first: 8000 })
    );
  }

  getUserBookings(status?: string): Observable<UserBookingResponse[]> {
    const params = status ? new HttpParams().set('status', status) : undefined;
    return this.http.get<unknown[]>(`${this.apiBaseUrl}/user/bookings`, { params }).pipe(
      map((rows) => (Array.isArray(rows) ? rows.map((row) => this.mapUserBooking(row)) : [])),
      timeout({ first: 8000 })
    );
  }

  getUserBookingDetail(bookingId: number): Observable<UserBookingDetailResponse> {
    return this.http.get<unknown>(`${this.apiBaseUrl}/user/bookings/${bookingId}`).pipe(
      map((response) => this.mapUserBookingDetail(response)),
      timeout({ first: 8000 })
    );
  }

  cancelBooking(bookingId: number, payload: CancelBookingRequest): Observable<BookingResponse> {
    return this.http.patch<unknown>(`${this.apiBaseUrl}/bookings/${bookingId}/cancel`, payload).pipe(
      map((response) => this.mapBooking(response)),
      timeout({ first: 8000 })
    );
  }

  cancelSeat(bookingId: number, bookingSeatId: number, payload: CancelBookingRequest): Observable<CancelSeatResponse> {
    return this.http.patch<unknown>(`${this.apiBaseUrl}/bookings/${bookingId}/seats/${bookingSeatId}/cancel`, payload).pipe(
      map((response) => this.mapCancelSeat(response)),
      timeout({ first: 8000 })
    );
  }

  private mapTripSearch(source: unknown): TripSearchResponse {
    const row = this.asRecord(source);
    return {
      tripId: this.toNumber(this.readValue(row, 'tripId')),
      operatorName: this.toString(this.readValue(row, 'operatorName')),
      busNumber: this.toString(this.readValue(row, 'busNumber')),
      busType: this.readValue(row, 'busType') as never,
      routeName: this.toString(this.readValue(row, 'routeName')),
      departureTime: this.toString(this.readValue(row, 'departureTime')),
      arrivalTime: this.toString(this.readValue(row, 'arrivalTime')),
      baseFare: this.toNumber(this.readValue(row, 'baseFare')),
      availableSeats: this.toNumber(this.readValue(row, 'availableSeats')),
      status: this.readValue(row, 'status') as never,
    };
  }

  private mapTripDetail(source: unknown): TripDetailResponse {
    const row = this.asRecord(source);
    return {
      ...this.mapTripSearch(row),
      routeId: this.toNumber(this.readValue(row, 'routeId')),
      sourceName: this.toString(this.readValue(row, 'sourceName')),
      destinationName: this.toString(this.readValue(row, 'destinationName')),
      seats: this.toArray(this.readValue(row, 'seats')).map((seat) => this.mapTripSeat(seat)),
    };
  }

  private mapTripSeat(source: unknown): TripSeatResponse {
    const row = this.asRecord(source);
    return {
      tripSeatId: this.toNumber(this.readValue(row, 'tripSeatId')),
      seatId: this.toNumber(this.readValue(row, 'seatId')),
      seatNumber: this.toString(this.readValue(row, 'seatNumber')),
      row: this.toNumber(this.readValue(row, 'row')),
      columnNumber: this.toNumber(this.readValue(row, 'columnNumber')),
      deck: this.readValue(row, 'deck') as never,
      seatType: this.readValue(row, 'seatType') as never,
      status: this.readValue(row, 'status') as never,
      lockedUntil: this.readOptionalString(this.readValue(row, 'lockedUntil')),
      price: this.toNumber(this.readValue(row, 'price')),
    };
  }

  private mapTripPoints(source: unknown): TripPointsResponse {
    const row = this.asRecord(source);
    return {
      boardingPoints: this.toArray(this.readValue(row, 'boardingPoints')).map((point) => this.mapTripPoint(point)),
      droppingPoints: this.toArray(this.readValue(row, 'droppingPoints')).map((point) => this.mapTripPoint(point)),
    };
  }

  private mapTripPoint(source: unknown): TripPointResponse {
    const row = this.asRecord(source);
    return {
      id: this.toNumber(this.readValue(row, 'id')),
      locationId: this.toNumber(this.readValue(row, 'locationId')),
      locationName: this.toString(this.readValue(row, 'locationName')),
      name: this.toString(this.readValue(row, 'name')),
      timeOffset: this.toNumber(this.readValue(row, 'timeOffset')),
      isDefault: this.toBoolean(this.readValue(row, 'isDefault')),
    };
  }

  private mapReserveSeat(source: unknown): ReserveSeatResponse {
    const row = this.asRecord(source);
    return {
      tripId: this.toNumber(this.readValue(row, 'tripId')),
      lockedUntil: this.toString(this.readValue(row, 'lockedUntil')),
      reservedTripSeatIds: this.toArray(this.readValue(row, 'reservedTripSeatIds')).map((item) => this.toNumber(item)),
    };
  }

  private mapBooking(source: unknown): BookingResponse {
    const row = this.asRecord(source);
    return {
      bookingId: this.toNumber(this.readValue(row, 'bookingId')),
      tripDetails: this.toString(this.readValue(row, 'tripDetails')),
      totalAmount: this.toNumber(this.readValue(row, 'totalAmount')),
      status: this.readValue(row, 'status') as never,
      bookingDate: this.toString(this.readValue(row, 'bookingDate')),
    };
  }

  private mapInitiatePayment(source: unknown): InitiatePaymentResponse {
    const row = this.asRecord(source);
    return {
      paymentId: this.toNumber(this.readValue(row, 'paymentId')),
      bookingId: this.toNumber(this.readValue(row, 'bookingId')),
      paymentStatus: this.readValue(row, 'paymentStatus') as never,
      gatewayPayload: this.asRecord(this.readValue(row, 'gatewayPayload')),
    };
  }

  private mapPaymentStatus(source: unknown): PaymentStatusResponse {
    const row = this.asRecord(source);
    return {
      bookingId: this.toNumber(this.readValue(row, 'bookingId')),
      paymentId: this.readNullableNumber(this.readValue(row, 'paymentId')),
      paymentStatus: this.readValue(row, 'paymentStatus') as never,
      bookingStatus: this.readValue(row, 'bookingStatus') as never,
      amount: this.toNumber(this.readValue(row, 'amount')),
      transactionId: this.readOptionalString(this.readValue(row, 'transactionId')),
      updatedAt: this.toString(this.readValue(row, 'updatedAt')),
    };
  }

  private mapUserBooking(source: unknown): UserBookingResponse {
    const row = this.asRecord(source);
    return {
      bookingId: this.toNumber(this.readValue(row, 'bookingId')),
      tripId: this.toNumber(this.readValue(row, 'tripId')),
      routeName: this.toString(this.readValue(row, 'routeName')),
      departureTime: this.toString(this.readValue(row, 'departureTime')),
      arrivalTime: this.toString(this.readValue(row, 'arrivalTime')),
      totalAmount: this.toNumber(this.readValue(row, 'totalAmount')),
      status: this.readValue(row, 'status') as never,
      bookingDate: this.toString(this.readValue(row, 'bookingDate')),
      seatCount: this.toNumber(this.readValue(row, 'seatCount')),
    };
  }

  private mapUserBookingDetail(source: unknown): UserBookingDetailResponse {
    const row = this.asRecord(source);
    return {
      bookingId: this.toNumber(this.readValue(row, 'bookingId')),
      tripId: this.toNumber(this.readValue(row, 'tripId')),
      routeName: this.toString(this.readValue(row, 'routeName')),
      departureTime: this.toString(this.readValue(row, 'departureTime')),
      arrivalTime: this.toString(this.readValue(row, 'arrivalTime')),
      totalAmount: this.toNumber(this.readValue(row, 'totalAmount')),
      platformFee: this.toNumber(this.readValue(row, 'platformFee')),
      status: this.readValue(row, 'status') as never,
      bookingDate: this.toString(this.readValue(row, 'bookingDate')),
      cancelledAt: this.readOptionalString(this.readValue(row, 'cancelledAt')),
      cancelReason: this.readOptionalString(this.readValue(row, 'cancelReason')),
      paymentStatus: this.readValue(row, 'paymentStatus') as never,
      seats: this.toArray(this.readValue(row, 'seats')).map((seat) => this.mapUserBookingSeat(seat)),
    };
  }

  private mapUserBookingSeat(source: unknown): UserBookingDetailResponse['seats'][number] {
    const row = this.asRecord(source);
    return {
      bookingSeatId: this.toNumber(this.readValue(row, 'bookingSeatId')),
      seatId: this.toNumber(this.readValue(row, 'seatId')),
      seatNumber: this.toString(this.readValue(row, 'seatNumber')),
      amountPaid: this.toNumber(this.readValue(row, 'amountPaid')),
      status: this.readValue(row, 'status') as never,
      passengerName: this.readOptionalString(this.readValue(row, 'passengerName')),
      passengerAge: this.readNullableNumber(this.readValue(row, 'passengerAge')),
      passengerGender: this.readValue(row, 'passengerGender') as never,
    };
  }

  private mapCancelSeat(source: unknown): CancelSeatResponse {
    const row = this.asRecord(source);
    return {
      bookingId: this.toNumber(this.readValue(row, 'bookingId')),
      bookingSeatId: this.toNumber(this.readValue(row, 'bookingSeatId')),
      bookingStatus: this.readValue(row, 'bookingStatus') as never,
      updatedTotalAmount: this.toNumber(this.readValue(row, 'updatedTotalAmount')),
    };
  }

  private readValue(row: Record<string, unknown>, key: string): unknown {
    const camel = key.charAt(0).toLowerCase() + key.slice(1);
    const pascal = key.charAt(0).toUpperCase() + key.slice(1);
    return row[key] ?? row[camel] ?? row[pascal];
  }

  private asRecord(source: unknown): Record<string, unknown> {
    return source && typeof source === 'object' ? (source as Record<string, unknown>) : {};
  }

  private toArray(value: unknown): unknown[] {
    return Array.isArray(value) ? value : [];
  }

  private toString(value: unknown): string {
    return value == null ? '' : String(value);
  }

  private readOptionalString(value: unknown): string | null {
    if (value == null || value === '') {
      return null;
    }

    return String(value);
  }

  private toNumber(value: unknown): number {
    if (typeof value === 'number') {
      return value;
    }

    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : 0;
  }

  private readNullableNumber(value: unknown): number | null {
    if (value == null || value === '') {
      return null;
    }

    return this.toNumber(value);
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

  private toParams(values: Record<string, unknown>): HttpParams {
    let params = new HttpParams();

    Object.entries(values).forEach(([key, value]) => {
      if (value == null || value === '') {
        return;
      }

      params = params.set(key, String(value));
    });

    return params;
  }
}

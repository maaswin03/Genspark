import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import {
  BookingFlowReservation,
  BookingFlowState,
  TripDetailResponse,
  TripPointsResponse,
} from '../models/trip-booking.models';

@Injectable({ providedIn: 'root' })
export class BookingFlowStateService {
  private readonly storageKey = 'booking_flow_state';

  constructor(@Inject(PLATFORM_ID) private readonly platformId: object) {}

  getState(): BookingFlowState {
    if (!isPlatformBrowser(this.platformId)) {
      return this.createEmptyState();
    }

    const raw = localStorage.getItem(this.storageKey);
    if (!raw) {
      return this.createEmptyState();
    }

    try {
      const parsed = JSON.parse(raw) as Partial<BookingFlowState>;
      return {
        tripId: parsed.tripId ?? null,
        trip: parsed.trip ?? null,
        tripPoints: parsed.tripPoints ?? null,
        selectedTripSeatIds: Array.isArray(parsed.selectedTripSeatIds) ? parsed.selectedTripSeatIds : [],
        boardingPointId: parsed.boardingPointId ?? null,
        droppingPointId: parsed.droppingPointId ?? null,
        reservation: parsed.reservation ?? null,
      };
    } catch {
      return this.createEmptyState();
    }
  }

  setTrip(tripId: number, trip: TripDetailResponse): void {
    const state = this.getState();
    this.persist({ ...state, tripId, trip });
  }

  setTripPoints(tripPoints: TripPointsResponse): void {
    const state = this.getState();
    this.persist({ ...state, tripPoints });
  }

  setSelection(selectedTripSeatIds: number[]): void {
    const state = this.getState();
    this.persist({ ...state, selectedTripSeatIds });
  }

  setBoardingPoint(boardingPointId: number): void {
    const state = this.getState();
    this.persist({ ...state, boardingPointId });
  }

  setDroppingPoint(droppingPointId: number): void {
    const state = this.getState();
    this.persist({ ...state, droppingPointId });
  }

  setReservation(reservation: BookingFlowReservation): void {
    const state = this.getState();
    this.persist({ ...state, reservation });
  }

  clear(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    localStorage.removeItem(this.storageKey);
  }

  private persist(state: BookingFlowState): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    localStorage.setItem(this.storageKey, JSON.stringify(state));
  }

  private createEmptyState(): BookingFlowState {
    return {
      tripId: null,
      trip: null,
      tripPoints: null,
      selectedTripSeatIds: [],
      boardingPointId: null,
      droppingPointId: null,
      reservation: null,
    };
  }
}

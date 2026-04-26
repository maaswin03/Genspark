import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Subject, finalize, takeUntil } from 'rxjs';
import { BookingFlowStateService } from '../../../core/services/booking-flow-state.service';
import { TripBookingService } from '../../../core/services/trip-booking.service';
import { TripDetailResponse, TripPointResponse, TripSeatResponse, BookingFlowReservation } from '../../../core/models/trip-booking.models';
import { SeatMapComponent } from '../../../shared/components/seat-map/seat-map.component';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-seat-selection',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, SeatMapComponent],
  templateUrl: './seat-selection.component.html',
  styleUrl: './seat-selection.component.css',
})
export class SeatSelectionComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  private timerHandle?: number;

  tripId = 0;
  trip: TripDetailResponse | null = null;
  boardingPoints: TripPointResponse[] = [];
  droppingPoints: TripPointResponse[] = [];
  activeDeck: 'lower' | 'upper' = 'lower';
  selectedSeatIds: number[] = [];
  selectedTripSeatIds: number[] = [];
  selectedBoardingPointId = 0;
  selectedDroppingPointId = 0;
  reservation: BookingFlowReservation | null = null;
  isLoading = false;
  errorMessage = '';
  infoMessage = '';
  now = Date.now();

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly tripBookingService: TripBookingService,
    private readonly bookingFlowState: BookingFlowStateService,
    private readonly cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe((params) => {
      this.tripId = Number(params.get('id') ?? 0);
      this.loadTripContext();
    });
    this.startClock();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.timerHandle) window.clearInterval(this.timerHandle);
  }

  selectSeat(seat: TripSeatResponse): void {
    const exists = this.selectedSeatIds.includes(seat.seatId);
    this.selectedSeatIds = exists
      ? this.selectedSeatIds.filter((id) => id !== seat.seatId)
      : [...this.selectedSeatIds, seat.seatId];
    this.errorMessage = '';
    this.cdr.detectChanges();
  }

  changeDeck(deck: 'lower' | 'upper'): void {
    this.activeDeck = deck;
    this.cdr.detectChanges();
  }

  reserveSeats(): void {
    if (!this.trip || this.selectedSeatIds.length === 0) {
      this.errorMessage = 'Select at least one available seat to reserve.';
      this.cdr.detectChanges();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.infoMessage = '';
    this.cdr.detectChanges();

    this.tripBookingService
      .reserveSeats({ tripId: this.tripId, seatIds: this.selectedSeatIds })
      .pipe(
        finalize(() => { this.isLoading = false; this.cdr.detectChanges(); }),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (response) => {
          this.reservation = response;
          this.selectedTripSeatIds = response.reservedTripSeatIds;
          this.bookingFlowState.setReservation(response);
          this.bookingFlowState.setSelection(response.reservedTripSeatIds);
          this.bookingFlowState.setTrip(this.tripId, this.trip!);
          this.infoMessage = 'Seats reserved! Complete your booking before the timer runs out.';
          this.startClock();
          this.cdr.detectChanges();
        },
        error: () => {
          this.errorMessage = 'Unable to reserve seats right now. Please try again.';
          this.cdr.detectChanges();
        },
      });
  }

  continueToPassengerDetails(): void {
    this.errorMessage = '';

    if (!this.isReservationActive()) {
      this.errorMessage = 'Your seat reservation has expired. Please reserve the seats again.';
      this.cdr.detectChanges();
      return;
    }

    if (!this.selectedBoardingPointId) {
      this.errorMessage = 'No boarding point available for this trip.';
      this.cdr.detectChanges();
      return;
    }

    if (!this.selectedDroppingPointId) {
      this.errorMessage = 'No dropping point available for this trip.';
      this.cdr.detectChanges();
      return;
    }

    this.bookingFlowState.setBoardingPoint(this.selectedBoardingPointId);
    this.bookingFlowState.setDroppingPoint(this.selectedDroppingPointId);
    this.router.navigate(['/bookings/passengers']);
  }

  get availableSeatCount(): number {
    return this.trip?.seats.filter((s) => this.normalize(s.status) === 'available').length ?? 0;
  }

  get selectedSeatTotal(): number {
    if (!this.trip) return 0;
    return this.selectedSeatIds.reduce(
      (total, seatId) => total + (this.trip?.seats.find((s) => s.seatId === seatId)?.price ?? this.trip?.baseFare ?? 0),
      0
    );
  }

  get timerLabel(): string {
    if (!this.reservation?.lockedUntil) return '05:00';
    const remaining = Math.max(0, new Date(this.reservation.lockedUntil).getTime() - this.now);
    const minutes = Math.floor(remaining / 60000);
    const seconds = Math.floor((remaining % 60000) / 1000);
    return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
  }

  get timerIsLow(): boolean {
    return !!this.reservation?.lockedUntil &&
      new Date(this.reservation.lockedUntil).getTime() - this.now < 60000;
  }

  get canProceed(): boolean {
    return this.isReservationActive() &&
      this.selectedBoardingPointId > 0 &&
      this.selectedDroppingPointId > 0 &&
      this.selectedTripSeatIds.length > 0;
  }

  get selectedSeatNumbers(): string[] {
    if (!this.trip) return [];
    const ids = this.selectedTripSeatIds.length > 0
      ? this.selectedTripSeatIds
      : this.selectedSeatIds.map((seatId) => this.trip?.seats.find((s) => s.seatId === seatId)?.tripSeatId ?? 0);
    return this.trip.seats.filter((s) => ids.includes(s.tripSeatId)).map((s) => s.seatNumber);
  }

  get boardingPoint(): TripPointResponse | null {
    return this.boardingPoints.find(p => p.id === this.selectedBoardingPointId) ?? null;
  }

  get droppingPoint(): TripPointResponse | null {
    return this.droppingPoints.find(p => p.id === this.selectedDroppingPointId) ?? null;
  }

  /** Auto-selects when there's exactly one option (office per location) */
  get hasMultipleBoardingPoints(): boolean {
    return this.boardingPoints.length > 1;
  }

  get hasMultipleDroppingPoints(): boolean {
    return this.droppingPoints.length > 1;
  }

  isReservationActive(): boolean {
    if (!this.reservation?.lockedUntil) return false;
    return new Date(this.reservation.lockedUntil).getTime() > Date.now();
  }

  get isSleeper(): boolean {
    if (!this.trip) return false;
    const bt = String(this.trip.busType ?? '').replace(/_/g, '').toLowerCase();
    return bt.includes('sleeper');
  }

  formatTime(value: string): string {
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? value : date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  private autoSelectPoints(): void {
    // Always pick default or first — regardless of how many points exist
    if (!this.selectedBoardingPointId && this.boardingPoints.length > 0) {
      const def = this.boardingPoints.find(p => p.isDefault) ?? this.boardingPoints[0];
      this.selectedBoardingPointId = def.id;
      this.bookingFlowState.setBoardingPoint(this.selectedBoardingPointId);
    }

    if (!this.selectedDroppingPointId && this.droppingPoints.length > 0) {
      const def = this.droppingPoints.find(p => p.isDefault) ?? this.droppingPoints[0];
      this.selectedDroppingPointId = def.id;
      this.bookingFlowState.setDroppingPoint(this.selectedDroppingPointId);
    }

    this.cdr.detectChanges();
  }

  private loadTripContext(): void {
    if (!this.tripId) return;

    const state = this.bookingFlowState.getState();

    // Clear stale state when switching to a different trip
    if (state.tripId !== null && state.tripId !== this.tripId) {
      this.bookingFlowState.clear();
      this.reservation = null;
      this.selectedTripSeatIds = [];
      this.selectedSeatIds = [];
      this.selectedBoardingPointId = 0;
      this.selectedDroppingPointId = 0;
      this.infoMessage = '';
      this.errorMessage = '';
    }

    const freshState = this.bookingFlowState.getState();
    if (freshState.tripId === this.tripId && freshState.trip && freshState.trip.seats.length > 0) {
      this.trip = freshState.trip;
      this.boardingPoints = freshState.tripPoints?.boardingPoints ?? [];
      this.droppingPoints = freshState.tripPoints?.droppingPoints ?? [];
      this.selectedTripSeatIds = freshState.selectedTripSeatIds;
      this.selectedBoardingPointId = freshState.boardingPointId ?? 0;
      this.selectedDroppingPointId = freshState.droppingPointId ?? 0;
      this.reservation = freshState.reservation;
      this.selectedSeatIds = this.trip.seats
        .filter((s) => this.selectedTripSeatIds.includes(s.tripSeatId))
        .map((s) => s.seatId);
      this.autoSelectPoints();
      return;
    }

    this.isLoading = true;
    this.cdr.detectChanges();

    this.tripBookingService
      .getTripDetails(this.tripId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (trip) => {
          this.trip = trip;
          this.bookingFlowState.setTrip(this.tripId, trip);
          this.cdr.detectChanges();

          this.tripBookingService
            .getTripPoints(this.tripId)
            .pipe(
              finalize(() => { this.isLoading = false; this.cdr.detectChanges(); }),
              takeUntil(this.destroy$)
            )
            .subscribe({
              next: (points) => {
                this.boardingPoints = points.boardingPoints;
                this.droppingPoints = points.droppingPoints;
                this.bookingFlowState.setTripPoints(points);
                this.autoSelectPoints();
              },
              error: () => {
                this.errorMessage = 'Unable to load boarding and dropping points.';
                this.cdr.detectChanges();
              },
            });
        },
        error: () => {
          this.isLoading = false;
          this.errorMessage = 'Unable to load trip details. Please go back and try again.';
          this.cdr.detectChanges();
        },
      });
  }

  private startClock(): void {
    if (this.timerHandle) window.clearInterval(this.timerHandle);
    this.now = Date.now();
    this.timerHandle = window.setInterval(() => {
      this.now = Date.now();
      if (this.reservation?.lockedUntil && new Date(this.reservation.lockedUntil).getTime() <= Date.now()) {
        this.errorMessage = 'Seat reservation expired. Please reserve seats again.';
        this.bookingFlowState.clear();
        this.reservation = null;
        this.selectedTripSeatIds = [];
        this.selectedSeatIds = [];
        window.clearInterval(this.timerHandle);
      }
      this.cdr.detectChanges();
    }, 1000);
  }

  private normalize(value: unknown): string {
    return String(value ?? '').trim().toLowerCase();
  }
}

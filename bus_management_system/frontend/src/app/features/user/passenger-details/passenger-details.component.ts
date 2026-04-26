import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { Subject, finalize, takeUntil } from 'rxjs';
import { BookingFlowStateService } from '../../../core/services/booking-flow-state.service';
import { TripBookingService } from '../../../core/services/trip-booking.service';
import { TripDetailResponse, TripPointResponse, BookingPassengerRequest } from '../../../core/models/trip-booking.models';

@Component({
  selector: 'app-passenger-details',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './passenger-details.component.html',
  styleUrl: './passenger-details.component.css',
})
export class PassengerDetailsComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  private timerHandle?: number;

  trip: TripDetailResponse | null = null;
  selectedTripSeatIds: number[] = [];
  boardingPoints: TripPointResponse[] = [];
  droppingPoints: TripPointResponse[] = [];
  boardingPointId = 0;
  droppingPointId = 0;
  reservationLockedUntil = '';
  isSubmitting = false;
  errorMessage = '';
  successMessage = '';
  now = Date.now();

  readonly form;

  constructor(
    private readonly fb: FormBuilder,
    private readonly router: Router,
    private readonly tripBookingService: TripBookingService,
    private readonly bookingFlowState: BookingFlowStateService,
    private readonly cdr: ChangeDetectorRef
  ) {
    this.form = this.fb.group({
      passengers: this.fb.array([]),
    });
  }

  ngOnInit(): void {
    const state = this.bookingFlowState.getState();
    if (!state.tripId || !state.trip || state.selectedTripSeatIds.length === 0) {
      this.router.navigate(['/home']);
      return;
    }

    this.trip = state.trip;
    this.selectedTripSeatIds = state.selectedTripSeatIds;
    this.reservationLockedUntil = state.reservation?.lockedUntil ?? '';

    // ✅ Do NOT read boardingPointId/droppingPointId from state here
    // loadPoints() will always fetch fresh and call autoSelectDefaults()
    this.boardingPointId = 0;
    this.droppingPointId = 0;

    this.loadPoints(state.tripId);
    this.buildPassengerControls();
    this.startClock();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.timerHandle) window.clearInterval(this.timerHandle);
  }

  get passengers(): FormArray {
    return this.form.get('passengers') as FormArray;
  }

  get canSubmit(): boolean {
    return this.form.valid && this.isReservationActive() && !this.isSubmitting;
  }

  get timerLabel(): string {
    if (!this.reservationLockedUntil) return '05:00';
    const remaining = Math.max(0, new Date(this.reservationLockedUntil).getTime() - this.now);
    const minutes = Math.floor(remaining / 60000);
    const seconds = Math.floor((remaining % 60000) / 1000);
    return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
  }

  get timerIsLow(): boolean {
    if (!this.reservationLockedUntil) return false;
    return new Date(this.reservationLockedUntil).getTime() - this.now < 60000;
  }

  get boardingPoint(): TripPointResponse | null {
    return this.boardingPoints.find(p => p.id === this.boardingPointId) ?? null;
  }

  get droppingPoint(): TripPointResponse | null {
    return this.droppingPoints.find(p => p.id === this.droppingPointId) ?? null;
  }

  get totalAmount(): number {
    if (!this.trip) return 0;
    return this.selectedTripSeatIds.reduce((total, tripSeatId) => {
      const seat = this.trip?.seats.find((s) => s.tripSeatId === tripSeatId);
      return total + (seat?.price ?? this.trip?.baseFare ?? 0);
    }, 0);
  }

  selectedSeatNumbers(): string[] {
    if (!this.trip) return [];
    return this.trip.seats
      .filter((s) => this.selectedTripSeatIds.includes(s.tripSeatId))
      .map((s) => s.seatNumber);
  }

  passengerSeatLabel(index: number): string {
    return this.selectedSeatNumbers()[index] ?? `Seat ${index + 1}`;
  }

  formatTime(value: string): string {
    const date = new Date(value);
    return Number.isNaN(date.getTime())
      ? value
      : date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  formatDate(value: string): string {
    const date = new Date(value);
    return Number.isNaN(date.getTime())
      ? value
      : date.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
  }

  isReservationActive(): boolean {
    return !!this.reservationLockedUntil &&
      new Date(this.reservationLockedUntil).getTime() > Date.now();
  }

  submitBooking(): void {
    if (!this.trip || !this.isReservationActive()) {
      this.errorMessage = 'Your reservation expired. Please choose seats again.';
      this.cdr.detectChanges();
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.errorMessage = 'Please fill in all passenger details correctly.';
      this.cdr.detectChanges();
      return;
    }

    const passengers: BookingPassengerRequest[] = this.passengers.controls.map((ctrl, i) => ({
      tripSeatId: this.selectedTripSeatIds[i],
      name: String(ctrl.get('name')?.value ?? '').trim(),
      age: Number(ctrl.get('age')?.value ?? 0),
      gender: String(ctrl.get('gender')?.value ?? 'male') as 'male' | 'female' | 'other',
    }));

    this.isSubmitting = true;
    this.errorMessage = '';
    this.cdr.detectChanges();

    console.log('Submitting booking:', {
      tripId: this.trip.tripId,
      boardingPointId: this.boardingPointId,
      droppingPointId: this.droppingPointId,
      passengers: passengers
    });

    this.tripBookingService
      .createBooking({
        tripId: this.trip.tripId,
        boardingPointId: this.boardingPointId,
        droppingPointId: this.droppingPointId,
        passengers,
      })
      .pipe(
        finalize(() => { this.isSubmitting = false; this.cdr.detectChanges(); }),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (response) => {
          this.successMessage = `Booking confirmed! #${response.bookingId}`;
          this.bookingFlowState.clear();
          this.cdr.detectChanges();
          this.router.navigate(['/bookings', response.bookingId, 'payment']);
        },
        error: () => {
          this.errorMessage = 'Unable to complete booking. Please try again.';
          this.cdr.detectChanges();
        },
      });
  }

  private loadPoints(tripId: number): void {
    this.tripBookingService
      .getTripPoints(tripId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (points) => {
          this.boardingPoints = points.boardingPoints;
          this.droppingPoints = points.droppingPoints;
          this.bookingFlowState.setTripPoints(points);
          this.autoSelectDefaults();
          this.cdr.detectChanges();
        },
        error: () => {
          this.errorMessage = 'Unable to load stop information.';
          this.cdr.detectChanges();
        },
      });
  }

  private autoSelectDefaults(): void {
    if (this.boardingPoints.length > 0) {
      const def = this.boardingPoints.find(p => p.isDefault) ?? this.boardingPoints[0];
      this.boardingPointId = def.id;
    }

    if (this.droppingPoints.length > 0) {
      const def = this.droppingPoints.find(p => p.isDefault) ?? this.droppingPoints[0];
      this.droppingPointId = def.id;
    }

    console.log('autoSelectDefaults:', {
      boardingPointId: this.boardingPointId,
      droppingPointId: this.droppingPointId,
      boardingPoints: this.boardingPoints,
      droppingPoints: this.droppingPoints
    });
  }




  private buildPassengerControls(): void {
    this.passengers.clear();
    this.selectedTripSeatIds.forEach(() => {
      this.passengers.push(
        this.fb.group({
          name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
          age: [null, [Validators.required, Validators.min(1), Validators.max(120)]],
          gender: ['male', Validators.required],
        })
      );
    });
  }

  private startClock(): void {
    this.now = Date.now();
    this.timerHandle = window.setInterval(() => {
      this.now = Date.now();
      if (!this.isReservationActive()) {
        this.errorMessage = 'Your reservation expired. Please go back and reserve seats again.';
        if (this.timerHandle) window.clearInterval(this.timerHandle);
      }
      this.cdr.detectChanges();
    }, 1000);
  }
}

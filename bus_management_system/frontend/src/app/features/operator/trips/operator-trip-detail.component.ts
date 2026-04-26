import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, NgZone, OnInit } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { catchError, finalize, forkJoin, of } from 'rxjs';
import { AdminBadgeComponent } from '../../admin/shared/admin-badge.component';
import { AdminTableShellComponent } from '../../admin/shared/admin-table-shell.component';
import { OperatorSeatMapComponent } from './components/operator-seat-map.component';
import {
  OperatorBookingDetailResponse,
  OperatorBookingSummaryResponse,
  PassengerManifestRow,
  SeatPricingResponse,
  TripDetailResponse,
} from '../../../core/models/operator.models';
import { OperatorService } from '../../../core/services/operator.service';

@Component({
  selector: 'app-operator-trip-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, AdminTableShellComponent, AdminBadgeComponent, OperatorSeatMapComponent],
  templateUrl: './operator-trip-detail.component.html',
  styleUrl: './operator-trip-detail.component.css',
})
export class OperatorTripDetailComponent implements OnInit {
  isLoading = true;
  isSavingPricing = false;

  message = '';
  messageTone: 'success' | 'danger' = 'success';

  tripId = 0;
  tripDetail: TripDetailResponse | null = null;
  pricingRows: SeatPricingResponse[] = [];
  bookingSummaries: OperatorBookingSummaryResponse[] = [];
  bookingDetails: OperatorBookingDetailResponse[] = [];
  manifestRows: PassengerManifestRow[] = [];

  readonly pricingForm;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly operatorService: OperatorService,
    private readonly fb: FormBuilder,
    private readonly zone: NgZone,
    private readonly cdr: ChangeDetectorRef
  ) {
    this.pricingForm = this.fb.group({
      prices: this.fb.array([]),
    });
  }

  ngOnInit(): void {
    const tripId = Number(this.route.snapshot.paramMap.get('id'));
    this.tripId = Number.isNaN(tripId) ? 0 : tripId;

    if (!this.tripId) {
      this.message = 'Invalid trip id.';
      this.messageTone = 'danger';
      this.isLoading = false;
      return;
    }

    this.loadTripDetail();
  }

  get pricesArray(): FormArray {
    return this.pricingForm.controls.prices as FormArray;
  }

  get bookedSeatNumbers(): string[] {
    const fromBookings = this.bookingDetails.flatMap((booking) => 
      booking.seats
        .filter((seat) => String(seat.status).trim().toLowerCase() !== '1' && String(seat.status).trim().toLowerCase() !== 'cancelled')
        .map((seat) => seat.seatNumber)
    );
    return Array.from(new Set(fromBookings));
  }

  loadTripDetail(): void {
    this.isLoading = true;
    this.message = '';

    forkJoin({
      trip: this.operatorService.getTripDetail(this.tripId),
      pricing: this.operatorService.getTripPricing(this.tripId).pipe(catchError(() => of([] as SeatPricingResponse[]))),
      bookings: this.operatorService.getBookings().pipe(catchError(() => of([] as OperatorBookingSummaryResponse[]))),
    })
      .pipe(
        finalize(() => {
          this.zone.run(() => {
            this.isLoading = false;
            this.cdr.markForCheck();
          });
        })
      )
      .subscribe({
        next: ({ trip, pricing, bookings }) => {
          const tripBookings = bookings.filter((booking) => booking.tripId === this.tripId);

          const detailsRequests = tripBookings.map((booking) =>
            this.operatorService.getBookingDetail(booking.bookingId).pipe(catchError(() => of(null)))
          );

          if (detailsRequests.length === 0) {
            this.zone.run(() => {
              this.tripDetail = trip;
              this.pricingRows = pricing;
              this.bookingSummaries = tripBookings;
              this.bookingDetails = [];
              this.manifestRows = [];
              this.setupPricingForm(pricing, trip);
              this.cdr.markForCheck();
            });
            return;
          }

          forkJoin(detailsRequests).subscribe({
            next: (details) => {
              this.zone.run(() => {
                this.tripDetail = trip;
                this.pricingRows = pricing;
                this.bookingSummaries = tripBookings;
                this.bookingDetails = details.filter((item): item is OperatorBookingDetailResponse => Boolean(item));
                this.manifestRows = this.buildManifestRows(this.bookingDetails);
                this.setupPricingForm(pricing, trip);
                this.cdr.markForCheck();
              });
            },
            error: () => {
              this.zone.run(() => {
                this.tripDetail = trip;
                this.pricingRows = pricing;
                this.bookingSummaries = tripBookings;
                this.bookingDetails = [];
                this.manifestRows = [];
                this.setupPricingForm(pricing, trip);
                this.cdr.markForCheck();
              });
            },
          });
        },
        error: () => {
          this.zone.run(() => {
            this.message = 'Unable to load trip details right now.';
            this.messageTone = 'danger';
            this.cdr.markForCheck();
          });
        },
      });
  }

  savePricing(): void {
    if (!this.tripDetail) {
      return;
    }

    if (this.pricingForm.invalid) {
      this.pricingForm.markAllAsTouched();
      return;
    }

    const prices = this.pricesArray.controls.map((group) => ({
      seatId: Number(group.get('seatId')?.value),
      price: Number(group.get('price')?.value),
    }));

    this.isSavingPricing = true;
    this.message = '';

    this.operatorService
      .setTripPricing(this.tripId, { prices })
      .pipe(
        finalize(() => {
          this.zone.run(() => {
            this.isSavingPricing = false;
            this.cdr.markForCheck();
          });
        })
      )
      .subscribe({
        next: (savedRows) => {
          this.zone.run(() => {
            this.pricingRows = savedRows;
            this.setupPricingForm(savedRows, this.tripDetail as TripDetailResponse);
            this.message = 'Seat pricing updated successfully.';
            this.messageTone = 'success';
            this.cdr.markForCheck();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.message = 'Unable to save pricing right now.';
            this.messageTone = 'danger';
            this.cdr.markForCheck();
          });
        },
      });
  }

  private readonly tripStatusMap: Record<string, string> = {
    '0': 'Scheduled',
    '1': 'Active',
    '2': 'Completed',
    '3': 'Cancelled',
  };

  private readonly seatStatusMap: Record<string, string> = {
    '0': 'Confirmed',
    '1': 'Cancelled',
  };

  statusTone(status: number | string, type: 'trip' | 'seat' = 'trip'): 'success' | 'warning' | 'danger' | 'dark' {
    const label = this.statusLabel(status, type).toLowerCase();

    if (label.includes('active') || label.includes('scheduled') || label.includes('confirmed')) {
      return 'success';
    }

    if (label.includes('cancel')) {
      return 'danger';
    }

    if (label.includes('complete')) {
      return 'dark';
    }

    return 'warning';
  }

  statusLabel(value: number | string, type: 'trip' | 'seat' = 'trip'): string {
    const key = String(value).trim().toLowerCase();
    
    if (type === 'trip' && this.tripStatusMap[key]) return this.tripStatusMap[key];
    if (type === 'seat' && this.seatStatusMap[key]) return this.seatStatusMap[key];

    return String(value)
      .replaceAll('_', ' ')
      .replace(/\b\w/g, (segment) => segment.toUpperCase());
  }

  formatGender(value: number | string | null | undefined): string {
    if (value === null || value === undefined) {
      return '-';
    }

    const normalized = String(value).trim().toLowerCase();
    if (normalized === '0' || normalized === 'male') {
      return 'Male';
    }

    if (normalized === '1' || normalized === 'female') {
      return 'Female';
    }

    if (normalized === '2' || normalized === 'other') {
      return 'Other';
    }

    return String(value);
  }

  getDeckLabel(seatId: number): string {
    if (!this.tripDetail) {
      return 'Lower';
    }

    const seat = this.tripDetail.seats.find((item) => item.seatId === seatId);
    if (!seat) {
      return 'Lower';
    }

    const normalized = String(seat.deck).trim().toLowerCase();
    if (normalized === '1' || normalized === 'upper') {
      return 'Upper';
    }

    if (normalized === '2' || normalized === 'single') {
      return 'Single';
    }

    return 'Lower';
  }

  private setupPricingForm(pricing: SeatPricingResponse[], trip: TripDetailResponse): void {
    this.pricesArray.clear();

    const effectiveRows = pricing.length > 0 ? pricing : trip.seats.map((seat) => ({
      seatId: seat.seatId,
      seatNumber: seat.seatNumber,
      price: Number(seat.price ?? trip.baseFare),
    }));

    effectiveRows.forEach((row) => {
      this.pricesArray.push(
        this.fb.group({
          seatId: [row.seatId, [Validators.required]],
          seatNumber: [row.seatNumber, [Validators.required]],
          deck: [this.getDeckLabel(row.seatId)],
          price: [Number(row.price), [Validators.required, Validators.min(1)]],
        })
      );
    });
  }

  private buildManifestRows(details: OperatorBookingDetailResponse[]): PassengerManifestRow[] {
    return details
      .flatMap((booking) =>
        booking.seats.map((seat) => ({
          seatNumber: seat.seatNumber,
          passengerName: seat.passengerName?.trim() || booking.userName,
          age: seat.passengerAge,
          gender: seat.passengerGender,
          bookingStatus: seat.status,
        }))
      )
      .sort((left, right) => left.seatNumber.localeCompare(right.seatNumber));
  }
}

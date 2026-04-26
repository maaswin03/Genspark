import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { ConfirmModalComponent } from '../../../../shared/components/confirm-modal/confirm-modal.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { UserBookingDetailResponse, UserBookingSeatResponse } from '../../../../core/models/trip-booking.models';
import { TripBookingService } from '../../../../core/services/trip-booking.service';

type CancelScope = 'booking' | 'seat';

@Component({
  selector: 'app-booking-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, StatusBadgeComponent, ConfirmModalComponent],
  templateUrl: './booking-detail.component.html',
  styleUrl: './booking-detail.component.css',
})
export class BookingDetailComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();

  bookingId = 0;
  booking: UserBookingDetailResponse | null = null;
  isLoading = true;
  isCancelling = false;
  errorMessage = '';
  successMessage = '';

  showCancelModal = false;
  cancelScope: CancelScope = 'booking';
  seatToCancel: UserBookingSeatResponse | null = null;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly tripBookingService: TripBookingService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe((params) => {
      const bookingId = Number(params.get('id') ?? 0);
      if (!bookingId) {
        this.errorMessage = 'Invalid booking id.';
        this.isLoading = false;
        return;
      }

      this.bookingId = bookingId;
      this.loadBooking();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  canCancelBooking(): boolean {
    if (!this.booking) {
      return false;
    }
    const status = this.normalize(this.booking.status);
    // Can only cancel pending or confirmed
    if (status !== '0' && status !== '1' && status !== 'pending' && status !== 'confirmed') {
      return false;
    }
    // Cannot cancel if departed
    if (new Date(this.booking.departureTime).getTime() < Date.now()) {
      return false;
    }
    return true;
  }

  canCancelSeat(seat: UserBookingSeatResponse): boolean {
    if (!this.booking) return false;
    const status = this.normalize(seat.status);
    // BookingSeatStatus enum: 0 = Confirmed, 1 = Cancelled
    if (status === '1' || status === 'cancelled') {
      return false;
    }
    // Cannot cancel if departed
    if (new Date(this.booking.departureTime).getTime() < Date.now()) {
      return false;
    }
    return true;
  }
  
  isSeatCancelled(seat: UserBookingSeatResponse): boolean {
    const status = this.normalize(seat.status);
    return status === '1' || status === 'cancelled';
  }

  showPayAction(): boolean {
    if (!this.booking) {
      return false;
    }

    const status = this.normalize(this.booking.paymentStatus);
    return status !== 'paid' && status !== 'success' && status !== '1';
  }

  openBookingCancel(): void {
    this.cancelScope = 'booking';
    this.seatToCancel = null;
    this.showCancelModal = true;
  }

  openSeatCancel(seat: UserBookingSeatResponse): void {
    this.cancelScope = 'seat';
    this.seatToCancel = seat;
    this.showCancelModal = true;
  }

  closeModal(): void {
    if (this.isCancelling) {
      return;
    }

    this.showCancelModal = false;
    this.seatToCancel = null;
  }

  confirmCancel(): void {
    if (this.cancelScope === 'booking') {
      this.cancelFullBooking();
      return;
    }

    this.cancelSingleSeat();
  }

  modalTitle(): string {
    return this.cancelScope === 'booking' ? 'Cancel Full Booking' : `Cancel Seat ${this.seatToCancel?.seatNumber ?? ''}`;
  }

  modalMessage(): string {
    if (this.cancelScope === 'booking') {
      const seatSubtotal = this.booking?.seats.filter(s => !this.isSeatCancelled(s)).reduce((sum, s) => sum + s.amountPaid, 0) ?? 0;
      return `This will cancel all active seats for this booking. ` +
        (seatSubtotal > 0 ? `Refund of ₹${seatSubtotal.toLocaleString('en-IN')} for seat costs will be processed within 5–7 business days.` : '');
    }

    const refund = this.seatToCancel?.amountPaid ?? 0;
    return `Cancel seat ${this.seatToCancel?.seatNumber ?? ''}? ` +
      (refund > 0 ? `Refund of ₹${refund.toLocaleString('en-IN')} will be processed within 5–7 business days.` : '');
  }

  private loadBooking(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.cdr.detectChanges();

    this.tripBookingService
      .getUserBookingDetail(this.bookingId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (booking) => {
          this.booking = booking;
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.isLoading = false;
          this.errorMessage = 'Unable to load booking details.';
          this.cdr.detectChanges();
        },
      });
  }

  private cancelFullBooking(): void {
    this.isCancelling = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.cdr.detectChanges();
    this.tripBookingService
      .cancelBooking(this.bookingId, { cancelReason: 'Cancelled by user' })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isCancelling = false;
          this.successMessage = 'Booking cancelled successfully.';
          this.closeModal();
          this.loadBooking();
          this.cdr.detectChanges();
        },
        error: () => {
          this.isCancelling = false;
          this.errorMessage = 'Unable to cancel booking right now.';
          this.cdr.detectChanges();
        },
      });
  }

  private cancelSingleSeat(): void {
    if (!this.seatToCancel) {
      return;
    }

    this.isCancelling = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.cdr.detectChanges();
    this.tripBookingService
      .cancelSeat(this.bookingId, this.seatToCancel.bookingSeatId, { cancelReason: 'Cancelled by user' })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.isCancelling = false;
          this.successMessage = `Seat ${this.seatToCancel?.seatNumber ?? ''} cancelled successfully.`;
          this.closeModal();
          this.loadBooking();
          this.cdr.detectChanges();
        },
        error: () => {
          this.isCancelling = false;
          this.errorMessage = 'Unable to cancel this seat right now.';
          this.cdr.detectChanges();
        },
      });
  }

  normalize(value: number | string): string {
    return String(value ?? '').trim().toLowerCase();
  }

  formatGender(value: number | string | null | undefined): string {
    if (value === null || value === undefined) return '-';
    const normalized = String(value).trim().toLowerCase();
    if (normalized === '0' || normalized === 'male') return 'Male';
    if (normalized === '1' || normalized === 'female') return 'Female';
    if (normalized === '2' || normalized === 'other') return 'Other';
    return String(value);
  }
}

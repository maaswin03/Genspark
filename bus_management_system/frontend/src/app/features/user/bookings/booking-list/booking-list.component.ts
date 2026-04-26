import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { ConfirmModalComponent } from '../../../../shared/components/confirm-modal/confirm-modal.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { UserBookingResponse } from '../../../../core/models/trip-booking.models';
import { TripBookingService } from '../../../../core/services/trip-booking.service';

type BookingTab = 'all' | 'upcoming' | 'completed' | 'cancelled';

@Component({
  selector: 'app-booking-list',
  standalone: true,
  imports: [CommonModule, RouterModule, StatusBadgeComponent, ConfirmModalComponent],
  templateUrl: './booking-list.component.html',
  styleUrl: './booking-list.component.css',
})
export class BookingListComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();

  activeTab: BookingTab = 'all';
  bookings: UserBookingResponse[] = [];
  isLoading = true;
  errorMessage = '';
  successMessage = '';

  showCancelModal = false;
  isCancelling = false;
  selectedBooking: UserBookingResponse | null = null;

  readonly tabs: Array<{ key: BookingTab; label: string }> = [
    { key: 'all', label: 'All' },
    { key: 'upcoming', label: 'Upcoming' },
    { key: 'completed', label: 'Completed' },
    { key: 'cancelled', label: 'Cancelled' },
  ];

  constructor(
    private readonly tripBookingService: TripBookingService,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadBookings();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  setTab(tab: BookingTab): void {
    if (this.activeTab === tab) {
      return;
    }

    this.activeTab = tab;
    this.loadBookings();
  }

  openBooking(bookingId: number): void {
    this.router.navigate(['/user/bookings', bookingId]);
  }

  canCancel(booking: UserBookingResponse): boolean {
    const status = this.normalize(booking.status);
    // Can only cancel pending or confirmed bookings that have not yet departed
    if (status !== '0' && status !== 'pending' && status !== '1' && status !== 'confirmed') return false;
    return !this.isCompleted(booking);
  }

  /** Derive Upcoming / Completed from departure time when status is Confirmed */
  displayStatus(booking: UserBookingResponse): string {
    const s = this.normalize(booking.status);
    if (s === '1' || s === 'confirmed') {
      const dep = new Date(booking.departureTime).getTime();
      return dep >= Date.now() ? 'Upcoming' : 'Completed';
    }
    if (s === '0' || s === 'pending') return 'Pending';
    if (s === '2' || s === 'cancelled') return 'Cancelled';
    return String(booking.status);
  }

  isCancelledStatus(booking: UserBookingResponse): boolean {
    const s = this.normalize(booking.status);
    return s === '2' || s === 'cancelled';
  }

  isUpcoming(booking: UserBookingResponse): boolean {
    const s = this.normalize(booking.status);
    if (s !== '1' && s !== 'confirmed') return false;
    return new Date(booking.departureTime).getTime() >= Date.now();
  }

  isCompleted(booking: UserBookingResponse): boolean {
    const s = this.normalize(booking.status);
    if (s !== '1' && s !== 'confirmed') return false;
    return new Date(booking.departureTime).getTime() < Date.now();
  }

  cancelModalMessage(): string {
    if (!this.selectedBooking) return 'Cancel this booking?';
    const amount = this.selectedBooking.totalAmount;
    return `This will cancel all seats in this booking.${
      amount > 0 ? ` Refund of ₹${amount.toLocaleString('en-IN')} will be processed within 5–7 business days.` : ''
    }`;
  }

  openCancelModal(booking: UserBookingResponse): void {
    this.selectedBooking = booking;
    this.showCancelModal = true;
  }

  closeCancelModal(): void {
    if (this.isCancelling) {
      return;
    }

    this.showCancelModal = false;
    this.selectedBooking = null;
  }

  confirmCancel(): void {
    if (!this.selectedBooking) {
      return;
    }

    this.isCancelling = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.tripBookingService
      .cancelBooking(this.selectedBooking.bookingId, { cancelReason: 'Cancelled by user' })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          if (this.selectedBooking) {
            this.bookings = this.bookings.map((booking) =>
              booking.bookingId === this.selectedBooking?.bookingId
                ? { ...booking, status: 'cancelled' }
                : booking
            );
          }
          this.isCancelling = false;
          this.successMessage = 'Booking cancelled successfully.';
          this.closeCancelModal();
          this.cdr.detectChanges();
        },
        error: () => {
          this.isCancelling = false;
          this.errorMessage = 'Unable to cancel this booking right now.';
          this.cdr.detectChanges();
        },
      });
  }

  private loadBookings(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.cdr.detectChanges();

    const status = this.activeTab === 'all' ? undefined : this.activeTab;
    this.tripBookingService
      .getUserBookings(status)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (rows) => {
          this.bookings = rows;
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.isLoading = false;
          this.errorMessage = 'Could not load your bookings right now.';
          this.cdr.detectChanges();
        },
      });
  }

  normalize(value: number | string): string {
    return String(value ?? '').trim().toLowerCase();
  }
}

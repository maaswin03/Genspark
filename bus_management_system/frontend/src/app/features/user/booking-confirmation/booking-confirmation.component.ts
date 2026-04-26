import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { TripBookingService } from '../../../core/services/trip-booking.service';
import { UserBookingDetailResponse } from '../../../core/models/trip-booking.models';

@Component({
  selector: 'app-booking-confirmation',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './booking-confirmation.component.html',
  styleUrls: ['./booking-confirmation.component.css'],
})
export class BookingConfirmationComponent implements OnInit {
  bookingId = 0;
  isLoading = true;
  errorMessage = '';
  booking: UserBookingDetailResponse | null = null;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly tripBookingService: TripBookingService,
    private readonly cdr: ChangeDetectorRef,
    @Inject(PLATFORM_ID) private readonly platformId: object
  ) {}

  ngOnInit(): void {
    const bookingId = Number(this.route.snapshot.paramMap.get('id') ?? 0);
    if (!bookingId) {
      this.errorMessage = 'Invalid booking id.';
      this.isLoading = false;
      this.cdr.detectChanges();
      return;
    }

    this.bookingId = bookingId;
    this.tripBookingService.getUserBookingDetail(bookingId).subscribe({
      next: (booking) => {
        this.booking = booking;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.errorMessage = 'Unable to load ticket details.';
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  get seatSubtotal(): number {
    return (this.booking?.seats ?? []).reduce((sum, seat) => sum + Number(seat.amountPaid || 0), 0);
  }

  get seatNumbersLabel(): string {
    return (this.booking?.seats ?? []).map((seat) => seat.seatNumber).join(', ');
  }

  printTicket(): void {
    if (isPlatformBrowser(this.platformId)) {
      window.print();
    }
  }

  goToBookings(): void {
    this.router.navigate(['/user/bookings']);
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

import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { TripBookingService } from '../../../core/services/trip-booking.service';
import { PaymentStatusResponse, UserBookingDetailResponse } from '../../../core/models/trip-booking.models';

type PaymentMethod = 'upi' | 'card' | 'netbanking' | 'wallet';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [CommonModule, RouterModule, StatusBadgeComponent],
  templateUrl: './payment.component.html',
  styleUrl: './payment.component.css',
})
export class PaymentComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  private pollHandle?: number;

  bookingId = 0;
  selectedMethod: PaymentMethod = 'upi';
  isLoading = true;
  isInitiating = false;
  errorMessage = '';
  successMessage = '';
  booking: UserBookingDetailResponse | null = null;
  paymentStatus: PaymentStatusResponse | null = null;

  readonly paymentMethods: Array<{ value: PaymentMethod; label: string; subtitle: string }> = [
    { value: 'upi',        label: 'UPI',                subtitle: 'Google Pay, PhonePe, BHIM and more' },
    { value: 'card',       label: 'Debit / Credit Card', subtitle: 'Visa, Mastercard, Rupay supported' },
    { value: 'netbanking', label: 'Net Banking',         subtitle: 'Major banks supported instantly' },
    { value: 'wallet',     label: 'Wallet',              subtitle: 'Fast checkout using linked wallets' },
  ];

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
        this.cdr.detectChanges();
        return;
      }
      this.bookingId = bookingId;
      this.loadPage();
    });
  }

  ngOnDestroy(): void {
    this.stopPolling();
    this.destroy$.next();
    this.destroy$.complete();
  }

  get amount(): number {
    return this.paymentStatus?.amount ?? this.booking?.totalAmount ?? 0;
  }

  get seatFare(): number {
    return (this.booking?.seats ?? []).reduce((sum, seat) => sum + Number(seat.amountPaid || 0), 0);
  }

  get totalPayable(): number {
    if (this.booking) {
      const computed = this.seatFare + Number(this.booking.platformFee || 0);
      if (computed > 0) {
        return computed;
      }
    }

    return this.amount;
  }

  get isPaid(): boolean {
    const status = this.normalize(
      this.paymentStatus?.paymentStatus ?? this.booking?.paymentStatus ?? ''
    );
    return status === '1' || status === 'paid' || status === 'success';
  }

  get isFailed(): boolean {
    const status = this.normalize(this.paymentStatus?.paymentStatus ?? '');
    return status === '2' || status === 'failed';
  }

  get isPending(): boolean {
    const status = this.normalize(
      this.paymentStatus?.paymentStatus ?? this.booking?.paymentStatus ?? 'pending'
    );
    return status === '0' || status === 'pending' || status === 'processing' || status === '';
  }

  initiatePayment(): void {
    if (!this.bookingId || this.isInitiating || this.isPaid) return;

    this.errorMessage = '';
    this.successMessage = '';
    this.isInitiating = true;

    this.tripBookingService
      .initiatePayment({ bookingId: this.bookingId, paymentMethod: this.selectedMethod })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          // Mock: immediately confirm via webhook
          this.tripBookingService
            .confirmPayment({
              paymentId: response.paymentId,
              status: 'success',
              gatewayTransactionId: `TXN-${Date.now()}`
            })
            .pipe(takeUntil(this.destroy$))
            .subscribe({
              next: () => {
                this.isInitiating = false;
                this.successMessage = 'Payment successful! Your booking is confirmed.';
                this.refreshPaymentStatus();
                this.cdr.detectChanges();
                setTimeout(() => {
                  this.router.navigate(['/bookings', this.bookingId, 'confirmation']);
                }, 1500);
              },
              error: () => {
                this.isInitiating = false;
                this.errorMessage = 'Payment confirmation failed. Please retry.';
                this.cdr.detectChanges();
              }
            });
        },
        error: () => {
          this.isInitiating = false;
          this.errorMessage = 'Unable to initiate payment right now. Please try again.';
          this.cdr.detectChanges();
        },
      });
  }

  retryStatusCheck(): void {
    this.errorMessage = '';
    this.successMessage = '';
    this.refreshPaymentStatus();
    this.startPolling();
  }

  goToBookings(): void {
    this.router.navigate(['/user/bookings']);
  }

  private loadPage(): void {
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
          this.refreshPaymentStatus();
          if (this.isPending) {
            this.startPolling();
          }
          this.cdr.detectChanges();
        },
        error: () => {
          this.isLoading = false;
          this.errorMessage = 'Unable to load booking details for payment.';
          this.cdr.detectChanges();
        },
      });
  }

  private refreshPaymentStatus(): void {
    this.tripBookingService
      .getPaymentStatus(this.bookingId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (status) => {
          this.paymentStatus = status;
          if (this.isPaid) {
            this.stopPolling();
            this.successMessage = 'Payment successful. Your seats are confirmed.';
          }
          if (this.isFailed) {
            this.stopPolling();
            this.errorMessage = 'Payment failed. You can retry.';
          }
          this.cdr.detectChanges();
        },
        error: (err) => {
          if (err?.status === 404) {
            this.paymentStatus = null;
            this.cdr.detectChanges();
            return;
          }
          this.errorMessage = 'Unable to fetch payment status. Please retry.';
          this.cdr.detectChanges();
        },
      });
  }

  private startPolling(): void {
    this.stopPolling();
    this.pollHandle = window.setInterval(() => {
      if (this.isPaid || this.isFailed) {
        this.stopPolling();
        return;
      }
      this.refreshPaymentStatus();
    }, 4000);
  }

  private stopPolling(): void {
    if (this.pollHandle) {
      window.clearInterval(this.pollHandle);
      this.pollHandle = undefined;
    }
  }

  private normalize(value: number | string): string {
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
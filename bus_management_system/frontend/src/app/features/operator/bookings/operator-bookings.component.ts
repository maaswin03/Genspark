import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { catchError, finalize, of } from 'rxjs';
import { AdminBadgeComponent } from '../../admin/shared/admin-badge.component';
import { AdminModalComponent } from '../../admin/shared/admin-modal.component';
import { AdminTableShellComponent } from '../../admin/shared/admin-table-shell.component';
import { OperatorBookingDetailResponse, OperatorBookingSummaryResponse } from '../../../core/models/operator.models';
import { OperatorService } from '../../../core/services/operator.service';

@Component({
  selector: 'app-operator-bookings',
  standalone: true,
  imports: [CommonModule, FormsModule, AdminTableShellComponent, AdminBadgeComponent, AdminModalComponent],
  templateUrl: './operator-bookings.component.html',
  styleUrl: './operator-bookings.component.css',
})
export class OperatorBookingsComponent implements OnInit {
  isLoading = true;
  isDetailLoading = false;
  message = '';
  messageTone: 'success' | 'danger' = 'success';

  bookings: OperatorBookingSummaryResponse[] = [];
  selectedBooking: OperatorBookingDetailResponse | null = null;
  detailModalOpen = false;

  searchText = '';
  statusFilter = 'all';
  startDate = '';
  endDate = '';

  constructor(
    private readonly operatorService: OperatorService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadBookings();
  }

  loadBookings(): void {
    this.isLoading = true;
    this.cdr.detectChanges();
    this.operatorService
      .getBookings()
      .pipe(
        catchError((error) => {
          console.error('Failed to load bookings', error);
          this.bookings = [];
          this.showMessage('Failed to load bookings. Please try again.', 'danger');
          return of([] as OperatorBookingSummaryResponse[]);
        }),
        finalize(() => {
          this.isLoading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe((rows) => {
        this.bookings = rows;
        this.cdr.detectChanges();
      });
  }

  get filteredBookings(): OperatorBookingSummaryResponse[] {
    const search = this.searchText.trim().toLowerCase();
    const start = this.startDate ? new Date(`${this.startDate}T00:00:00`) : null;
    const end = this.endDate ? new Date(`${this.endDate}T23:59:59`) : null;

    return this.bookings.filter((booking) => {
      const statusLabel = this.formatBookingStatus(booking.bookingStatus).toLowerCase();
      if (this.statusFilter !== 'all' && statusLabel !== this.statusFilter) {
        return false;
      }

      const departure = new Date(booking.departureTime);
      if (start && departure < start) {
        return false;
      }

      if (end && departure > end) {
        return false;
      }

      if (!search) {
        return true;
      }

      return (
        String(booking.bookingId).includes(search) ||
        booking.userName.toLowerCase().includes(search) ||
        booking.userEmail.toLowerCase().includes(search)
      );
    });
  }

  openBookingDetail(bookingId: number): void {
    this.detailModalOpen = true;
    this.isDetailLoading = true;
    this.selectedBooking = null;
    this.cdr.detectChanges();

    this.operatorService
      .getBookingDetail(bookingId)
      .pipe(
        catchError((error) => {
          console.error('Failed to load booking detail', error);
          this.showMessage('Failed to load booking detail.', 'danger');
          this.closeDetailModal();
          return of(null);
        }),
        finalize(() => {
          this.isDetailLoading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe((detail) => {
        this.selectedBooking = detail;
        this.cdr.detectChanges();
      });
  }

  closeDetailModal(): void {
    this.detailModalOpen = false;
    this.selectedBooking = null;
  }

  bookingBadgeTone(status: number | string): 'success' | 'warning' | 'danger' | 'dark' | 'info' {
    const normalized = String(status ?? '').toLowerCase();

    if (normalized.includes('confirmed') || normalized.includes('booked') || normalized === '1') {
      return 'success';
    }

    if (normalized.includes('pending') || normalized === '0') {
      return 'warning';
    }

    if (normalized.includes('cancel') || normalized === '2') {
      return 'danger';
    }

    if (normalized.includes('completed') || normalized === '3') {
      return 'info';
    }

    return 'dark';
  }

  seatBadgeTone(status: number | string): 'success' | 'warning' | 'danger' | 'dark' | 'info' {
    const normalized = String(status ?? '').toLowerCase();

    if (normalized.includes('confirmed') || normalized === '0') {
      return 'success';
    }

    if (normalized.includes('cancel') || normalized === '1') {
      return 'danger';
    }

    return 'dark';
  }

  formatBookingStatus(status: number | string): string {
    const value = String(status ?? '').trim();
    if (!value) {
      return 'Unknown';
    }

    if (/^\d+$/.test(value)) {
      switch (Number(value)) {
        case 0:
          return 'Pending';
        case 1:
          return 'Confirmed';
        case 2:
          return 'Cancelled';
        case 3:
          return 'Completed';
        default:
          return `Status ${value}`;
      }
    }

    return value;
  }

  formatSeatStatus(status: number | string): string {
    const value = String(status ?? '').trim();
    if (!value) {
      return 'Unknown';
    }

    if (/^\d+$/.test(value)) {
      switch (Number(value)) {
        case 0:
          return 'Confirmed';
        case 1:
          return 'Cancelled';
        default:
          return `Status ${value}`;
      }
    }

    return value;
  }

  trackByBookingId(_: number, item: OperatorBookingSummaryResponse): number {
    return item.bookingId;
  }

  clearFilters(): void {
    this.searchText = '';
    this.statusFilter = 'all';
    this.startDate = '';
    this.endDate = '';
  }

  private showMessage(text: string, tone: 'success' | 'danger'): void {
    this.message = text;
    this.messageTone = tone;
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

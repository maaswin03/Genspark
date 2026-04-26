import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, NgZone, OnInit } from '@angular/core';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';
import { catchError, finalize, forkJoin, of } from 'rxjs';
import { AdminBadgeComponent } from '../../admin/shared/admin-badge.component';
import { AdminTableShellComponent } from '../../admin/shared/admin-table-shell.component';
import {
  BusResponse,
  OperatorBookingSummaryResponse,
  OperatorRevenueSummaryResponse,
  OperatorTripRevenueResponse,
  OperatorTripResponse,
} from '../../../core/models/operator.models';
import { OperatorService } from '../../../core/services/operator.service';

@Component({
  selector: 'app-operator-dashboard',
  standalone: true,
  imports: [CommonModule, BaseChartDirective, AdminBadgeComponent, AdminTableShellComponent],
  templateUrl: './operator-dashboard.component.html',
  styleUrl: './operator-dashboard.component.css',
})
export class OperatorDashboardComponent implements OnInit {
  private readonly tripStatusLabels: Record<number, string> = {
    0: 'Scheduled',
    1: 'Active',
    2: 'Completed',
    3: 'Cancelled',
  };

  private readonly bookingStatusLabels: Record<number, string> = {
    0: 'Pending',
    1: 'Confirmed',
    2: 'Cancelled',
  };

  isLoading = true;
  errorMessage = '';

  revenueSummary: OperatorRevenueSummaryResponse = {
    operatorId: 0,
    companyName: '',
    totalRevenue: 0,
    totalPlatformFee: 0,
    totalOperatorEarning: 0,
    totalTransactions: 0,
  };

  buses: BusResponse[] = [];
  trips: OperatorTripResponse[] = [];
  bookings: OperatorBookingSummaryResponse[] = [];
  tripRevenue: OperatorTripRevenueResponse[] = [];

  chartType: 'bar' = 'bar';
  chartData: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: [
      {
        label: 'Monthly earnings',
        data: [],
        backgroundColor: '#dc2626',
      },
    ],
  };

  chartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: false,
      },
    },
    scales: {
      y: {
        beginAtZero: true,
      },
    },
  };

  constructor(
    private readonly operatorService: OperatorService,
    private readonly zone: NgZone,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.isLoading = true;
    this.errorMessage = '';

    forkJoin({
      revenue: this.operatorService.getRevenue().pipe(
        catchError(() =>
          of({
            operatorId: 0,
            companyName: '',
            totalRevenue: 0,
            totalPlatformFee: 0,
            totalOperatorEarning: 0,
            totalTransactions: 0,
          })
        )
      ),
      trips: this.operatorService.getTrips().pipe(catchError(() => of([] as OperatorTripResponse[]))),
      bookings: this.operatorService.getBookings().pipe(catchError(() => of([] as OperatorBookingSummaryResponse[]))),
      buses: this.operatorService.getBuses().pipe(catchError(() => of([] as BusResponse[]))),
      tripRevenue: this.operatorService.getTripRevenue().pipe(catchError(() => of([] as OperatorTripRevenueResponse[]))),
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
        next: ({ revenue, trips, bookings, buses, tripRevenue }) => {
          this.zone.run(() => {
            this.revenueSummary = revenue;
            this.trips = trips;
            this.bookings = bookings;
            this.buses = buses;
            this.tripRevenue = tripRevenue;
            this.updateChart(tripRevenue);
            this.cdr.markForCheck();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.errorMessage = 'Unable to load dashboard data right now.';
            this.cdr.markForCheck();
          });
        },
      });
  }

  get activeBusesCount(): number {
    return this.buses.filter((bus) => bus.isActive).length;
  }

  get currentMonthBookings(): number {
    const currentMonth = new Date().getMonth();
    return this.bookings.filter((booking) => {
      const isCurrentMonth = new Date(booking.departureTime).getMonth() === currentMonth;
      const status = this.getBookingStatusLabel(booking.bookingStatus).toLowerCase();
      return isCurrentMonth && status !== 'cancelled';
    }).length;
  }

  get upcomingTripsToday(): OperatorTripResponse[] {
    const today = new Date();
    const now = today.getTime();

    return this.trips
      .filter((trip) => this.isSameDay(trip.departureTime, today) && new Date(trip.departureTime).getTime() >= now)
      .sort((a, b) => new Date(a.departureTime).getTime() - new Date(b.departureTime).getTime());
  }

  get recentBookings(): OperatorBookingSummaryResponse[] {
    return [...this.bookings]
      .filter((booking) => this.getBookingStatusLabel(booking.bookingStatus).toLowerCase() !== 'cancelled')
      .sort((a, b) => new Date(b.departureTime).getTime() - new Date(a.departureTime).getTime())
      .slice(0, 5);
  }

  get chartHasData(): boolean {
    return Boolean(this.chartData.labels?.length);
  }

  formatTripTime(value: string): string {
    return new Date(value).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  getTripStatusTone(status: number | string): 'success' | 'warning' | 'danger' | 'dark' {
    const normalized = this.getTripStatusLabel(status).toLowerCase();
    if (normalized === 'active') {
      return 'success';
    }

    if (normalized === 'scheduled') {
      return 'warning';
    }

    if (normalized === 'cancelled') {
      return 'danger';
    }

    return 'dark';
  }

  getBookingStatusTone(status: number | string): 'success' | 'warning' | 'danger' | 'dark' {
    const normalized = this.getBookingStatusLabel(status).toLowerCase();
    if (normalized === 'confirmed') {
      return 'success';
    }

    if (normalized === 'pending') {
      return 'warning';
    }

    if (normalized === 'cancelled') {
      return 'danger';
    }

    return 'dark';
  }

  getTripStatusLabel(value: number | string): string {
    const numeric = this.toNullableNumber(value);
    if (numeric !== null && numeric in this.tripStatusLabels) {
      return this.tripStatusLabels[numeric];
    }

    return this.normalizeLabel(value);
  }

  getBookingStatusLabel(value: number | string): string {
    const numeric = this.toNullableNumber(value);
    if (numeric !== null && numeric in this.bookingStatusLabels) {
      return this.bookingStatusLabels[numeric];
    }

    return this.normalizeLabel(value);
  }

  private updateChart(rows: OperatorTripRevenueResponse[]): void {
    const monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    const values = Array.from({ length: 12 }, () => 0);

    rows.forEach((row) => {
      const monthIndex = new Date(row.departureTime).getMonth();
      values[monthIndex] += Number(row.operatorEarning ?? 0);
    });

    this.chartData = {
      labels: monthNames,
      datasets: [
        {
          label: 'Monthly earnings',
          data: values,
          backgroundColor: '#dc2626',
        },
      ],
    };
  }

  private isSameDay(value: string, other: Date): boolean {
    const date = new Date(value);
    return (
      date.getFullYear() === other.getFullYear() &&
      date.getMonth() === other.getMonth() &&
      date.getDate() === other.getDate()
    );
  }

  private normalizeLabel(value: number | string): string {
    return String(value)
      .replaceAll('_', ' ')
      .replace(/\b\w/g, (segment) => segment.toUpperCase());
  }

  private toNullableNumber(value: number | string): number | null {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : null;
  }
}

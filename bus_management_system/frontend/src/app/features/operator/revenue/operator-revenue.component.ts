import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ChartConfiguration } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { catchError, finalize, forkJoin, of } from 'rxjs';
import {
  OperatorRevenueSummaryResponse,
  OperatorTripRevenueResponse,
} from '../../../core/models/operator.models';
import { OperatorService } from '../../../core/services/operator.service';
import { AdminTableShellComponent } from '../../admin/shared/admin-table-shell.component';

@Component({
  selector: 'app-operator-revenue',
  standalone: true,
  imports: [CommonModule, FormsModule, BaseChartDirective, AdminTableShellComponent],
  templateUrl: './operator-revenue.component.html',
  styleUrl: './operator-revenue.component.css',
})
export class OperatorRevenueComponent implements OnInit {
  isLoading = true;
  errorMessage = '';
  selectedMonth = this.getCurrentMonthInput();

  summary: OperatorRevenueSummaryResponse = {
    operatorId: 0,
    companyName: '',
    totalRevenue: 0,
    totalPlatformFee: 0,
    totalOperatorEarning: 0,
    totalTransactions: 0,
  };

  tripRevenueRows: OperatorTripRevenueResponse[] = [];

  lineChartData: ChartConfiguration<'line'>['data'] = {
    labels: [],
    datasets: [
      {
        label: 'Operator Earnings',
        data: [],
        borderColor: '#ef4444',
        backgroundColor: 'rgba(239, 68, 68, 0.16)',
        fill: true,
        tension: 0.28,
      },
    ],
  };

  lineChartOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
    },
    scales: {
      y: {
        beginAtZero: true,
      },
    },
  };

  constructor(
    private readonly operatorService: OperatorService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.cdr.detectChanges();

    forkJoin({
      summary: this.operatorService.getRevenue().pipe(
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
      trips: this.operatorService.getTripRevenue().pipe(catchError(() => of([] as OperatorTripRevenueResponse[]))),
    })
      .pipe(finalize(() => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: ({ summary, trips }) => {
          this.summary = summary;
          this.tripRevenueRows = [...trips].sort(
            (a, b) => new Date(b.departureTime).getTime() - new Date(a.departureTime).getTime()
          );
          this.rebuildLineChart();
          this.cdr.detectChanges();
        },
        error: () => {
          this.errorMessage = 'Unable to load revenue data right now.';
          this.cdr.detectChanges();
        },
      });
  }

  onMonthChange(): void {
    this.rebuildLineChart();
  }

  get filteredTripRows(): OperatorTripRevenueResponse[] {
    if (!this.selectedMonth) {
      return this.tripRevenueRows;
    }

    return this.tripRevenueRows.filter((row) => this.matchesMonth(row.departureTime, this.selectedMonth));
  }

  get filteredMonthBookings(): number {
    return this.filteredTripRows.reduce((sum, row) => sum + Number(row.totalBookings ?? 0), 0);
  }

  get filteredMonthNetEarnings(): number {
    return this.filteredTripRows.reduce((sum, row) => sum + Number(row.operatorEarning ?? 0), 0);
  }

  get filteredMonthTripsCount(): number {
    return this.filteredTripRows.length;
  }

  get totalTripsCount(): number {
    return this.tripRevenueRows.length;
  }

  get avgEarningPerTrip(): number {
    return this.totalTripsCount > 0 ? this.summary.totalOperatorEarning / this.totalTripsCount : 0;
  }

  exportCsv(): void {
    const rows = this.filteredTripRows;
    const header = [
      'Trip Id',
      'Route',
      'Departure',
      'Arrival',
      'Bookings',
      'Net Earnings',
    ];

    const lines = rows.map((row) => [
      String(row.tripId),
      this.csvEscape(row.routeName),
      this.csvEscape(new Date(row.departureTime).toISOString()),
      this.csvEscape(new Date(row.arrivalTime).toISOString()),
      String(row.totalBookings),
      String(row.operatorEarning),
    ]);

    const csv = [header, ...lines].map((line) => line.join(',')).join('\n');
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `operator-revenue-${this.selectedMonth || 'all'}.csv`;
    link.click();
    URL.revokeObjectURL(url);
  }

  private rebuildLineChart(): void {
    const labels = this.buildLast12MonthLabels();
    const monthlyEarnings = labels.map((label) =>
      this.tripRevenueRows
        .filter((row) => this.labelForDate(row.departureTime) === label)
        .reduce((sum, row) => sum + Number(row.operatorEarning ?? 0), 0)
    );

    this.lineChartData = {
      labels,
      datasets: [
        {
          label: 'Operator Earnings',
          data: monthlyEarnings,
          borderColor: '#ef4444',
          backgroundColor: 'rgba(239, 68, 68, 0.16)',
          fill: true,
          tension: 0.28,
          pointRadius: 3,
          pointHoverRadius: 5,
        },
      ],
    };
  }

  private buildLast12MonthLabels(): string[] {
    const now = new Date();
    const labels: string[] = [];

    for (let i = 11; i >= 0; i -= 1) {
      const date = new Date(now.getFullYear(), now.getMonth() - i, 1);
      labels.push(date.toLocaleString('en-IN', { month: 'short', year: 'numeric' }));
    }

    return labels;
  }

  private labelForDate(value: string): string {
    const date = new Date(value);
    return date.toLocaleString('en-IN', { month: 'short', year: 'numeric' });
  }

  private matchesMonth(value: string, monthInput: string): boolean {
    const date = new Date(value);
    const [year, month] = monthInput.split('-').map(Number);

    return date.getFullYear() === year && date.getMonth() + 1 === month;
  }

  private getCurrentMonthInput(): string {
    const date = new Date();
    return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`;
  }

  private csvEscape(value: string): string {
    return `"${(value ?? '').replace(/"/g, '""')}"`;
  }
}

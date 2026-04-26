import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, NgZone } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';
import { finalize, forkJoin } from 'rxjs';
import {
  AdminOperatorRevenueResponse,
  AdminRevenueSummaryResponse,
} from '../../../core/models/admin.models';
import { AdminService } from '../../../core/services/admin.service';
import { AdminTableShellComponent } from '../shared/admin-table-shell.component';

@Component({
  selector: 'app-admin-revenue',
  standalone: true,
  imports: [CommonModule, FormsModule, BaseChartDirective, AdminTableShellComponent],
  templateUrl: './admin-revenue.component.html',
  styleUrl: './admin-revenue.component.css',
})
export class AdminRevenueComponent {
  isLoading = true;
  message = '';
  messageTone: 'success' | 'danger' = 'success';

  selectedMonth = this.getCurrentMonthInput();

  summary: AdminRevenueSummaryResponse = {
    totalRevenue: 0,
    totalPlatformFee: 0,
    totalOperatorEarning: 0,
    totalTransactions: 0,
  };

  operatorRows: AdminOperatorRevenueResponse[] = [];

  chartData: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: [
      {
        label: 'Monthly Revenue',
        data: [],
        backgroundColor: '#dc2626',
        maxBarThickness: 26,
      },
    ],
  };

  chartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { display: false } },
    scales: { y: { beginAtZero: true } },
  };

  constructor(
    private readonly adminService: AdminService,
    private readonly zone: NgZone,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadRevenue();
  }

  loadRevenue(): void {
    this.isLoading = true;
    this.message = '';

    forkJoin({
      summary: this.adminService.getRevenueSummary(),
      operatorRows: this.adminService.getOperatorRevenue(),
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
        next: ({ summary, operatorRows }) => {
          this.zone.run(() => {
            this.summary = summary;
            this.operatorRows = [...operatorRows].sort((a, b) => b.totalRevenue - a.totalRevenue);
            this.rebuildChart();
            this.cdr.markForCheck();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.message = 'Unable to load revenue details right now.';
            this.messageTone = 'danger';
            this.cdr.markForCheck();
          });
        },
      });
  }

  onMonthChange(): void {
    this.rebuildChart();
  }

  exportCsv(): void {
    const header = ['Operator Name', 'Total Bookings', 'Gross Revenue', 'Platform Fee', 'Net Payout'];
    const rows = this.operatorRows.map((row) => [
      this.csvEscape(row.companyName),
      String(row.totalTransactions),
      String(row.totalRevenue),
      String(row.totalPlatformFee),
      String(row.totalOperatorEarning),
    ]);

    const csv = [header, ...rows].map((line) => line.join(',')).join('\n');
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `admin-revenue-${this.selectedMonth || 'all'}.csv`;
    link.click();
    URL.revokeObjectURL(url);
  }

  private rebuildChart(): void {
    const labels = this.buildLast12MonthLabels();
    const selectedLabel = this.toMonthLabel(this.selectedMonth) ?? labels[labels.length - 1];

    // Backend currently provides aggregate totals only; render selected month snapshot.
    const values = labels.map((label) => (label === selectedLabel ? this.summary.totalRevenue : 0));
    this.chartData = {
      labels,
      datasets: [
        {
          label: 'Monthly Revenue',
          data: values,
          backgroundColor: labels.map((label) => (label === selectedLabel ? '#dc2626' : '#fecaca')),
          maxBarThickness: 26,
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

  private toMonthLabel(monthInput: string): string | null {
    if (!monthInput) {
      return null;
    }

    const [year, month] = monthInput.split('-').map((value) => Number(value));
    if (!year || !month) {
      return null;
    }

    return new Date(year, month - 1, 1).toLocaleString('en-IN', { month: 'short', year: 'numeric' });
  }

  private getCurrentMonthInput(): string {
    const date = new Date();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    return `${date.getFullYear()}-${month}`;
  }

  private csvEscape(value: string): string {
    const escaped = (value ?? '').replace(/"/g, '""');
    return `"${escaped}"`;
  }
}

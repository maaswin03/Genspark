import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, NgZone, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';
import { catchError, finalize, forkJoin, of } from 'rxjs';
import {
  AdminRevenueSummaryResponse,
  OperatorProfileResponse,
  normalizeApprovalStatus,
  statusBadgeClass,
} from '../../../core/models/admin.models';
import { AdminService } from '../../../core/services/admin.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, BaseChartDirective, RouterLink],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.css',
})
export class AdminDashboardComponent {
  isLoading = true;
  errorMessage = '';

  operators: OperatorProfileResponse[] = [];
  recentOperators: OperatorProfileResponse[] = [];
  revenueSummary: AdminRevenueSummaryResponse = {
    totalRevenue: 0,
    totalPlatformFee: 0,
    totalOperatorEarning: 0,
    totalTransactions: 0,
  };

  operatorCounts = {
    pending: 0,
    approved: 0,
    blocked: 0,
  };

  chartType: 'bar' = 'bar';
  chartData: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: [
      {
        label: 'Revenue',
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
    private readonly adminService: AdminService,
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
      revenue: this.adminService.getRevenueSummary().pipe(
        catchError(() =>
          of({
            totalRevenue: 0,
            totalPlatformFee: 0,
            totalOperatorEarning: 0,
            totalTransactions: 0,
          })
        )
      ),
      operators: this.adminService.getOperators().pipe(catchError(() => of([] as OperatorProfileResponse[]))),
      operatorRevenue: this.adminService.getOperatorRevenue().pipe(catchError(() => of([] as any[]))),
    })
      .pipe(finalize(() => {
        this.zone.run(() => {
          this.isLoading = false;
          this.cdr.markForCheck();
        });
      }))
      .subscribe({
        next: ({ revenue, operators, operatorRevenue }) => {
          this.zone.run(() => {
            this.revenueSummary = revenue;
            this.operators = operators;

            const sorted = [...operators].sort(
              (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
            );
            this.recentOperators = sorted.slice(0, 6);

            this.operatorCounts = {
              pending: operators.filter((x) => this.getStatusLabel(x.approvalStatus) === 'Pending').length,
              approved: operators.filter((x) => this.getStatusLabel(x.approvalStatus) === 'Approved').length,
              blocked: operators.filter((x) => this.getStatusLabel(x.approvalStatus) === 'Blocked').length,
            };

            const topRows = (operatorRevenue as Array<{ companyName: string; totalRevenue: number }>).slice(0, 6);
            this.chartData = {
              labels: topRows.map((row) => row.companyName),
              datasets: [
                {
                  label: 'Revenue',
                  data: topRows.map((row) => row.totalRevenue),
                  backgroundColor: '#dc2626',
                },
              ],
            };

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

  getStatusLabel(status: number | string): string {
    return normalizeApprovalStatus(status);
  }

  getStatusClass(status: number | string): string {
    return statusBadgeClass(normalizeApprovalStatus(status));
  }
}

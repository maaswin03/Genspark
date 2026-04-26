import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, NgZone, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { OperatorProfileResponse, normalizeApprovalStatus, statusBadgeClass } from '../../../core/models/admin.models';
import { AdminService } from '../../../core/services/admin.service';

@Component({
  selector: 'app-admin-operators',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-operators.component.html',
  styleUrl: './admin-operators.component.css',
})
export class AdminOperatorsComponent {
  isLoading = true;
  errorMessage = '';
  searchTerm = '';

  operators: OperatorProfileResponse[] = [];
  filteredOperators: OperatorProfileResponse[] = [];

  readonly filterOptions = ['All', 'Pending', 'Approved', 'Rejected', 'Blocked'] as const;
  activeFilter: (typeof this.filterOptions)[number] = 'All';

  constructor(
    private readonly adminService: AdminService,
    private readonly router: Router,
    private readonly zone: NgZone,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadOperators();
  }

  loadOperators(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.adminService
      .getOperators()
      .pipe(finalize(() => {
        this.zone.run(() => {
          this.isLoading = false;
          this.cdr.markForCheck();
        });
      }))
      .subscribe({
        next: (rows) => {
          this.zone.run(() => {
            this.operators = rows;
            this.applyFilters();
            this.cdr.markForCheck();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.errorMessage = 'Unable to load operators right now.';
            this.cdr.markForCheck();
          });
        },
      });
  }

  setFilter(filter: (typeof this.filterOptions)[number]): void {
    this.activeFilter = filter;
    this.applyFilters();
  }

  applyFilters(): void {
    const term = this.searchTerm.trim().toLowerCase();
    this.filteredOperators = this.operators
      .filter((operator) => {
        const statusLabel = this.getStatusLabel(operator.approvalStatus);
        const matchesFilter = this.activeFilter === 'All' || statusLabel === this.activeFilter;
        const matchesSearch = !term || operator.companyName.toLowerCase().includes(term);
        return matchesFilter && matchesSearch;
      })
      .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
  }

  goToDetail(operatorId: number): void {
    this.router.navigate(['/admin/operators', operatorId]);
  }

  approve(operatorId: number): void {
    this.adminService.approveOperator(operatorId).subscribe({
      next: (updated) => {
        this.zone.run(() => this.updateLocalStatus(updated.id, updated.approvalStatus));
      },
      error: () => {
        this.errorMessage = 'Unable to approve operator right now.';
      },
    });
  }

  reject(operatorId: number): void {
    const reason = window.prompt('Enter rejection reason');
    if (!reason?.trim()) {
      return;
    }

    this.adminService.rejectOperator(operatorId, reason.trim()).subscribe({
      next: (updated) => {
        this.zone.run(() => this.updateLocalStatus(updated.id, updated.approvalStatus));
      },
      error: () => {
        this.errorMessage = 'Unable to reject operator right now.';
      },
    });
  }

  block(operatorId: number): void {
    const reason = window.prompt('Enter blocked reason');
    if (!reason?.trim()) {
      return;
    }

    this.adminService.blockOperator(operatorId, reason.trim()).subscribe({
      next: (updated) => {
        this.zone.run(() => this.updateLocalStatus(updated.id, updated.approvalStatus));
      },
      error: () => {
        this.errorMessage = 'Unable to block operator right now.';
      },
    });
  }

  getStatusLabel(status: number | string): string {
    return normalizeApprovalStatus(status);
  }

  getStatusClass(status: number | string): string {
    return statusBadgeClass(normalizeApprovalStatus(status));
  }

  private updateLocalStatus(operatorId: number, status: number | string): void {
    this.operators = this.operators.map((operator) =>
      operator.id === operatorId ? { ...operator, approvalStatus: status } : operator
    );
    this.applyFilters();
    this.cdr.markForCheck();
  }
}

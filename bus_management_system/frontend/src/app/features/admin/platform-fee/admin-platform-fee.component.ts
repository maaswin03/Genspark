import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, NgZone } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { PlatformFeeResponse } from '../../../core/models/admin.models';
import { AdminService } from '../../../core/services/admin.service';
import { AdminBadgeComponent } from '../shared/admin-badge.component';
import { AdminModalComponent } from '../shared/admin-modal.component';
import { AdminTableShellComponent } from '../shared/admin-table-shell.component';

type PlatformFeeHistoryRow = {
  feeValue: number;
  updatedAt: string;
  isActive: boolean;
};

@Component({
  selector: 'app-admin-platform-fee',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, AdminModalComponent, AdminTableShellComponent, AdminBadgeComponent],
  templateUrl: './admin-platform-fee.component.html',
  styleUrl: './admin-platform-fee.component.css',
})
export class AdminPlatformFeeComponent {
  isLoading = true;
  isSaving = false;
  message = '';
  messageTone: 'success' | 'danger' = 'success';

  currentFee: PlatformFeeResponse | null = null;
  feeHistory: PlatformFeeHistoryRow[] = [];

  isConfirmModalOpen = false;

  readonly feeForm;

  constructor(
    private readonly adminService: AdminService,
    private readonly fb: FormBuilder,
    private readonly zone: NgZone,
    private readonly cdr: ChangeDetectorRef
  ) {
    this.feeForm = this.fb.group({
      feeValue: [null as number | null, [Validators.required, Validators.min(0), Validators.max(100)]],
    });
  }

  ngOnInit(): void {
    this.loadPlatformFee();
  }

  get feeValuePreview(): number {
    const fee = Number(this.feeForm.controls.feeValue.value ?? 0);
    if (!Number.isFinite(fee) || fee < 0) {
      return 0;
    }

    return Math.round((1000 * fee) / 100);
  }

  loadPlatformFee(): void {
    this.isLoading = true;
    this.message = '';

    this.adminService
      .getPlatformFee()
      .pipe(
        finalize(() => {
          this.zone.run(() => {
            this.isLoading = false;
            this.cdr.markForCheck();
          });
        })
      )
      .subscribe({
        next: (response) => {
          this.zone.run(() => {
            this.currentFee = response;
            this.feeForm.patchValue({ feeValue: response.feeValue });
            this.feeHistory = this.mergeHistoryRow({
              feeValue: response.feeValue,
              updatedAt: response.updatedAt,
              isActive: true,
            });
            this.cdr.markForCheck();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.currentFee = null;
            this.feeHistory = [];
            this.message = 'No active platform fee found yet. Set a new fee to continue.';
            this.messageTone = 'danger';
            this.cdr.markForCheck();
          });
        },
      });
  }

  openConfirmModal(): void {
    if (this.feeForm.invalid) {
      this.feeForm.markAllAsTouched();
      return;
    }

    this.isConfirmModalOpen = true;
  }

  closeConfirmModal(): void {
    if (this.isSaving) {
      return;
    }

    this.isConfirmModalOpen = false;
  }

  submitFee(): void {
    if (this.feeForm.invalid) {
      this.feeForm.markAllAsTouched();
      return;
    }

    const feeValue = Number(this.feeForm.controls.feeValue.value);
    if (!Number.isFinite(feeValue) || feeValue < 0 || feeValue > 100) {
      this.feeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.message = '';

    this.adminService
      .setPlatformFee({ feeValue })
      .pipe(
        finalize(() => {
          this.zone.run(() => {
            this.isSaving = false;
            this.cdr.markForCheck();
          });
        })
      )
      .subscribe({
        next: (response) => {
          this.zone.run(() => {
            this.currentFee = response;
            this.feeHistory = this.mergeHistoryRow({
              feeValue: response.feeValue,
              updatedAt: response.updatedAt,
              isActive: true,
            });
            this.isConfirmModalOpen = false;
            this.message = 'Platform fee updated successfully.';
            this.messageTone = 'success';
            this.cdr.markForCheck();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.message = 'Unable to update platform fee right now.';
            this.messageTone = 'danger';
            this.cdr.markForCheck();
          });
        },
      });
  }

  private mergeHistoryRow(latest: PlatformFeeHistoryRow): PlatformFeeHistoryRow[] {
    const previous = this.feeHistory.map((item) => ({ ...item, isActive: false }));
    const withoutDuplicate = previous.filter(
      (item) => !(item.feeValue === latest.feeValue && item.updatedAt === latest.updatedAt)
    );

    return [latest, ...withoutDuplicate].sort(
      (a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime()
    );
  }
}

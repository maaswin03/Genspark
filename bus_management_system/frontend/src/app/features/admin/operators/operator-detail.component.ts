import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, NgZone, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError, finalize, forkJoin, of, switchMap } from 'rxjs';
import {
  AdminOperatorRevenueResponse,
  ApprovalStatusLabel,
  BusResponse,
  OperatorDetailResponse,
  normalizeApprovalStatus,
  statusBadgeClass,
} from '../../../core/models/admin.models';
import { AdminService } from '../../../core/services/admin.service';

type ModalType = 'reject' | 'block' | null;

@Component({
  selector: 'app-operator-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './operator-detail.component.html',
  styleUrl: './operator-detail.component.css',
})
export class OperatorDetailComponent {
  isLoading = true;
  isSubmitting = false;
  errorMessage = '';
  activeModal: ModalType = null;
  operatorId = 0;

  operator: OperatorDetailResponse | null = null;
  buses: BusResponse[] = [];
  revenueSummary: AdminOperatorRevenueResponse | null = null;

  readonly actionForm;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly fb: FormBuilder,
    private readonly adminService: AdminService,
    private readonly zone: NgZone,
    private readonly cdr: ChangeDetectorRef
  ) {
    this.actionForm = this.fb.group({
      reason: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(1000)]],
    });
  }

  ngOnInit(): void {
    this.loadDetail();
  }

  loadDetail(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.route.paramMap
      .pipe(
        switchMap((params) => {
          this.operatorId = Number(params.get('id'));
          if (!this.operatorId) {
            this.zone.run(() => {
              this.errorMessage = 'Invalid operator id.';
              this.isLoading = false;
              this.cdr.markForCheck();
            });
            return of(null);
          }

          return forkJoin({
            operator: this.adminService.getOperatorDetail(this.operatorId),
            allBuses: this.adminService.getAllBuses(),
            operatorRevenue: this.adminService.getOperatorRevenue(),
          }).pipe(
            catchError(() => {
              this.zone.run(() => {
                this.errorMessage = 'Unable to load operator details right now.';
                this.isLoading = false;
                this.cdr.markForCheck();
              });
              return of(null);
            })
          );
        })
      )
      .subscribe({
        next: (result) => {
          this.zone.run(() => {
            this.isLoading = false;
            
            if (result) {
              this.operator = result.operator;
              this.buses = result.allBuses.filter((bus: BusResponse) => bus.operatorId === result.operator.id);
              this.revenueSummary = result.operatorRevenue.find((row) => row.operatorId === result.operator.id) ?? null;
            }
            
            this.cdr.markForCheck();
          });
        }
      });
  }

  openDocument(url: string): void {
    if (!url) {
      return;
    }

    window.open(url, '_blank', 'noopener');
  }

  verifyDocument(documentId: number): void {
    if (!this.operator) {
      return;
    }

    this.isSubmitting = true;
    this.adminService
      .verifyOperatorDocument(this.operator.id, documentId)
      .pipe(finalize(() => {
        this.zone.run(() => {
          this.isSubmitting = false;
          this.cdr.markForCheck();
        });
      }))
      .subscribe({
        next: () => {
          this.zone.run(() => {
            this.operator = {
              ...this.operator!,
              documents: this.operator!.documents.map((doc) =>
                doc.id === documentId ? { ...doc, verifiedAt: new Date().toISOString() } : doc
              ),
            };
            this.cdr.markForCheck();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.errorMessage = 'Document verification failed. Please try again.';
            this.cdr.markForCheck();
          });
        },
      });
  }

  approve(): void {
    this.runStatusAction(() => this.adminService.approveOperator(this.operatorId));
  }

  unblock(): void {
    this.runStatusAction(() => this.adminService.unblockOperator(this.operatorId));
  }

  showRejectModal(): void {
    this.activeModal = 'reject';
    this.actionForm.reset({ reason: '' });
  }

  showBlockModal(): void {
    this.activeModal = 'block';
    this.actionForm.reset({ reason: '' });
  }

  closeModal(): void {
    this.activeModal = null;
    this.actionForm.reset({ reason: '' });
  }

  submitModalAction(): void {
    if (this.actionForm.invalid || !this.activeModal) {
      this.actionForm.markAllAsTouched();
      return;
    }

    const reason = this.actionForm.controls.reason.value?.trim() ?? '';
    const request = this.activeModal === 'reject'
      ? () => this.adminService.rejectOperator(this.operatorId, reason)
      : () => this.adminService.blockOperator(this.operatorId, reason);

    this.runStatusAction(request, true);
  }

  goBack(): void {
    this.router.navigateByUrl('/admin/operators');
  }

  get statusLabel(): ApprovalStatusLabel {
    return normalizeApprovalStatus(this.operator?.approvalStatus);
  }

  get statusClass(): string {
    return statusBadgeClass(this.statusLabel);
  }

  get reasonInvalid(): boolean {
    const control = this.actionForm.controls.reason;
    return control.touched && control.invalid;
  }

  get timelineSteps(): Array<{ label: ApprovalStatusLabel; complete: boolean; active: boolean }> {
    const current = this.statusLabel;
    const order: ApprovalStatusLabel[] = ['Pending', 'Approved', 'Rejected', 'Blocked'];

    return order.map((label) => ({
      label,
      complete:
        current === label ||
        (current === 'Approved' && label === 'Pending') ||
        ((current === 'Rejected' || current === 'Blocked') && label === 'Pending'),
      active: current === label,
    }));
  }

  private runStatusAction(
    action: () => ReturnType<AdminService['approveOperator']>,
    closeModalAfterSuccess = false
  ): void {
    this.isSubmitting = true;
    this.errorMessage = '';

    action()
      .pipe(finalize(() => {
        this.zone.run(() => {
          this.isSubmitting = false;
          this.cdr.markForCheck();
        });
      }))
      .subscribe({
        next: (updated) => {
          this.zone.run(() => {
            this.operator = updated;
            if (closeModalAfterSuccess) {
              this.closeModal();
            }
            this.cdr.markForCheck();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.errorMessage = 'Unable to update operator status. Please try again.';
            this.cdr.markForCheck();
          });
        },
      });
  }
}

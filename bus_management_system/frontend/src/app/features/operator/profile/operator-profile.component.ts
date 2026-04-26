import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { catchError, finalize, forkJoin, of } from 'rxjs';
import {
  OperatorDocumentResponse,
  OperatorProfileResponse,
  UploadOperatorDocumentRequest,
} from '../../../core/models/operator.models';
import { OperatorService } from '../../../core/services/operator.service';
import { AdminBadgeComponent } from '../../admin/shared/admin-badge.component';
import { AdminTableShellComponent } from '../../admin/shared/admin-table-shell.component';

@Component({
  selector: 'app-operator-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, AdminBadgeComponent, AdminTableShellComponent],
  templateUrl: './operator-profile.component.html',
  styleUrl: './operator-profile.component.css',
})
export class OperatorProfileComponent implements OnInit {
  isLoading = true;
  isSavingProfile = false;
  isSavingPassword = false;
  isUploadingDoc = false;

  message = '';
  messageTone: 'success' | 'danger' = 'success';

  profile: OperatorProfileResponse | null = null;
  documents: OperatorDocumentResponse[] = [];

  profileForm;

  passwordForm;

  documentForm;

  readonly documentTypeOptions = [
    { value: 0, label: 'License' },
    { value: 1, label: 'Registration' },
    { value: 2, label: 'Insurance' },
    { value: 3, label: 'Other' },
  ];

  constructor(
    private readonly fb: FormBuilder,
    private readonly operatorService: OperatorService,
    private readonly cdr: ChangeDetectorRef
  ) {
    this.profileForm = this.fb.nonNullable.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      phone: ['', [Validators.required, Validators.maxLength(20)]],
      companyName: ['', [Validators.required, Validators.maxLength(200)]],
    });

    this.passwordForm = this.fb.nonNullable.group({
      currentPassword: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(100)]],
      newPassword: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(100)]],
      confirmPassword: ['', [Validators.required]],
    });

    this.documentForm = this.fb.nonNullable.group({
      documentType: [0, [Validators.required]],
      fileUrl: ['', [Validators.required, Validators.maxLength(500)]],
    });
  }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    this.cdr.detectChanges();

    forkJoin({
      profile: this.operatorService.getProfile().pipe(catchError(() => of(null))),
      documents: this.operatorService.getDocuments().pipe(catchError(() => of([] as OperatorDocumentResponse[]))),
    })
      .pipe(finalize(() => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: ({ profile, documents }) => {
          if (!profile) {
            this.showMessage('Unable to load profile details right now.', 'danger');
            this.cdr.detectChanges();
            return;
          }

          this.profile = profile;
          this.documents = documents;
          this.profileForm.reset({
            name: profile.name,
            phone: profile.phone,
            companyName: profile.companyName,
          });
          this.cdr.detectChanges();
        },
        error: () => {
          this.showMessage('Unable to load profile details right now.', 'danger');
          this.cdr.detectChanges();
        },
      });
  }

  saveProfile(): void {
    if (this.profileForm.invalid || this.isSavingProfile) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.isSavingProfile = true;
    const payload = this.profileForm.getRawValue();

    this.operatorService
      .updateProfile(payload)
      .pipe(finalize(() => (this.isSavingProfile = false)))
      .subscribe({
        next: (updated) => {
          this.profile = updated;
          this.showMessage('Profile updated successfully.', 'success');
        },
        error: (error) => {
          this.handleHttpError(error, 'Unable to update profile.');
        },
      });
  }

  savePassword(): void {
    if (this.passwordForm.invalid || this.isSavingPassword) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    const { currentPassword, newPassword, confirmPassword } = this.passwordForm.getRawValue();
    if (newPassword !== confirmPassword) {
      this.showMessage('New password and confirmation do not match.', 'danger');
      return;
    }

    this.isSavingPassword = true;

    this.operatorService
      .changePassword({ currentPassword, newPassword })
      .pipe(finalize(() => (this.isSavingPassword = false)))
      .subscribe({
        next: () => {
          this.passwordForm.reset({
            currentPassword: '',
            newPassword: '',
            confirmPassword: '',
          });
          this.showMessage('Password changed successfully.', 'success');
        },
        error: (error) => {
          this.handleHttpError(error, 'Unable to change password.');
        },
      });
  }

  uploadDocument(): void {
    if (this.documentForm.invalid || this.isUploadingDoc) {
      this.documentForm.markAllAsTouched();
      return;
    }

    this.isUploadingDoc = true;
    const value = this.documentForm.getRawValue();
    const payload: UploadOperatorDocumentRequest = {
      documentType: Number(value.documentType),
      fileUrl: value.fileUrl.trim(),
    };

    this.operatorService
      .uploadDocument(payload)
      .pipe(finalize(() => (this.isUploadingDoc = false)))
      .subscribe({
        next: (document) => {
          this.documents = [document, ...this.documents];
          this.documentForm.reset({
            documentType: 0,
            fileUrl: '',
          });
          this.showMessage('Document uploaded successfully.', 'success');
        },
        error: (error) => {
          this.handleHttpError(error, 'Unable to upload document.');
        },
      });
  }

  get approvalStatusLabel(): string {
    if (!this.profile) {
      return 'Pending';
    }

    const value = String(this.profile.approvalStatus).toLowerCase();
    if (value === '1' || value === 'approved') {
      return 'Approved';
    }

    if (value === '2' || value === 'rejected') {
      return 'Rejected';
    }

    if (value === '3' || value === 'blocked') {
      return 'Blocked';
    }

    return 'Pending';
  }

  get approvalBadgeTone(): 'warning' | 'success' | 'danger' | 'dark' {
    const status = this.approvalStatusLabel;
    if (status === 'Approved') {
      return 'success';
    }

    if (status === 'Rejected') {
      return 'danger';
    }

    if (status === 'Blocked') {
      return 'dark';
    }

    return 'warning';
  }

  documentTypeLabel(value: number | string): string {
    const numValue = Number(value);
    switch (numValue) {
      case 0:
        return 'License';
      case 1:
        return 'Registration';
      case 2:
        return 'Insurance';
      case 3:
        return 'Other';
      default:
        return String(value);
    }
  }

  verificationLabel(document: OperatorDocumentResponse): string {
    return document.verifiedAt ? 'Verified' : 'Pending Verification';
  }

  verificationTone(document: OperatorDocumentResponse): 'success' | 'warning' {
    return document.verifiedAt ? 'success' : 'warning';
  }

  private handleHttpError(error: unknown, fallbackMessage: string): void {
    const message =
      typeof error === 'object' &&
      error !== null &&
      'error' in error &&
      typeof (error as { error?: unknown }).error === 'string'
        ? (error as { error: string }).error
        : fallbackMessage;

    this.showMessage(message, 'danger');
  }

  private showMessage(text: string, tone: 'success' | 'danger'): void {
    this.message = text;
    this.messageTone = tone;
  }
}

import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { ChangeUserPasswordRequest, UserProfileResponse } from '../../../core/models/user-portal.models';
import { UserPortalService } from '../../../core/services/user-portal.service';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css',
})
export class ProfileComponent implements OnInit {
  isLoading = true;
  isSavingProfile = false;
  isSavingPassword = false;
  errorMessage = '';
  successMessage = '';
  profile: UserProfileResponse | null = null;
  accountCreatedAt = new Date();

  readonly profileForm;
  readonly passwordForm;

  constructor(
    private readonly fb: FormBuilder,
    private readonly userPortalService: UserPortalService,
    private readonly cdr: ChangeDetectorRef
  ) {
    this.profileForm = this.fb.nonNullable.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      email: [{ value: '', disabled: true }, [Validators.required, Validators.email]],
      phone: ['', [Validators.required, Validators.maxLength(20)]],
      createdAt: [{ value: '', disabled: true }],
    });

    this.passwordForm = this.fb.nonNullable.group({
      currentPassword: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(100)]],
      newPassword: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(100)]],
      confirmPassword: ['', [Validators.required]],
    });
  }

  ngOnInit(): void {
    this.loadProfile();
  }

  get passwordsMatch(): boolean {
    const { newPassword, confirmPassword } = this.passwordForm.getRawValue();
    return !confirmPassword || newPassword === confirmPassword;
  }

  saveProfile(): void {
    if (this.profileForm.invalid || this.isSavingProfile) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.errorMessage = '';
    this.successMessage = '';
    this.isSavingProfile = true;
    this.cdr.detectChanges();

    const payload = {
      name: this.profileForm.controls.name.value.trim(),
      phone: this.profileForm.controls.phone.value.trim(),
    };

    this.userPortalService
      .updateProfile(payload)
      .pipe(finalize(() => (this.isSavingProfile = false)))
      .subscribe({
        next: (updated) => {
          this.profile = updated;
          this.successMessage = 'Profile updated successfully.';
          this.patchProfileForm(updated);
          this.cdr.detectChanges();
        },
        error: () => {
          this.errorMessage = 'Unable to update profile right now.';
          this.cdr.detectChanges();
        },
      });
  }

  changePassword(): void {
    if (this.passwordForm.invalid || !this.passwordsMatch || this.isSavingPassword || !this.profile) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    this.errorMessage = '';
    this.successMessage = '';
    this.isSavingPassword = true;
    this.cdr.detectChanges();

    const payload: ChangeUserPasswordRequest = this.passwordForm.getRawValue();
    this.userPortalService
      .changePassword(payload, {
        name: this.profile.name,
        phone: this.profile.phone,
      })
      .pipe(finalize(() => (this.isSavingPassword = false)))
      .subscribe({
        next: () => {
          this.successMessage = 'Password change request submitted.';
          this.passwordForm.reset({
            currentPassword: '',
            newPassword: '',
            confirmPassword: '',
          });
          this.cdr.detectChanges();
        },
        error: () => {
          this.errorMessage = 'Unable to change password right now.';
          this.cdr.detectChanges();
        },
      });
  }

  private loadProfile(): void {
    this.errorMessage = '';
    this.isLoading = true;
    this.cdr.detectChanges();

    this.userPortalService
      .getProfile()
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (profile) => {
          this.profile = profile;
          this.patchProfileForm(profile);
          this.cdr.detectChanges();
        },
        error: () => {
          this.errorMessage = 'Unable to load your profile right now.';
          this.cdr.detectChanges();
        },
      });
  }

  private patchProfileForm(profile: UserProfileResponse): void {
    this.profileForm.patchValue({
      name: profile.name,
      email: profile.email,
      phone: profile.phone,
      createdAt: this.accountCreatedAt.toLocaleDateString(),
    });
  }
}

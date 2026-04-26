import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { finalize, from, of, switchMap } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { TokenService } from '../../../core/services/token.service';

@Component({
  selector: 'app-operator-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './operator-register.component.html',
  styleUrl: './operator-register.component.css',
})
export class OperatorRegisterComponent {
  readonly form;
  isSubmitting = false;
  selectedFile: File | null = null;
  successMessage = '';
  errorMessage = '';

  readonly documentTypes = [
    { label: 'License', value: 0 },
    { label: 'Registration', value: 1 },
    { label: 'Insurance', value: 2 },
    { label: 'Other', value: 3 },
  ];

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly tokenService: TokenService
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required, Validators.maxLength(20)]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      companyName: ['', [Validators.required, Validators.maxLength(200)]],
      licenseNumber: ['', [Validators.required, Validators.maxLength(100)]],
      documentType: [0, [Validators.required]],
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files?.[0] ?? null;
  }

  onSubmit(): void {
    this.successMessage = '';
    this.errorMessage = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;

    this.authService
      .operatorRegister({
        name: this.form.controls.name.value?.trim() ?? '',
        email: this.form.controls.email.value?.trim() ?? '',
        phone: this.form.controls.phone.value?.trim() ?? '',
        password: this.form.controls.password.value ?? '',
        companyName: this.form.controls.companyName.value?.trim() ?? '',
        licenseNumber: this.form.controls.licenseNumber.value?.trim() ?? '',
      })
      .pipe(
        switchMap((response) => {
          if (!this.selectedFile) {
            return of(null);
          }

          this.tokenService.setToken(response.token);
          this.tokenService.setRole(response.role);

          return from(this.fileToDataUrl(this.selectedFile)).pipe(
            switchMap((fileUrl) =>
              this.authService.uploadOperatorDocument({
                documentType: Number(this.form.controls.documentType.value ?? 0),
                fileUrl,
              })
            )
          );
        }),
        finalize(() => {
          this.isSubmitting = false;
          this.tokenService.clearAuth();
        })
      )
      .subscribe({
        next: () => {
          this.successMessage = 'Your application is under review';
          this.form.reset({ documentType: 0 });
          this.selectedFile = null;
        },
        error: (error) => {
          this.errorMessage = error?.error ?? 'Unable to submit operator registration.';
        },
      });
  }

  private fileToDataUrl(file: File) {
    return new Promise<string>((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => resolve(String(reader.result ?? ''));
      reader.onerror = () => reject(new Error('Failed to read file'));
      reader.readAsDataURL(file);
    });
  }
}

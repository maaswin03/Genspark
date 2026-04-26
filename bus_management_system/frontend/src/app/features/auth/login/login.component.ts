import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Inject, NgZone, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  readonly form;
  isSubmitting = false;
  showPassword = false;
  errorMessage = '';

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly cdr: ChangeDetectorRef,
    private readonly zone: NgZone,
    @Inject(PLATFORM_ID) private readonly platformId: object
  ) {
    this.form = this.fb.group({
      email: [this.getRememberedEmail(), [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [Boolean(this.getRememberedEmail())],
    });
  }

  onSubmit(): void {
    this.errorMessage = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;

    this.authService
      .login({
        email: this.form.controls.email.value?.trim() ?? '',
        password: this.form.controls.password.value ?? '',
      })
      .pipe(
        finalize(() => {
          this.zone.run(() => {
            this.isSubmitting = false;
            this.cdr.markForCheck();
          });
        })
      )
      .subscribe({
        next: (response) => {
          this.zone.run(() => {
            this.persistRememberedEmail();
            this.redirectByRole(response.role);
            this.cdr.markForCheck();
          });
        },
        error: (error) => {
          this.zone.run(() => {
            const backendMessage =
              typeof error?.error === 'string'
                ? error.error
                : typeof error?.error?.message === 'string'
                  ? error.error.message
                  : null;

            this.errorMessage =
              error?.status === 401
                ? backendMessage ?? 'Invalid email or password.'
                : backendMessage ?? 'Unable to sign in right now. Please try again.';

            this.isSubmitting = false;
            this.cdr.markForCheck();
          });
        },
      });
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  get emailInvalid(): boolean {
    const control = this.form.controls.email;
    return control.touched && control.invalid;
  }

  get passwordInvalid(): boolean {
    const control = this.form.controls.password;
    return control.touched && control.invalid;
  }

  private redirectByRole(role: string): void {
    const normalizedRole = role.toLowerCase();

    if (normalizedRole === 'admin') {
      this.router.navigateByUrl('/admin');
      return;
    }

    if (normalizedRole === 'operator') {
      this.router.navigateByUrl('/operator');
      return;
    }

    this.router.navigateByUrl('/home');
  }

  private persistRememberedEmail(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    const key = 'remembered_email';
    const email = this.form.controls.email.value?.trim() ?? '';

    if (this.form.controls.rememberMe.value) {
      localStorage.setItem(key, email);
      return;
    }

    localStorage.removeItem(key);
  }

  private getRememberedEmail(): string {
    if (!isPlatformBrowser(this.platformId)) {
      return '';
    }

    return localStorage.getItem('remembered_email') ?? '';
  }
}

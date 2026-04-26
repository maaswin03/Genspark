import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, timeout } from 'rxjs';
import {
  AuthResponse,
  LoginRequest,
  OperatorRegisterRequest,
  RegisterRequest,
  UploadDocumentRequest,
} from '../models/auth.models';
import { TokenService } from './token.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiBaseUrl = 'http://localhost:5131/api';

  constructor(
    private readonly http: HttpClient,
    private readonly tokenService: TokenService
  ) {}

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiBaseUrl}/auth/login`, request)
      .pipe(timeout({ first: 3000 }))
      .pipe(tap((response) => this.persistAuth(response)));
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiBaseUrl}/auth/register`, request);
  }

  operatorRegister(request: OperatorRegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiBaseUrl}/auth/operator/register`, request);
  }

  uploadOperatorDocument(request: UploadDocumentRequest): Observable<unknown> {
    return this.http.post(`${this.apiBaseUrl}/operator/documents`, request);
  }

  isAuthenticated(): boolean {
    return Boolean(this.tokenService.getToken());
  }

  getCurrentRole(): string | null {
    return this.tokenService.getRole();
  }

  hasRole(role: string): boolean {
    const currentRole = this.getCurrentRole();
    return currentRole?.toLowerCase() === role.toLowerCase();
  }

  logout(): void {
    this.tokenService.clearAuth();
  }

  private persistAuth(response: AuthResponse): void {
    this.tokenService.setToken(response.token);
    this.tokenService.setRole(response.role);
    this.tokenService.setUserProfile({
      name: response.name,
      email: response.email,
    });
  }
}

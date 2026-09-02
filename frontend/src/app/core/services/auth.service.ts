import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { API_BASE_URL } from '../config/api-config';
import { LoginRequest, LoginResponse } from '../models/auth.models';

const TOKEN_KEY = 'elms_token';
const USERNAME_KEY = 'elms_username';
const ROLE_KEY = 'elms_role';
const EMPLOYEE_ID_KEY = 'elms_employee_id';

@Injectable({ providedIn: 'root' })
export class AuthService {
  // Signals so components (and guards) can read auth state reactively,
  // without every consumer having to subscribe to an Observable by hand.
  private readonly _isAuthenticated = signal(this.hasToken());
  private readonly _role = signal(localStorage.getItem(ROLE_KEY));
  private readonly _username = signal(localStorage.getItem(USERNAME_KEY));

  readonly isAuthenticated = computed(() => this._isAuthenticated());
  readonly role = computed(() => this._role());
  readonly username = computed(() => this._username());

  constructor(private http: HttpClient, private router: Router) {}

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${API_BASE_URL}/auth/login`, credentials)
      .pipe(tap((response) => this.setSession(response)));
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USERNAME_KEY);
    localStorage.removeItem(ROLE_KEY);
    localStorage.removeItem(EMPLOYEE_ID_KEY);

    this._isAuthenticated.set(false);
    this._role.set(null);
    this._username.set(null);

    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  getEmployeeId(): number | null {
    const value = localStorage.getItem(EMPLOYEE_ID_KEY);
    return value ? Number(value) : null;
  }

  private setSession(response: LoginResponse): void {
    localStorage.setItem(TOKEN_KEY, response.token);
    localStorage.setItem(USERNAME_KEY, response.username);
    localStorage.setItem(ROLE_KEY, response.role);

    if (response.employeeId !== null) {
      localStorage.setItem(EMPLOYEE_ID_KEY, String(response.employeeId));
    }

    this._isAuthenticated.set(true);
    this._role.set(response.role);
    this._username.set(response.username);
  }

  private hasToken(): boolean {
    return !!localStorage.getItem(TOKEN_KEY);
  }
}

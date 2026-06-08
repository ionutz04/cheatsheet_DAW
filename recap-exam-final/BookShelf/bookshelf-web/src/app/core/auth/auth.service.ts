import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
import { AuthResponse, LoginRequest } from './auth.models';

const TOKEN_KEY = 'bookshelf_access_token';

// ASP.NET Identity encodes ClaimTypes.Role under this URI in the JWT payload.
const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
const NAMEID_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';

function parseClaim(token: string | null, claim: string): string[] {
  if (!token) return [];
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const raw = payload[claim];
    if (!raw) return [];
    return Array.isArray(raw) ? raw : [raw];
  } catch {
    return [];
  }
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _token = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  readonly token = computed(() => this._token());
  readonly isAuthenticated = computed(() => !!this._token());
  readonly isAdmin = computed(() => parseClaim(this._token(), ROLE_CLAIM).includes('Admin'));
  readonly userId = computed(() => parseClaim(this._token(), NAMEID_CLAIM)[0] ?? null);

  constructor(private readonly http: HttpClient) {}

  login(request: LoginRequest) {
    return this.http.post<AuthResponse>('/api/auth/login', request).pipe(
      tap((res) => this.setToken(res.token))
    );
  }

  // BookShelf register requires a FullName; derive a simple one from the email.
  register(request: LoginRequest) {
    const fullName = request.email.split('@')[0] || request.email;
    return this.http
      .post<AuthResponse>('/api/auth/register', { fullName, email: request.email, password: request.password })
      .pipe(tap((res) => this.setToken(res.token)));
  }

  logout() {
    this.clearToken();
  }

  private setToken(token: string) {
    localStorage.setItem(TOKEN_KEY, token);
    this._token.set(token);
  }

  private clearToken() {
    localStorage.removeItem(TOKEN_KEY);
    this._token.set(null);
  }
}

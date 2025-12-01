// src/app/Core/Services/auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable, BehaviorSubject, throwError, of } from 'rxjs';
import { tap, catchError, switchMap, map } from 'rxjs/operators';

interface LoginRequest { Email: string; Password: string; }
interface RegisterRequest { Name: string; Email: string; Password: string; }

function getStoredToken(): string | null {
  if (typeof window === 'undefined') return null;
  return localStorage.getItem('token');
}
function getStoredUser(): any | null {
  if (typeof window === 'undefined') return null;
  const raw = localStorage.getItem('user');
  return raw ? JSON.parse(raw) : null;
}
function getStoredRole(): string | null {
  if (typeof window === 'undefined') return null;
  return localStorage.getItem('role');
}
function normalizeRole(r: any): 'Admin' | 'Member' {
  const s = (r ?? '').toString().trim().toLowerCase();
  return s === 'admin' ? 'Admin' : 'Member';
}

/** --- Helpers for decoding JWT and extracting a role claim --- */
function decodeJwt(token: string): any {
  try {
    const base64 = token.split('.')[1];
    if (!base64) return null;
    const json = typeof atob !== 'undefined'
      ? atob(base64.replace(/-/g, '+').replace(/_/g, '/'))
      : Buffer.from(base64, 'base64').toString('binary');
    // decodeURIComponent(escape(...)) handles UTF-8 in some browsers
    const safe = decodeURIComponent(escape(json));
    return JSON.parse(safe);
  } catch {
    return null;
  }
}
function roleFromJwtPayload(p: any): 'Admin' | 'Member' {
  if (!p || typeof p !== 'object') return 'Member';
  // Common ASP.NET role claim keys
  const raw =
    p.role ??
    p['roles'] ??
    p['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
  const val = Array.isArray(raw) ? raw[0] : raw;
  return normalizeRole(val);
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private base = environment.apiBaseUrl;

  private _isLoggedIn$ = new BehaviorSubject<boolean>(!!getStoredToken());
  isLoggedIn$ = this._isLoggedIn$.asObservable();

  private _isAdmin$ = new BehaviorSubject<boolean>(getStoredRole() === 'Admin');
  isAdmin$ = this._isAdmin$.asObservable();

  constructor(private http: HttpClient) {
    const token = getStoredToken();
    const storedUser = getStoredUser();
    let role = getStoredRole();

    if (typeof window !== 'undefined' && (token || storedUser)) {
      this._isLoggedIn$.next(!!token);

      // If role wasn't stored, try to derive it from the token
      if (!role && token) {
        role = roleFromJwtPayload(decodeJwt(token));
        localStorage.setItem('role', role);
      }
      this._isAdmin$.next(role === 'Admin');
    }
  }

  /**
   * LOGIN — POST /api/Auth/login
   * Expects a JSON response with at least a { token }.
   * Optionally accepts { user, role }.
   *
   * If the response has no numeric Id on user, we fetch /api/User/GetUsers
   * and store the matching user (so LoanCreate sees a proper UserId).
   */
  login(body: LoginRequest): Observable<any> {
    return this.http.post<{ token?: string; user?: any; role?: any }>(
      `${this.base}/api/Auth/login`,
      body
    ).pipe(
      switchMap(res => {
        const token = res?.token;
        if (!token) throw new Error('Missing token in login response');

        const jwtPayload = decodeJwt(token);
        const roleFromToken = roleFromJwtPayload(jwtPayload);

        // Start with whatever the API returned (or fallback to email)
        let user = res?.user ?? getStoredUser() ?? { email: body.Email };
        const hasId = !!(user && (user.id ?? user.Id ?? user.userId ?? user.UserId));
        const baseRole = normalizeRole(res?.role ?? user?.role ?? roleFromToken);

        // No Id on user → look it up so other pages (Loans) have a numeric Id
        if (!hasId) {
          return this.http.get<any[]>(`${this.base}/api/User/GetUsers`).pipe(
            map(list => {
              const emailLc = body.Email.toLowerCase();
              const found = (list || []).find(x =>
                (x.email ?? x.Email ?? '').toString().toLowerCase() === emailLc
              );
              user = found ?? user;
              const role = normalizeRole(res?.role ?? user?.role ?? roleFromToken);
              return { token, user, role };
            }),
            // If lookup fails, proceed anyway so app remains logged in
            catchError(() => of({ token, user, role: baseRole }))
          );
        }

        // Already have an Id → continue
        return of({ token, user, role: baseRole });
      }),
      tap(({ token, user, role }) => {
        if (typeof window !== 'undefined') {
          localStorage.setItem('token', token);
          localStorage.setItem('user', JSON.stringify(user));
          localStorage.setItem('role', role);
        }
        this._isLoggedIn$.next(true);
        this._isAdmin$.next(role === 'Admin');
      }),
      catchError(err => {
        if (typeof window !== 'undefined') {
          localStorage.removeItem('token');
          localStorage.removeItem('user');
          localStorage.removeItem('role');
        }
        this._isLoggedIn$.next(false);
        this._isAdmin$.next(false);
        return throwError(() => err);
      })
    );
  }

  /**
   * REGISTER — POST /api/User/create
   * Backend often returns text/plain here; accept text to avoid parse errors.
   */
  register(body: RegisterRequest): Observable<any> {
    return this.http.post(`${this.base}/api/User/create`, body, { responseType: 'text' });
  }
  // Backwards-compat alias if you're already calling createUser()
  createUser(body: RegisterRequest): Observable<any> {
    return this.register(body);
  }

  getCurrentUser(): any | null { return getStoredUser(); }
  getRole(): string | null { return getStoredRole(); }
  getToken(): string | null { return getStoredToken(); }

  logout(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      localStorage.removeItem('role');
    }
    this._isLoggedIn$.next(false);
    this._isAdmin$.next(false);
  }
}

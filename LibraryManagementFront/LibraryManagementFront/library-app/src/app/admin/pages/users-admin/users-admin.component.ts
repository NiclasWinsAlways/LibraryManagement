import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { FormBuilder } from '@angular/forms';
import { environment } from '../../../../environments/environment';

type RawUser = any;

export interface UserRow {
  id: number | string;
  name: string;
  email: string;
  role: 'Admin' | 'Member';
}

function tokenHeaders(): HttpHeaders {
  const t = localStorage.getItem('token') || '';
  return t ? new HttpHeaders({ Authorization: `Bearer ${t}` }) : new HttpHeaders();
}
function normRole(x: any): 'Admin' | 'Member' {
  const s = (x ?? '').toString().trim().toLowerCase();
  return s === 'admin' ? 'Admin' : 'Member';
}
function pick(o: any, keys: string[], fallback?: any) {
  for (const k of keys) if (o && o[k] !== undefined && o[k] !== null) return o[k];
  return fallback;
}
function toUser(u: RawUser): UserRow {
  return {
    id: pick(u, ['id', 'Id', 'userId']),
    name: pick(u, ['name', 'Name', 'fullName'], ''),
    email: pick(u, ['email', 'Email'], ''),
    role: normRole(pick(u, ['role', 'Role', 'userRole'], 'Member')),
  };
}

@Component({
  selector: 'app-users-admin',
  templateUrl: './users-admin.component.html',
  styleUrls: ['./users-admin.component.css']
})
export class UsersAdminComponent implements OnInit {
  api = environment.apiBaseUrl;

  loading = false;
  error: string | null = null;

  users: UserRow[] = [];
  filtered: UserRow[] = [];
  q = '';

  constructor(private http: HttpClient, private fb: FormBuilder) {}

  ngOnInit(): void {
    this.load();
  }

  private headers(): HttpHeaders { return tokenHeaders(); }

  load(): void {
    this.loading = true; this.error = null;
    this.http.get<any[]>(`${this.api}/api/User/GetUsers`, { headers: this.headers() })
      .subscribe({
        next: (res) => {
          const arr = Array.isArray(res) ? res : (res as any)?.items ?? [];
          this.users = arr.map(toUser);
          this.applyFilter();
          this.loading = false;
        },
        error: (err) => { this.loading = false; this.error = 'Failed to load users.'; console.error(err); }
      });
  }

  applyFilter(): void {
    const ql = (this.q ?? '').toString().toLowerCase();
    this.filtered = this.users.filter(u =>
      !ql ||
      (u.name ?? '').toLowerCase().includes(ql) ||
      (u.email ?? '').toLowerCase().includes(ql)
    );
  }

  setRole(u: UserRow, role: 'Admin' | 'Member'): void {
    if (u.role === role) return;

    this.loading = true; this.error = null;
    // Try a few common payload shapes that backends use
    const attempts = [
      { Role: role },
      { role },
      { IsAdmin: role === 'Admin' },
    ];

    const tryPut = (i: number) => {
      if (i >= attempts.length) {
        this.loading = false;
        this.error = 'Failed to update role.';
        return;
      }
      this.http.put(`${this.api}/api/User/${u.id}`, attempts[i], {
        headers: this.headers(),
        responseType: 'text'
      }).subscribe({
        next: () => { this.loading = false; this.load(); },
        error: (err) => {
          // If backend rejects shape (400/415/etc.), try next shape
          if (i < attempts.length - 1) return tryPut(i + 1);
          console.error('[USERS] role update failed', err);
          this.loading = false;
          this.error = err?.error || 'Failed to update role.';
        }
      });
    };

    tryPut(0);
  }

  trackById(_: number, u: UserRow) { return u.id; }
}

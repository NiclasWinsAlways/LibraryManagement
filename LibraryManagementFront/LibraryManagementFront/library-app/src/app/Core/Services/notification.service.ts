import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface NotificationDto {
  id: number;
  userId: number;
  message: string;
  isRead: boolean;
  createdAt: string; // ISO from backend
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly base = `${environment.apiBaseUrl}/api/Notification`;

  constructor(private http: HttpClient) {}

  private authHeaders(): HttpHeaders {
    const token = localStorage.getItem('token') || '';
    return token
      ? new HttpHeaders({ Authorization: `Bearer ${token}` })
      : new HttpHeaders();
  }

  /** GET /api/Notification/user/{userId} */
  getForUser(userId: number): Observable<NotificationDto[]> {
    return this.http.get<NotificationDto[]>(`${this.base}/user/${userId}`, {
      headers: this.authHeaders(),
    });
  }

  /** POST /api/Notification/create */
  create(body: { UserId: number; Message: string }): Observable<any> {
    return this.http.post(`${this.base}/create`, body, {
      headers: this.authHeaders(),
      responseType: 'text',
    });
  }

  /** POST /api/Notification/{id}/read */
  markRead(id: number): Observable<any> {
    return this.http.post(`${this.base}/${id}/read`, {}, {
      headers: this.authHeaders(),
      responseType: 'text',
    });
  }

  /** PUT /api/Notification/{id} */
  update(n: NotificationDto): Observable<any> {
    const body = {
      id: n.id,
      userId: n.userId,
      message: n.message,
      isRead: n.isRead,
    };
    return this.http.put(`${this.base}/${n.id}`, body, {
      headers: this.authHeaders(),
      responseType: 'text',
    });
  }

  /** DELETE /api/Notification/{id} */
  delete(id: number): Observable<any> {
    return this.http.delete(`${this.base}/${id}`, {
      headers: this.authHeaders(),
      responseType: 'text',
    });
  }

  // ---------- OPTIONAL BACKEND SEND ENDPOINTS ----------
  /** POST /api/Notification/send-email { To, Subject, Body } */
  sendEmail(body: { To: string; Subject: string; Body: string }): Observable<any> {
    return this.http.post(`${this.base}/send-email`, body, {
      headers: this.authHeaders(),
      responseType: 'text',
    }).pipe(
      // if endpoint doesn't exist, let caller fallback
      catchError(err => of({ _error: err, _missing: true }))
    );
  }

  /** POST /api/Notification/send-sms { To, Body } */
  sendSms(body: { To: string; Body: string }): Observable<any> {
    return this.http.post(`${this.base}/send-sms`, body, {
      headers: this.authHeaders(),
      responseType: 'text',
    }).pipe(
      catchError(err => of({ _error: err, _missing: true }))
    );
  }

  /** POST /api/Notification/run-due-scan — trigger server job now */
  runDueScan(): Observable<{ ok: boolean }> {
    return this.http.post<{ ok: boolean }>(`${this.base}/run-due-scan`, {}, {
      headers: this.authHeaders(),
    });
  }
}

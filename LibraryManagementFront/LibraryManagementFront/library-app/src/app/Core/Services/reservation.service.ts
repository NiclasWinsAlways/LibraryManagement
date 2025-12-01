import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable, catchError, of } from 'rxjs';

export interface ReservationDto {
  id: number;
  bookId: number;
  bookTitle?: string | null;
  userId: number;
  userName?: string | null;
  createdAt: string;   // backend DateTime as ISO string
  status: string;      // "Active", "Cancelled", etc.
}

@Injectable({ providedIn: 'root' })
export class ReservationService {
  private base = `${environment.apiBaseUrl}/api/Reservation`;

  constructor(private http: HttpClient) {}

  private headers(): HttpHeaders {
    const t = localStorage.getItem('token') || '';
    return t
      ? new HttpHeaders({ Authorization: `Bearer ${t}` })
      : new HttpHeaders();
  }

  /** USER: POST /api/Reservation/create */
  create(body: { BookId: number; UserId: number }): Observable<any> {
    return this.http.post(`${this.base}/create`, body, {
      headers: this.headers(),
      responseType: 'text',
    });
  }

  /** USER: GET /api/Reservation/getReservations?userId={id} */
  getMine(userId: number): Observable<ReservationDto[]> {
    return this.http
      .get<ReservationDto[]>(`${this.base}/getReservations`, {
        headers: this.headers(),
        params: { userId: String(userId) },
      })
      .pipe(catchError(() => of([])));
  }

  /** ADMIN: GET /api/Reservation/getReservations (all) */
  getAll(): Observable<ReservationDto[]> {
    return this.http
      .get<ReservationDto[]>(`${this.base}/getReservations`, {
        headers: this.headers(),
      })
      .pipe(catchError(() => of([])));
  }

  /** ADMIN: GET /api/Reservation/{id} */
  getById(id: number): Observable<ReservationDto> {
    return this.http.get<ReservationDto>(`${this.base}/${id}`, {
      headers: this.headers(),
    });
  }

  /** ADMIN: PUT /api/Reservation/{id} */
  update(r: ReservationDto): Observable<any> {
    const body = {
      id: r.id,
      bookId: r.bookId,
      userId: r.userId,
      status: r.status,
    };

    return this.http.put(`${this.base}/${r.id}`, body, {
      headers: this.headers(),
      responseType: 'text',
    });
  }

  /** ADMIN: DELETE /api/Reservation/{id} */
  delete(id: number): Observable<any> {
    return this.http.delete(`${this.base}/${id}`, {
      headers: this.headers(),
      responseType: 'text',
    });
  }

  /** USER/ADMIN: POST /api/Reservation/{id}/cancel */
  cancel(id: number): Observable<any> {
    return this.http.post(`${this.base}/${id}/cancel`, {}, {
      headers: this.headers(),
      responseType: 'text',
    });
  }
}

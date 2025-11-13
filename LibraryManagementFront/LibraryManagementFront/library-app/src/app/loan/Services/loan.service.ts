import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable, catchError, of } from 'rxjs';

export interface LoanDto {
  id: number;
  bookId: number;
  userId: number;
  startDate: string;
  endDate: string;
  status: string;
}

@Injectable({ providedIn: 'root' })
export class LoanService {
  private readonly base = `${environment.apiBaseUrl}/api/Loan`;

  constructor(private http: HttpClient) {}

  private authHeaders(): HttpHeaders {
    const token = localStorage.getItem('token') || '';
    return token
      ? new HttpHeaders({ Authorization: `Bearer ${token}` })
      : new HttpHeaders();
  }

  /** GET /api/Loan/getloans */
  getAll(): Observable<LoanDto[]> {
    return this.http
      .get<LoanDto[]>(`${this.base}/getloans`, { headers: this.authHeaders() })
      .pipe(
        catchError(err => {
          console.warn('Loan fetch failed, returning empty list', err);
          return of([] as LoanDto[]);
        })
      );
  }

  /** POST /api/Loan/create */
  create(body: {
    BookId: number;
    UserId: number;
    EndDate: string;
    Status: string;
  }): Observable<any> {
    return this.http.post(`${this.base}/create`, body, {
      headers: this.authHeaders(),
      responseType: 'text',
    });
  }

  /** POST /api/Loan/{id}/return */
  markReturned(id: number): Observable<any> {
    return this.http.post(`${this.base}/${id}/return`, {}, {
      headers: this.authHeaders(),
      responseType: 'text',
    });
  }
}

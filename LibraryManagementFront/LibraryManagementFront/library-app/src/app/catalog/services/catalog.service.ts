// src/app/Core/Services/catalog.service.ts
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, switchMap, map, catchError, of } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export type BorrowStatus = 'all' | 'available' | 'borrowed';

export interface CatalogSearchParams {
  title?: string;
  author?: string;
  genre?: string;
  isbn?: string;
  status?: BorrowStatus;
  page?: number;
  pageSize?: number;
}

export interface BookDto {
  id: number;
  title: string;
  genre: string;
  author: string;
  isbn: string;
  copiesAvailable: number;
  isAvailable: boolean;
}

@Injectable({ providedIn: 'root' })
export class CatalogService {
  private readonly _params = new BehaviorSubject<CatalogSearchParams>({
    status: 'all',
    page: 1,
    pageSize: 10,
  });

  // --- match your Swagger ---
  private readonly getAllUrl = `${environment.apiBaseUrl}/api/Book/getbooks`;
  private readonly byIdUrl    = `${environment.apiBaseUrl}/api/Book`; // /{id}
  private readonly createUrl  = `${environment.apiBaseUrl}/api/Book/create`;
  private readonly updateUrl  = `${environment.apiBaseUrl}/api/Book`; // /{id} PUT
  private readonly deleteUrl  = `${environment.apiBaseUrl}/api/Book`; // /{id} DELETE

  constructor(private http: HttpClient) {}

  get params$(): Observable<CatalogSearchParams> { return this._params.asObservable(); }
  get currentParams(): CatalogSearchParams { return this._params.value; }

  setParams(patch: Partial<CatalogSearchParams>): void {
    this._params.next({ ...this._params.value, ...patch });
  }

  /** Force a re-fetch with current filters/paging (used after returns/creates/updates). */
  refresh(): void {
    this._params.next({ ...this._params.value });
  }

  // --- raw fetch (no params; API returns all books) ---
  private fetchAllBooks(): Observable<BookDto[]> {
    return this.http.get<BookDto[]>(this.getAllUrl).pipe(
      catchError(err => {
        console.warn('Catalog fetch failed, returning empty list:', err);
        return of([] as BookDto[]);
      })
    );
  }

  // --- local filtering + pagination since API doesn’t support it ---
  private applyFilters(items: BookDto[], p: CatalogSearchParams): BookDto[] {
    const q = (s?: string) => (s ?? '').toLowerCase();
    let list = items;

    if (p.title)  list = list.filter(b => q(b.title).includes(q(p.title)));
    if (p.author) list = list.filter(b => q(b.author).includes(q(p.author)));
    if (p.genre)  list = list.filter(b => q(b.genre).includes(q(p.genre)));
    if (p.isbn)   list = list.filter(b => q(b.isbn).includes(q(p.isbn)));

    if (p.status && p.status !== 'all') {
      const wantAvailable = p.status === 'available';
      list = list.filter(b =>
        typeof b.isAvailable === 'boolean'
          ? b.isAvailable === wantAvailable
          : ((b.copiesAvailable ?? 0) > 0) === wantAvailable
      );
    }
    return list;
  }

  // what the component subscribes to
  results$ = this._params.pipe(
    switchMap(p =>
      this.fetchAllBooks().pipe(
        map(all => {
          const filtered = this.applyFilters(all, p);
          const page = Math.max(1, p.page ?? 1);
          const pageSize = Math.max(1, p.pageSize ?? 10);
          const start = (page - 1) * pageSize;
          const paged = filtered.slice(start, start + pageSize);
          return {
            items: paged,
            total: filtered.length,
            page,
            pageSize,
          };
        })
      )
    )
  );

  // --- optional helpers for admin screens (matching Swagger) ---
  getById(id: number) {
    return this.http.get<BookDto>(`${this.byIdUrl}/${id}`);
  }
  create(book: Partial<BookDto>) {
    return this.http.post(this.createUrl, book);
  }
  update(id: number, book: Partial<BookDto>) {
    return this.http.put(`${this.updateUrl}/${id}`, book);
  }
  remove(id: number) {
    return this.http.delete(`${this.deleteUrl}/${id}`);
  }
}

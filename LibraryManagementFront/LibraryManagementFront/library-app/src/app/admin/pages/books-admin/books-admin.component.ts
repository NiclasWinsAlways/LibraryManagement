import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

interface BookDto {
  id: number;
  title: string;
  author: string;
  genre: string;
  isbn: string;
  copiesAvailable: number;
}

function tokenHeaders(): HttpHeaders {
  const t = localStorage.getItem('token') || '';
  return t ? new HttpHeaders({ Authorization: `Bearer ${t}` }) : new HttpHeaders();
}

// Map backend (camel or Pascal) → UI model
function toBook(x: any): BookDto {
  return {
    id: x.id ?? x.Id,
    title: x.title ?? x.Title ?? '',
    author: x.author ?? x.Author ?? '',
    genre: x.genre ?? x.Genre ?? '',
    isbn: x.isbn ?? x.ISBN ?? '',
    copiesAvailable: x.copiesAvailable ?? x.CopiesAvailable ?? 0,
  } as BookDto;
}

@Component({
  selector: 'app-books-admin',
  templateUrl: './books-admin.component.html',
  styleUrls: ['./books-admin.component.css']
})
export class BooksAdminComponent implements OnInit {
  api = environment.apiBaseUrl;

  loading = false;
  error: string | null = null;

  // list
  books: BookDto[] = [];
  filtered: BookDto[] = [];
  q = '';

  // create/edit
  createForm!: FormGroup;
  editForm!: FormGroup;
  editing: BookDto | null = null;

  constructor(private fb: FormBuilder, private http: HttpClient) {}

  ngOnInit(): void {
    this.createForm = this.fb.group({
      Title: ['', [Validators.required, Validators.minLength(1)]],
      Author: ['', [Validators.required, Validators.minLength(1)]],
      Genre: [''],
      ISBN: [''],
      CopiesAvailable: [0, [Validators.min(0)]],
    });

    this.editForm = this.fb.group({
      Title: ['', [Validators.required, Validators.minLength(1)]],
      Author: ['', [Validators.required, Validators.minLength(1)]],
      Genre: [''],
      ISBN: [''],
      CopiesAvailable: [0, [Validators.min(0)]],
    });

    this.load();
  }

  /** template helper */
  t(name: string) { return this.createForm.get(name); }

  /** GET /api/Book/getbooks */
  load(): void {
    this.loading = true; this.error = null;
    const url = `${this.api}/api/Book/getbooks`;
    this.http.get<any[]>(url, { headers: tokenHeaders() })
      .subscribe({
        next: (res) => {
          const arr = Array.isArray(res) ? res : [];
          this.books = arr.map(toBook);
          this.applyFilter();
          this.loading = false;
          console.log('[BOOKS] loaded', this.books.length);
        },
        error: (err) => {
          console.error('[BOOKS] load failed', err);
          this.loading = false;
          this.error = 'Failed to load books.';
        }
      });
  }

  /** filter client-side */
  applyFilter(): void {
    const ql = (this.q ?? '').toString().toLowerCase();
    this.filtered = this.books.filter(b =>
      !ql ||
      (b.title ?? '').toLowerCase().includes(ql) ||
      (b.author ?? '').toLowerCase().includes(ql) ||
      (b.genre ?? '').toLowerCase().includes(ql) ||
      (b.isbn ?? '').toLowerCase().includes(ql)
    );
  }

  /** POST /api/Book/create  (PascalCase body) */
  create(): void {
    if (this.loading) return;
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }
    const payload = this.createForm.value; // PascalCase
    const url = `${this.api}/api/Book/create`;

    this.loading = true; this.error = null;
    this.http.post(url, payload, { headers: tokenHeaders(), responseType: 'text' })
      .subscribe({
        next: () => {
          this.loading = false;
          this.createForm.reset({ Title: '', Author: '', Genre: '', ISBN: '', CopiesAvailable: 0 });
          this.load();
        },
        error: (err) => {
          console.error('[BOOKS] create failed', err);
          this.loading = false;
          this.error = err?.error || 'Could not create book.';
        }
      });
  }

  /** open edit drawer */
  startEdit(b: BookDto): void {
    this.editing = b;
    this.editForm.reset({
      Title: b.title,
      Author: b.author,
      Genre: b.genre,
      ISBN: b.isbn,
      CopiesAvailable: b.copiesAvailable ?? 0
    });
  }

  cancelEdit(): void {
    this.editing = null;
    this.editForm.reset();
  }

  /** PUT /api/Book/{id} */
  saveEdit(): void {
    if (!this.editing) return;
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }
    const id = this.editing.id;
    const body = this.editForm.value; // PascalCase
    const url = `${this.api}/api/Book/${id}`;

    this.loading = true; this.error = null;
    this.http.put(url, body, { headers: tokenHeaders(), responseType: 'text' })
      .subscribe({
        next: () => {
          this.loading = false;
          this.editing = null;
          this.load();
        },
        error: (err) => {
          console.error('[BOOKS] update failed', err);
          this.loading = false;
          this.error = err?.error || 'Could not update book.';
        }
      });
  }

  /** DELETE /api/Book/{id} */
  delete(b: BookDto): void {
    if (!confirm(`Delete "${b.title}" by ${b.author}?`)) return;
    const url = `${this.api}/api/Book/${b.id}`;

    this.loading = true; this.error = null;
    this.http.delete(url, { headers: tokenHeaders(), responseType: 'text' })
      .subscribe({
        next: () => {
          this.loading = false;
          this.load();
        },
        error: (err) => {
          console.error('[BOOKS] delete failed', err);
          this.loading = false;
          this.error = err?.error || 'Could not delete book.';
        }
      });
  }

  trackById(_: number, b: BookDto) { return b.id; }
}

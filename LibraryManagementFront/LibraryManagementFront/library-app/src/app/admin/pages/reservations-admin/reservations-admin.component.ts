import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import {
  ReservationService,
  ReservationDto,
} from '../../../Core/Services/reservation.service';

@Component({
  selector: 'app-reservations-admin',
  templateUrl: './reservations-admin.component.html',
  styleUrls: ['./reservations-admin.component.css'],
})
export class ReservationsAdminComponent implements OnInit {
  loading = false;
  error = '';
  q = '';

  all: ReservationDto[] = [];
  filtered: ReservationDto[] = [];

  editForm!: FormGroup;
  editing: ReservationDto | null = null;

  constructor(
    private fb: FormBuilder,
    private api: ReservationService
  ) {}

  ngOnInit(): void {
    this.editForm = this.fb.group({
      Status: ['', [Validators.required]],
    });

    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';

    this.api.getAll().subscribe({
      next: (res) => {
        this.all = res || [];
        this.applyFilter();
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.error = 'Failed to load reservations.';
        this.loading = false;
      },
    });
  }

  applyFilter(): void {
    const q = (this.q || '').toLowerCase();

    if (!q) {
      this.filtered = this.all.slice();
      return;
    }

    this.filtered = this.all.filter((r) =>
      String(r.id).includes(q) ||
      String(r.userId).includes(q) ||
      (r.userName || '').toLowerCase().includes(q) ||
      String(r.bookId).includes(q) ||
      (r.bookTitle || '').toLowerCase().includes(q) ||
      (r.status || '').toLowerCase().includes(q)
    );
  }

  trackById(_index: number, r: ReservationDto): number {
    return r.id;
  }

  startEdit(r: ReservationDto): void {
    this.editing = r;
    this.editForm.reset({
      Status: r.status || 'Active',
    });
  }

  cancelEdit(): void {
    this.editing = null;      // 👈 hides drawer (*ngIf="editing as e")
    this.editForm.reset();
  }

  saveEdit(): void {
    if (!this.editing || this.editForm.invalid) return;

    const v = this.editForm.value;

    const updated: ReservationDto = {
      ...this.editing,
      status: v.Status,
    };

    this.loading = true;
    this.error = '';

    this.api.update(updated).subscribe({
      next: () => {
        const idx = this.all.findIndex(x => x.id === updated.id);
        if (idx !== -1) {
          this.all[idx] = { ...this.all[idx], ...updated };
        }
        this.applyFilter();
        this.cancelEdit();    // 👈 close after save
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.error = 'Failed to save reservation.';
        this.loading = false;
      },
    });
  }

  delete(r: ReservationDto): void {
    if (!confirm(`Delete reservation #${r.id}?`)) return;

    this.loading = true;
    this.error = '';

    this.api.delete(r.id).subscribe({
      next: () => {
        this.all = this.all.filter(x => x.id !== r.id);
        this.applyFilter();
        if (this.editing?.id === r.id) {
          this.cancelEdit();
        }
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.error = 'Failed to delete reservation.';
        this.loading = false;
      },
    });
  }
}

// src/app/auth/pages/my-reservations/my-reservations.component.ts
import { Component, OnInit } from '@angular/core';
import { ReservationService, ReservationDto } from '../../../Core/Services/reservation.service';
import { AuthService } from '../../../Core/Services/auth.service';

@Component({
  selector: 'app-my-reservations',
  templateUrl: './my-reservations.component.html',
  styleUrls: ['./my-reservations.component.css']
})
export class MyReservationsComponent implements OnInit {
  loading = false;
  error: string | null = null;
  items: ReservationDto[] = [];

  constructor(private resv: ReservationService, private auth: AuthService) {}

  ngOnInit(): void {
    const u = this.auth.getCurrentUser();
    const userId = u?.id ?? u?.Id ?? 0;
    if (!userId) { this.error = 'You must be logged in.'; return; }

    this.loading = true;
    this.resv.getMine(userId).subscribe({
      next: list => { this.items = list ?? []; this.loading = false; },
      error: err => { this.error = 'Failed to load reservations.'; this.loading = false; console.error(err); }
    });
  }

  /** show yyyy-MM-dd from CreatedAt (backend PascalCase) */
  displayDate(r: ReservationDto): string {
    const anyR = r as any;
    const raw = anyR.createdAt ?? anyR.CreatedAt ?? null;
    if (!raw) return '';
    const d = new Date(raw);
    return isNaN(d.getTime()) ? '' : d.toISOString().slice(0, 10);
  }

  cancel(r: ReservationDto) {
    if (!confirm('Cancel this reservation?')) return;
    this.loading = true;
    this.resv.cancel(r.id).subscribe({
      next: () => {
        this.items = this.items.filter(x => x.id !== r.id);
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.error = 'Failed to cancel reservation.';
        console.error(err);
      }
    });
  }
}

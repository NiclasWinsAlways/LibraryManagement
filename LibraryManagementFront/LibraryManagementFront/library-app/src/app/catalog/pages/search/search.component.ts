import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { Router } from '@angular/router';

import { CatalogService } from '../../services/catalog.service';
import { ReservationService } from '../../../Core/Services/reservation.service';
import { AuthService } from '../../../Core/Services/auth.service';

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.css'],
})
export class SearchComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  form!: FormGroup;
  results$!: any;

  /** prevent double-reserve clicks */
  reserving = new Set<number>();

  constructor(
    private fb: FormBuilder,
    private catalog: CatalogService,
    private router: Router,
    private reservations: ReservationService,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      title: [''],
      author: [''],
      genre: [''],
      isbn: [''],
      status: ['all'],
    });

    this.results$ = this.catalog.results$;

    // initial load
    this.catalog.setParams(this.form.value || {});

    // live filter
    this.form.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe(v => this.catalog.setParams(v || {}));
  }

  changePage(delta: number) {
    const next = Math.max(1, (this.catalog.currentParams.page ?? 1) + delta);
    this.catalog.setParams({ page: next });
  }

  // Loan flow
  onLoan(book: any) {
    this.router.navigate(['/loans/new', book.id]);
  }

  // Reserve flow (called when book is borrowed)
  onReserve(book: any) {
    const user = this.auth.getCurrentUser();
    const userId = user?.id ?? user?.Id ?? 0;
    if (!userId) {
      alert('You must be logged in to reserve.');
      return;
    }
    if (!book?.id || this.reserving.has(book.id)) return;

    this.reserving.add(book.id);
    this.reservations.create({ BookId: book.id, UserId: userId }).subscribe({
      next: () => {
        alert('Reservation placed.');
        // refresh the catalog so copies/status reflect any backend changes
        this.catalog.refresh();
        this.reserving.delete(book.id);
      },
      error: (err) => {
        console.error('[RESERVE] failed', err);
        alert(err?.error || 'Could not create reservation.');
        this.reserving.delete(book.id);
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}

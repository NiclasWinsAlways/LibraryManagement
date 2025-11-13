import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';

import { LoanService } from '../../../loan/Services/loan.service';
import { CatalogService } from '../../../catalog/services/catalog.service';

type LoanRaw = any;

export interface Loan {
  id: number | string;
  userName?: string;
  userEmail?: string;
  bookTitle?: string;
  loanDate?: string;
  dueDate?: string;
  returnDate?: string | null;
  status: 'Active' | 'Overdue' | 'Returned';
}

function pick(o: any, keys: string[], fallback?: any) {
  for (const k of keys) {
    if (o && o[k] !== undefined && o[k] !== null) return o[k];
  }
  return fallback;
}

function normalizeLoan(x: LoanRaw): Loan {
  const id         = pick(x, ['id', 'loanId', 'Id']);
  const userName   = pick(x, ['userName', 'UserName', 'borrowerName']);
  const userEmail  = pick(x, ['userEmail', 'UserEmail', 'email']);
  const bookTitle  = pick(x, ['bookTitle', 'BookTitle', 'title']);
  const loanDate   = pick(x, ['loanDate', 'LoanDate', 'borrowedAt', 'startDate', 'StartDate']);
  const dueDate    = pick(x, ['dueDate', 'DueDate', 'endDate', 'EndDate']);
  const returnDate = pick(x, ['returnDate', 'ReturnDate', 'returnedAt'], null);
  const rawStatus  = pick(x, ['status', 'Status'], '').toString();

  // Normalize status from backend values into our union
  let status: Loan['status'] = 'Active';

  if (returnDate || rawStatus.toLowerCase() === 'returned' || rawStatus === 'Afleveret') {
    status = 'Returned';
  } else if (rawStatus.toLowerCase() === 'overdue' || rawStatus === 'Forfalden') {
    status = 'Overdue';
  } else {
    // If date-based overdue:
    if (dueDate && !returnDate && new Date(dueDate) < new Date()) {
      status = 'Overdue';
    } else {
      status = 'Active';
    }
  }

  return {
    id,
    userName,
    userEmail,
    bookTitle,
    loanDate,
    dueDate,
    returnDate,
    status,
  };
}

@Component({
  selector: 'app-loans-admin',
  standalone: false,
  templateUrl: './loans-admin.component.html',
  styleUrls: ['./loans-admin.component.css'],
})
export class LoansAdminComponent implements OnInit {
  loading = false;
  error: string | null = null;

  loans: Loan[] = [];
  filtered: Loan[] = [];

  filter!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private loanService: LoanService,
    private catalog: CatalogService
  ) {}

  ngOnInit(): void {
    this.filter = this.fb.group({
      q: [''],
      status: ['Active'], // All | Active | Overdue | Returned
    });

    this.filter.valueChanges.subscribe(() => this.applyFilter());

    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = null;

    this.loanService.getAll().subscribe({
      next: (res: any) => {
        const arr = Array.isArray(res) ? res : res?.items ?? [];
        this.loans = arr.map(normalizeLoan);
        this.applyFilter();
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.error = 'Failed to load loans.';
        this.loading = false;
      },
    });
  }

  applyFilter(): void {
    const { q, status } = this.filter.value;
    const ql = (q ?? '').toString().toLowerCase();

    this.filtered = this.loans.filter((l) => {
      const matchesQ =
        !ql ||
        (l.userName ?? '').toLowerCase().includes(ql) ||
        (l.userEmail ?? '').toLowerCase().includes(ql) ||
        (l.bookTitle ?? '').toLowerCase().includes(ql);

      const matchesStatus =
        status === 'All' || !status || l.status === status;

      return matchesQ && matchesStatus;
    });
  }

  markReturned(l: Loan): void {
    if (l.status === 'Returned') return;

    if (!confirm(`Mark "${l.bookTitle}" as returned for ${l.userName || l.userEmail || 'this user'}?`)) {
      return;
    }

    const idNum = Number(l.id);
    if (!idNum) return;

    this.loading = true;
    this.error = null;

    this.loanService.markReturned(idNum).subscribe({
      next: () => {
        // Update local state: mark as returned
        this.loans = this.loans.map(x =>
          Number(x.id) === idNum
            ? { ...x, status: 'Returned', returnDate: new Date().toISOString() }
            : x
        );
        this.applyFilter();
        this.loading = false;

        // 🔥 Ensure catalog availability reflects the return
        this.catalog.refresh();
      },
      error: (err) => {
        console.error(err);
        this.error = 'Failed to mark as returned.';
        this.loading = false;
      },
    });
  }
}

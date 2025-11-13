import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AuthService } from '../../../../../Core/Services/auth.service';
import { LoanService } from '../../../../Services/loan.service';
import { CatalogService } from '../../../../../catalog/services/catalog.service';

@Component({
  selector: 'app-loan-create',
  templateUrl: './loan-create.component.html',
  styleUrls: ['./loan-create.component.css'],
})
export class LoanCreateComponent implements OnInit {
  bookId!: number;
  userId = 0;
  isLoggedIn = false;
  loading = false;
  error: string | null = null;
  success = false;

  form!: FormGroup;

  constructor(
    private route: ActivatedRoute,
    private fb: FormBuilder,
    private auth: AuthService,
    private loans: LoanService,
    private catalog: CatalogService // <-- added
  ) {}

  ngOnInit(): void {
    // book id from route
    this.bookId = Number(this.route.snapshot.paramMap.get('bookId'));

    // grab current user (from localStorage)
    const user = this.auth.getCurrentUser();
    this.isLoggedIn = !!user;
    this.userId = user?.id ?? user?.Id ?? 0;

    // build tiny form
    const defaultEnd = new Date(Date.now() + 14 * 24 * 60 * 60 * 1000)
      .toISOString()
      .slice(0, 10);

    this.form = this.fb.group({
      endDate: [defaultEnd],
    });

    // react to future login
    this.auth.isLoggedIn$.subscribe((v) => {
      this.isLoggedIn = v;
      if (v) {
        const u = this.auth.getCurrentUser();
        this.userId = u?.id ?? u?.Id ?? 0;
      } else {
        this.userId = 0;
      }
    });
  }

  submit(): void {
    if (!this.isLoggedIn || !this.userId) {
      this.error = 'You must be logged in to loan.';
      return;
    }

    if (this.form.invalid) return;

    // backend is C#, so send PascalCase
    const payload = {
      BookId: this.bookId,
      UserId: this.userId,
      EndDate: this.form.value.endDate,
      Status: 'Aktiv',
    };

    this.loading = true;
    this.error = null;

    this.loans.create(payload).subscribe({
      next: () => {
        this.loading = false;
        this.success = true;

        // force catalog to refresh availability / copiesAvailable
        this.catalog.refresh?.();
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error ?? 'Could not create loan.';
        console.error(err);
      },
    });
  }
}

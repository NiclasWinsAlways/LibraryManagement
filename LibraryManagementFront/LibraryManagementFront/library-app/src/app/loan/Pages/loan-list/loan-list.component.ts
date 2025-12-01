import { Component, OnInit } from '@angular/core';
import { LoanService, LoanDto } from '../../Services/loan.service';
import { AuthService } from '../../../Core/Services/auth.service';
import { Observable } from 'rxjs';
import { NotificationService, NotificationDto } from '../../../Core/Services/notification.service'; // 👈 add

@Component({
  selector: 'app-loan-list',
  templateUrl: './loan-list.component.html',
  styleUrls: ['./loan-list.component.css'],
})
export class LoanListComponent implements OnInit {
  loans$!: Observable<LoanDto[]>;
  isLoggedIn = false;

  constructor(
    private loans: LoanService,
    private auth: AuthService,
    private notifications: NotificationService, // 👈 add
  ) {}

  ngOnInit(): void {
    // react to login / logout
    this.auth.isLoggedIn$.subscribe((v) => {
      this.isLoggedIn = v;
      if (v) {
        this.loans$ = this.loans.getAll();
        this.loans.getAll().subscribe(ls => this.generateUserDueSoonReminders(ls)); // 👈 add
      }
    });

    // page refresh case
    if (this.auth.getToken()) {
      this.isLoggedIn = true;
      this.loans$ = this.loans.getAll();
      this.loans.getAll().subscribe(ls => this.generateUserDueSoonReminders(ls)); // 👈 add
    }
  }

  toLocal(date: string): string {
    return date ? new Date(date).toLocaleDateString() : '';
  }

  // -------- user-side frontend-only generator --------
  private asyncCreateReminderIfMissing(userId: number, msg: string, existing: NotificationDto[]) {
    const norm = (s: string) => (s || '').trim().toLowerCase();
    if (existing.some(n => norm(n.message) === norm(msg))) return;
    this.notifications.create({ UserId: userId, Message: msg }).subscribe();
  }

  private generateUserDueSoonReminders(loans: LoanDto[]) {
    const user = this.auth.getCurrentUser();
    const userId = user?.id ?? user?.Id ?? 0;
    if (!userId) return;

    this.notifications.getForUser(userId).subscribe(existing => {
      const now = new Date();
      const inDays = (d: Date) => (d.getTime() - now.getTime()) / 86400000;

      loans
        .filter(l =>
          (l.status === 'Aktiv' || l.status === 'Active') &&
          !!l.endDate &&
          inDays(new Date(l.endDate)) >= 1 &&
          inDays(new Date(l.endDate)) <= 2
        )
        .forEach(l => {
          const due = new Date(l.endDate).toISOString().slice(0,10);
          const msg = `Reminder: your loan of '${(l as any).bookTitle ?? ('book #' + l.bookId)}' is due on ${due}. Please return or extend it.`;
          this.asyncCreateReminderIfMissing(userId, msg, existing);
        });
    });
  }
}

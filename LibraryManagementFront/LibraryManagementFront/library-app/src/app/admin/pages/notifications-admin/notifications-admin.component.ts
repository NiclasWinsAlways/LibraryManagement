import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import {
  NotificationService,
  NotificationDto,
} from '../../../Core/Services/notification.service';
import { LoanService, LoanDto } from '../../../loan/Services/loan.service';

@Component({
  selector: 'app-notifications-admin',
  templateUrl: './notifications-admin.component.html',
  styleUrls: ['./notifications-admin.component.css'],
})
export class NotificationsAdminComponent implements OnInit {
  loading = false;
  error = '';

  // forms
  filter!: FormGroup;      // { userId }
  createForm!: FormGroup;  // { UserId, Message }
  editForm!: FormGroup;    // { Message, IsRead }

  // data
  items: NotificationDto[] = [];
  editing: NotificationDto | null = null;

  // info about last scan
  lastScanMsg = '';

  // targets
  userEmailInput = '';
  userPhoneInput = '';

  constructor(
    private fb: FormBuilder,
    private api: NotificationService,
    private loansApi: LoanService,
  ) {}

  ngOnInit(): void {
    this.filter = this.fb.group({ userId: [''] });
    this.createForm = this.fb.group({
      UserId: ['', [Validators.required]],
      Message: ['', [Validators.required, Validators.minLength(1)]],
    });
    this.editForm = this.fb.group({ Message: [''], IsRead: [false] });
    this.filter.valueChanges.subscribe(() => this.load());
  }

  // -------- LIST --------
  load(): void {
    const userId = Number(this.filter.value.userId);
    if (!userId) { this.items = []; return; }

    this.loading = true;
    this.error = '';

    this.api.getForUser(userId).subscribe({
      next: (res) => { this.items = res || []; this.loading = false; },
      error: (err) => {
        console.error(err);
        this.error = 'Failed to load notifications.';
        this.loading = false;
      },
    });
  }

  // -------- CREATE --------
  create(): void {
    if (this.createForm.invalid) return;
    const v = this.createForm.value;
    const userId = Number(v.UserId);

    this.loading = true;
    this.error = '';

    this.api.create({ UserId: userId, Message: v.Message }).subscribe({
      next: () => {
        if (Number(this.filter.value.userId) === userId) {
          const now = new Date().toISOString();
          this.items = [
            { id: Math.floor(Math.random() * 1e9), userId, message: v.Message, isRead: false, createdAt: now },
            ...this.items,
          ];
        }
        this.createForm.reset({ UserId: userId, Message: '' });
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.error = 'Failed to create notification.';
        this.loading = false;
      },
    });
  }

  // -------- EDIT --------
  startEdit(n: NotificationDto): void {
    this.editing = n;
    this.editForm.reset({ Message: n.message, IsRead: n.isRead });
  }

  cancelEdit(): void {
    this.editing = null;
    this.editForm.reset();
  }

  saveEdit(): void {
    if (!this.editing) return;

    const v = this.editForm.value;
    const updated: NotificationDto = {
      ...this.editing,
      message: v.Message,
      isRead: !!v.IsRead,
    };

    this.loading = true;
    this.error = '';

    this.api.update(updated).subscribe({
      next: () => {
        this.items = this.items.map(x => x.id === updated.id ? { ...x, ...updated } : x);
        this.cancelEdit();
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.error = 'Failed to save notification.';
        this.loading = false;
      },
    });
  }

  // -------- ACTIONS --------
  markRead(n: NotificationDto): void {
    if (n.isRead) return;
    this.api.markRead(n.id).subscribe({
      next: () => { n.isRead = true; },
      error: (err) => console.error(err),
    });
  }

  delete(n: NotificationDto): void {
    if (!confirm(`Delete notification #${n.id}?`)) return;

    this.loading = true;
    this.error = '';

    this.api.delete(n.id).subscribe({
      next: () => {
        this.items = this.items.filter(x => x.id !== n.id);
        if (this.editing?.id === n.id) this.cancelEdit();
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.error = 'Failed to delete notification.';
        this.loading = false;
      },
    });
  }

  // -------- Send EMAIL (backend first, Gmail fallback) --------
  sendEmail(n: NotificationDto) {
    const to = (this.userEmailInput || '').trim();
    if (!to) { alert('Enter a recipient email.'); return; }

    this.loading = true; this.error = '';
    this.api.sendEmail({ To: to, Subject: 'Library reminder', Body: n.message }).subscribe({
      next: (res: any) => {
        this.loading = false;
        if (res && (res._missing || res._error)) this.openGmail(n, to);
        else alert('Email sent.');
      },
      error: () => { this.loading = false; this.openGmail(n, to); }
    });
  }

  // -------- Send SMS (backend first, device SMS fallback) --------
  sendSms(n: NotificationDto) {
    const to = (this.userPhoneInput || '').trim();
    if (!to) { alert('Enter phone (e.g., 4512345678).'); return; }

    this.loading = true; this.error = '';
    this.api.sendSms({ To: to, Body: n.message }).subscribe({
      next: (res: any) => {
        this.loading = false;
        if (res && (res._missing || res._error)) this.smsFallback(n, to);
        else alert('SMS sent.');
      },
      error: () => { this.loading = false; this.smsFallback(n, to); }
    });
  }

  // -------- Server due-scan now --------
  runDueScanNow(): void {
    this.loading = true; this.error = ''; this.lastScanMsg = '';
    this.api.runDueScan().subscribe({
      next: () => {
        this.loading = false;
        this.lastScanMsg = 'Due-scan triggered. Emails (if any) were sent by server.';
        setTimeout(() => this.load(), 400);
      },
      error: (err) => {
        console.error(err);
        this.error = 'Failed to trigger due-scan.';
        this.loading = false;
      },
    });
  }

  // -------- Fallbacks --------
  private openGmail(n: NotificationDto, email: string) {
    const subject = encodeURIComponent('Library reminder');
    const body = encodeURIComponent(n.message);
    const mailto = `mailto:${encodeURIComponent(email)}?subject=${subject}&body=${body}`;
    const gmail  = `https://mail.google.com/mail/?extsrc=mailto&url=${encodeURIComponent(mailto)}`;
    const w = window.open(gmail, '_blank');
    if (!w) window.location.href = gmail;
  }
  private smsFallback(n: NotificationDto, phone: string) {
    const body = encodeURIComponent(n.message || '');
    window.location.href = `sms:${phone}?&body=${body}`;
  }

  // -------- FRONTEND-ONLY DUE-SOON GENERATION (0–3 days) --------
  private parseDateLoose(input: any): Date | null {
    if (!input) return null;
    if (input instanceof Date) return input;
    const s = String(input);
    const d = s.length === 10 && /^\d{4}-\d{2}-\d{2}$/.test(s) ? new Date(s + 'T00:00:00') : new Date(s);
    return isNaN(d.getTime()) ? null : d;
  }
  private daysUntil(date: Date): number {
    const start = new Date();
    const a = new Date(start.getFullYear(), start.getMonth(), start.getDate()).getTime();
    const b = new Date(date.getFullYear(), date.getMonth(), date.getDate()).getTime();
    return Math.round((b - a) / (24 * 60 * 60 * 1000));
  }
  private isActiveish(status: any): boolean {
    const s = String(status || '').toLowerCase();
    if (!s) return false;
    if (s.includes('return')) return false;
    return s.includes('aktiv') || s.includes('active') || s.includes('overdue');
  }
  private alreadyHasMessage(existing: NotificationDto[], msg: string): boolean {
    const norm = (x: string) => (x || '').trim().toLowerCase();
    return existing.some(n => norm(n.message) === norm(msg));
  }
  generateDueSoon(): void {
    const userId = Number(this.filter.value.userId);
    if (!userId) return;

    this.loading = true; this.error = ''; this.lastScanMsg = '';

    this.api.getForUser(userId).subscribe({
      next: existing => {
        this.loansApi.getAll().subscribe({
          next: (loans: LoanDto[]) => {
            const candidates = (loans || []).filter((l: any) => {
              if (Number(l.userId) !== userId) return false;
              if (!this.isActiveish(l.status)) return false;
              const d = this.parseDateLoose(l.endDate);
              if (!d) return false;
              const days = this.daysUntil(d);
              return days >= 0 && days <= 3;
            });

            if (!candidates.length) {
              this.loading = false;
              this.lastScanMsg = 'No due-soon loans found (0–3 days).';
              alert('No loans due within 0–3 days for this user.');
              return;
            }

            const createCalls = candidates.map((t: any) => {
              const end = this.parseDateLoose(t.endDate)!;
              const dueStr = end.toISOString().slice(0, 10);
              const title = (t as any).bookTitle ?? `book #${t.bookId}`;
              const msg = `Reminder: your loan of '${title}' is due on ${dueStr}. Please return or extend it.`;
              if (this.alreadyHasMessage(existing, msg)) return null;
              return this.api.create({ UserId: userId, Message: msg });
            }).filter(Boolean) as any[];

            if (!createCalls.length) {
              this.loading = false;
              this.lastScanMsg = `All ${candidates.length} reminder(s) already exist.`;
              alert('All reminders already exist for this user.');
              return;
            }

            let created = 0;
            const next = () => {
              const c = createCalls.shift();
              if (!c) {
                this.loading = false;
                this.lastScanMsg = `Created ${created} reminder(s).`;
                this.load();
                alert(`Created ${created} reminder(s).`);
                return;
              }
              c.subscribe({ next: () => { created++; next(); }, error: () => { next(); } });
            };
            next();
          },
          error: (err) => {
            console.error(err);
            this.error = 'Failed to read loans.';
            this.loading = false;
          }
        });
      },
      error: (err) => {
        console.error(err);
        this.error = 'Failed to read existing notifications.';
        this.loading = false;
      }
    });
  }
}

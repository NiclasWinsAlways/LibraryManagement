import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  Validators,
  FormGroup,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../Core/Services/auth.service';

function match(control: AbstractControl): ValidationErrors | null {
  const pass = control.get('password')?.value;
  const confirm = control.get('confirm')?.value;
  return pass && confirm && pass !== confirm ? { mismatch: true } : null;
}

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent implements OnInit {
  form!: FormGroup;
  loading = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group(
      {
        name: ['', [Validators.required, Validators.minLength(2)]],
        email: ['', [Validators.required, Validators.email]],
        password: ['', [Validators.required, Validators.minLength(6)]],
        confirm: ['', [Validators.required]],
      },
      { validators: match }
    );
  }

  submit(): void {
    if (this.form.invalid || this.loading) return;

    const { confirm, ...raw } = this.form.value;

    // backend wants PascalCase
    const payload = {
      Name: raw.name,
      Email: raw.email,
      Password: raw.password,
    };
    console.log('Register payload to API:', payload);

    this.loading = true;
    this.error = null;

    this.auth.register(payload).subscribe({
      next: (res) => {
        console.log('User created:', res);
        this.loading = false;
        this.router.navigateByUrl('/auth/login');
      },
      error: (err) => {
        console.error('Create user failed:', err);
        this.loading = false;
        this.error = err?.error || 'Could not create user.';
      },
    });
  }
}

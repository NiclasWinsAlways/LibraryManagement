import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { AuthService } from './Core/Services/auth.service';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit {
  year = new Date().getFullYear();
  isHome = true;
  isLoggedIn = false;
  isAdmin = false;

  constructor(private router: Router, private auth: AuthService) {}

  ngOnInit(): void {
    // update home flag
    this.router.events
      .pipe(filter((e) => e instanceof NavigationEnd))
      .subscribe((e: any) => {
        this.isHome = e.urlAfterRedirects === '/' || e.urlAfterRedirects === '';
      });

    // login / logout
    this.auth.isLoggedIn$.subscribe((v) => (this.isLoggedIn = v));
    this.auth.isAdmin$.subscribe((v) => (this.isAdmin = v));

    // page refresh
    if (this.auth.getToken()) {
      this.isLoggedIn = true;
      this.isAdmin = this.auth.getRole() === 'Admin';
    }
  }

  onLogout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/');
  }
}

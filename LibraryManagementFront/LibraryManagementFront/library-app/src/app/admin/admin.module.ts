import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';

import { AdminRoutingModule } from './admin-routing.module';

import { AdminPageComponent } from './pages/admin-page/admin-page.component';
import { LoansAdminComponent } from './pages/loans-admin/loans-admin.component';
import { BooksAdminComponent } from './pages/books-admin/books-admin.component';
import { UsersAdminComponent } from './pages/users-admin/users-admin.component';
import { ReservationsAdminComponent } from './pages/reservations-admin/reservations-admin.component';
import { NotificationsAdminComponent } from './pages/notifications-admin/notifications-admin.component';

@NgModule({
  declarations: [
    AdminPageComponent,
    LoansAdminComponent,
    BooksAdminComponent,
    UsersAdminComponent,
    ReservationsAdminComponent,
    NotificationsAdminComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,
    AdminRoutingModule,
  ],
})
export class AdminModule {}

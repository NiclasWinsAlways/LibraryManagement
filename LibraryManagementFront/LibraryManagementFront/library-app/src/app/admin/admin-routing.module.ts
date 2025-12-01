import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { AdminPageComponent } from './pages/admin-page/admin-page.component';
import { LoansAdminComponent } from './pages/loans-admin/loans-admin.component';
import { BooksAdminComponent } from './pages/books-admin/books-admin.component';
import { UsersAdminComponent } from './pages/users-admin/users-admin.component';
import { ReservationsAdminComponent } from './pages/reservations-admin/reservations-admin.component';
import { NotificationsAdminComponent } from './pages/notifications-admin/notifications-admin.component';
import { AdminGuard } from '../Core/Guards/admin.guard';

const routes: Routes = [
  { path: '', component: AdminPageComponent, canActivate: [AdminGuard] },
  { path: 'books', component: BooksAdminComponent, canActivate: [AdminGuard] },
  { path: 'loans', component: LoansAdminComponent, canActivate: [AdminGuard] },
  { path: 'users', component: UsersAdminComponent, canActivate: [AdminGuard] },
  { path: 'reservations', component: ReservationsAdminComponent, canActivate: [AdminGuard] },
  { path: 'notifications', component: NotificationsAdminComponent, canActivate: [AdminGuard] },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AdminRoutingModule {}

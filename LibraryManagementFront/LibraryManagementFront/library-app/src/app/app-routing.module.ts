import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoanCreateComponent } from './loan/Pages/loan-list/new/loan-create/loan-create.component';
import { LoanListComponent } from './loan/Pages/loan-list/loan-list.component';

const routes: Routes = [
  // catalog (lazy)
  {
    path: 'catalog',
    loadChildren: () =>
      import('./catalog/catalog.module').then((m) => m.CatalogModule),
  },

  // auth (lazy)
  {
    path: 'auth',
    loadChildren: () =>
      import('./auth/auth.module').then((m) => m.AuthModule),
  },

  // admin (lazy)
  {
    path: 'admin',
    loadChildren: () =>
      import('./admin/admin.module').then((m) => m.AdminModule),
  },

  // loans (non-lazy)
  {
    path: 'loans',
    children: [
      { path: '', component: LoanListComponent },
      { path: 'new/:bookId', component: LoanCreateComponent },
    ],
  },

  // wildcard → go back to "/" so your app.component shows frontpage
  { path: '**', redirectTo: '' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}

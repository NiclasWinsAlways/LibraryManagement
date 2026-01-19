// src/app/app.module.ts
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';

//  BOTH kinds of forms
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';

//  your loan components (use YOUR exact path/casing)
import { LoanListComponent } from './loan/Pages/loan-list/loan-list.component';
import { LoanCreateComponent } from './loan/Pages/loan-list/new/loan-create/loan-create.component';

@NgModule({
  declarations: [
    AppComponent,
    LoanListComponent,
    LoanCreateComponent,
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,          //  this is the one ngModel needs
    ReactiveFormsModule,  //  for your login/register
    AppRoutingModule,
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}

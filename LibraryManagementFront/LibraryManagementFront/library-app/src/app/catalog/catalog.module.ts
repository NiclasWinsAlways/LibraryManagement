import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http'; // ← add

import { CatalogRoutingModule } from './catalog-routing.module';
import { SearchComponent } from './pages/search/search.component';

@NgModule({
  declarations: [SearchComponent],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,     // ← add
    CatalogRoutingModule
  ],
})
export class CatalogModule {}

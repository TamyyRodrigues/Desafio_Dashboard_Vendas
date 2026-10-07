import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { CurrencyBrPipe } from './pipes/currency-br.pipe';
import { PageHeaderComponent } from './components/page-header/page-header.component';

@NgModule({
  declarations: [CurrencyBrPipe, PageHeaderComponent],
  imports: [CommonModule],
  exports: [CurrencyBrPipe, PageHeaderComponent]
})
export class SharedModule {}

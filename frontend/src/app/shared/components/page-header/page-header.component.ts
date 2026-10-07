import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-page-header',
  template: `
    <header class="mb-4">
      <h1 class="h3 mb-1">{{ title }}</h1>
      <p *ngIf="subtitle" class="text-muted mb-0">{{ subtitle }}</p>
    </header>
  `
})
export class PageHeaderComponent {
  @Input() title = '';
  @Input() subtitle = '';
}

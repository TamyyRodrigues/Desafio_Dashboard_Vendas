import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { TableModule } from 'primeng/table';
import { InputTextModule } from 'primeng/inputtext';
import { ChartModule } from 'primeng/chart';
import { ButtonModule } from 'primeng/button';
import { ModalModule } from 'ngx-bootstrap/modal';
import { AlertModule } from 'ngx-bootstrap/alert';

import { SharedModule } from '../../shared/shared.module';
import { UploadComponent } from '../upload/upload.component';
import { DashboardComponent } from './dashboard.component';

@NgModule({
  declarations: [DashboardComponent, UploadComponent],
  imports: [
    CommonModule,
    FormsModule,
    SharedModule,
    TableModule,
    InputTextModule,
    ChartModule,
    ButtonModule,
    ModalModule,
    AlertModule,
    RouterModule.forChild([{ path: '', component: DashboardComponent }])
  ]
})
export class DashboardModule {}

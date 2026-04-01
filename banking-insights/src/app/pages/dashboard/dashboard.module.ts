import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardComponent } from './dashboard/dashboard.component';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../../app/shared/shared.module';
import { PrimeTreeTableComponent } from '../../shared/components/prime-tree-table/prime-tree-table.component';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { SidebarModule } from 'primeng/sidebar';
import { ViewIconDetailsComponent } from './view-icon-details/view-icon-details.component';

const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'dashboard-view',
  },
  {
    path: 'dashboard-view',
    component: DashboardComponent,
  },
];

@NgModule({
  declarations: [
    DashboardComponent,
    ViewIconDetailsComponent
    
  ],
  imports: [
    CommonModule,
    SharedModule,
    FormsModule,
    SidebarModule,
    HttpClientModule,
    RouterModule.forChild(routes)
  ],
  exports:[
    DashboardComponent,
    ViewIconDetailsComponent
  ]
})
export class DashboardModule { }

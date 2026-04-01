import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LayoutComponent } from './shared/layout/layout.component';
import { authGuard, noAuthGuard } from './core/guards/authentication/authentication.guard';


const routes: Routes = [
  {
    path: '',
    component: LayoutComponent, // Add layout component here
    canActivate: [authGuard],
    canActivateChild: [authGuard],
    children: [
      {
        path: 'dashboard',
        loadChildren: () => import('./pages/dashboard/dashboard.module').then(m => m.DashboardModule)
      },
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      }
    ]
  },
{
    path: 'login',
    pathMatch:'full',
    // canActivate: [authenticationGuard],
        canActivate: [noAuthGuard],

    loadChildren:()=>import('./pages/login/login.module').then((c)=>c.LoginModule)
    // canActivate: [noAuthGuard],
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }

import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoginViewComponent } from './login-view/login-view.component';
import { RouterModule, Routes } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { InputOtpModule } from 'primeng/inputotp';



const routes: Routes = [
  {
    path: '',
    pathMatch:'full',
    component: LoginViewComponent,
  },
];

@NgModule({
  declarations: [
    LoginViewComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CommonModule,
    InputOtpModule,
    SharedModule,
    RouterModule.forChild(routes),

  ],
  exports:[
    LoginViewComponent
  ]
})
export class LoginModule { }

import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormControl } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthenticationService } from '../../../store/auth/authentication/authentication.service';
import { Store } from '@ngrx/store';
import { loginSuccess } from '../../../store/auth/auth.actions';
import { ToasterService } from '../../../services/toaster/toaster.service';
import { EncryptionService } from '../../../services/security/encryption.service';

@Component({
  selector: 'app-login-view',
  standalone: false,
  // imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login-view.component.html',
  styleUrls: ['./login-view.component.scss']
})
export class LoginViewComponent {

  // ===== Dummy Credentials =====
  private readonly DUMMY_USERNAME = 'admin';
  private readonly DUMMY_PASSWORD = 'admin123';
  private readonly DUMMY_OTP = '123456';

  loginForm!: FormGroup;
  otpForm!: FormGroup;

  isOtpMode = false;
  isLoading = false;
  currentYear = new Date().getFullYear();

  // signal used in template
  otpTemplate = signal(false);
  countdown = signal(0);
  private countdownInterval: any;
    authService = inject(AuthenticationService);
    security = inject(EncryptionService)
      store = inject(Store);



  constructor(private fb: FormBuilder, private router: Router,  private toaster: ToasterService ) {
    this.initForms();
  }

  // ===== Form Initialization =====
  private initForms(): void {
    this.loginForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required],
      // contact: ['', [
      //   Validators.required,
      //   Validators.pattern(/(^\d{10}$)|(^[\w.-]+@[\w.-]+\.\w+$)/)
      // ]]
    });

    this.otpForm = this.fb.group({
      otp: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  // ===== Helpers =====
  getControl(name: string): FormControl {
    return this.loginForm.get(name) as FormControl;
  }

   onSubmit() {
    // Mark only the username and password fields as touched
    this.loginForm.get('username')?.markAsTouched();
    this.loginForm.get('password')?.markAsTouched();

    // Validate only the username and password fields
    if (this.loginForm.get('username')?.invalid || this.loginForm.get('password')?.invalid) {
      // this.notificationService.error('Please fill in all required fields');
      return;
    }

    const payload:any = {
      Username : this.getControl('username').value,
      Password : this.getControl('password').value,  
    };
    
    const encrypted_data = this.security.encryptAES_JSON(payload);
    
    const encrypted_payload:any = {
      jsonEncrypt:encrypted_data
    }


    this.isLoading = true;
    this.authService.login(encrypted_payload).subscribe({
      next: (res) => {
        if (res?.status === 'Ok') {
          const jwtToken = res?.data?.token;
          const user = res?.data?.user
          // const userData = res[0]?.Data ? res[0].Data[0] : null;
          if (jwtToken) {
            // Store the token in localStorage
            // localStorage.setItem('googleToken', jwtToken);
            const loginData = {
              token: jwtToken,
              user:user      
            };
            this.store.dispatch(loginSuccess({ authData: loginData }));

            // this.router.navigate(['/']);
            this.router.navigate(['/dashboard/dashboard-view']);
                  this.toaster.show('success', 'Login Successful', 'You have successfully logged in!');

          }
        } else {
          // this.notificationService.error(res[0]?.Message || 'Something went wrong');
        }
        this.isLoading = false;
      },
      error: (err) => {
        console.log(err);
        this.isLoading = false;
      },
    });
  }

  // ===== Send OTP =====
  onSendOtp(): void {
    if (this.loginForm.get('contact')?.invalid) return;

    this.isLoading = true;

    setTimeout(() => {
      this.isLoading = false;
      this.otpTemplate.set(true);
      this.startCountdown();

      // console.log('Dummy OTP:', this.DUMMY_OTP);
      alert(`📩 OTP Sent (Dummy): ${this.DUMMY_OTP}`);
    }, 1000);
  }

  // ===== Verify OTP =====
  verifyOtp(): void {
    if (this.otpForm.invalid) return;

    const enteredOtp = this.otpForm.value.otp;

    this.isLoading = true;

    setTimeout(() => {
      this.isLoading = false;

      if (enteredOtp === this.DUMMY_OTP) {
        alert('✅ OTP Verified Successfully!');
      } else {
        alert('❌ Invalid OTP');
      }
    }, 1000);
  }

  // ===== Countdown Logic =====
  private startCountdown(): void {
    this.countdown.set(30);
    clearInterval(this.countdownInterval);

    this.countdownInterval = setInterval(() => {
      if (this.countdown() > 0) {
        this.countdown.set(this.countdown() - 1);
      } else {
        clearInterval(this.countdownInterval);
      }
    }, 1000);
  }
}

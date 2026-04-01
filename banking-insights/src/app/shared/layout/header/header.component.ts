import { Component, inject } from '@angular/core';
import { AuthenticationService } from '../../../store/auth/authentication/authentication.service';
import { userDetails } from '../../../store/auth/auth.selectors';
import { take } from 'rxjs';
import { Store } from '@ngrx/store';
import { EncryptionService } from '../../../services/security/encryption.service';


interface User {
  UserId: number;
  LoginName: string;
  PasswordHash: string | null;
  FirstName: string;
  LastName: string;
}

@Component({
  selector: 'app-header',
  standalone: false,
  // imports: [],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})



export class HeaderComponent {

  

  authService = inject(AuthenticationService);

  userDetails: { FirstName: string; LastName: string } = { FirstName: '', LastName: ''};

  Store = inject(Store);
  security = inject(EncryptionService);

  greetingText: string = '';
  userName: string = '';
  userEmail: string = '';
  todayFormatted: string = '';
  store: any;
  search$: any;
  receivedSearchText: any;
  ngOnInit() {


     this.Store
          .select(userDetails)
          .pipe(take(1))
          .subscribe((user) => {

            const encrypted_user = JSON.parse(this.security.decryptAES(user.user))
            
            this.userDetails = {
              FirstName:encrypted_user.FirstName,
              LastName:encrypted_user.LastName
            }
             this.getGreetingText();
            this.setTodayFormatted();
    
          });
 
    // this.store.select(storedDetails).subscribe((data: any) => {
    //   this.userName = data?.userName || '';
    //   this.userEmail = data?.Email_Id_Personl ||data?.Email_Id_Official || '';
    // });
  }
  getList() {
    throw new Error('Method not implemented.');
  }

    logout() {
    // this.authService.logout();
    this.authService.logout();
  }

  getGreetingText() {
    const currentHour = new Date().getHours();
    if (currentHour < 12) {
      this.greetingText = 'Good Morning';
    } else if (currentHour < 18) {
      this.greetingText = 'Good Afternoon';
    } else {
      this.greetingText = 'Good Evening';
    }
  }
  private setTodayFormatted() {
    const today = new Date();
    const day = today.getDate().toString().padStart(2, '0');
    const month = today.toLocaleString('en-US', { month: 'short' });
    const year = today.getFullYear();
  
    this.todayFormatted = `${day}, ${month} ${year}`;
  }

}

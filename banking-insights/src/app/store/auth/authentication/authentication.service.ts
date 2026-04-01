import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { ConfigService } from '../../../services/config.service';
import { logout } from '../auth.actions';

const endpointPaths = {
  login: 'https://task.piplapps.com:8032/login_PS',
};

@Injectable({
  providedIn: 'root'
})


export class AuthenticationService {
  private http = inject(HttpClient);
  private router = inject(Router);
  store = inject(Store);
  private config = inject(ConfigService);

  login(credential: Credential): Observable<any> {  
    return this.http.post(`${endpointPaths.login}`, credential);
  }

  logout() {
    localStorage.clear();
    this.store.dispatch(logout());
    this.router.navigate(['/login'], { replaceUrl: true });
  }
}

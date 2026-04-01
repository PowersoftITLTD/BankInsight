import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ConfigService {
  private config: any;

  constructor(private http: HttpClient) { }

  loadConfig() {
    return this.http
      .get('config.json').subscribe({
        next: ((config) => {
          this.config = config;
          // console.log('Config file: ', config)
        })
      })
  }

  get apiBaseUrl(): string {
    return this.config?.apiBaseUrl || '';
  }

  get clientId(): string {
    return this.config?.client_id || '';
  }

  
  get encryptionKey(): string {
    return this.config?.encryptionKey || '';
  }
}
    
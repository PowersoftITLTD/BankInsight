import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ConfigService } from './config.service';
import { Observable } from 'rxjs';


const endpointPaths = {
  BankPortal_Dashboard_PS: '/BankPortal',
  // viewClassification: '/ViewClassification',
  // ApprovalTemplate:'/ApprovalTemplate',
  // DocumentTemplate:'/DocumentTemplate',
  // projectDefination:'/ProjectDefination',
  // documentDepository:'/ProjectDocumentDepository',
  // recursiveTask:''
};


@Injectable({
  providedIn: 'root'
})
export class ApiService {

  private http = inject(HttpClient);
  private config = inject(ConfigService);

  constructor() { }

  postDetails(
    url: string,
    body: Object,
    BankPortal_Dashboard_PS: any): Observable<any> {
    if (BankPortal_Dashboard_PS) {
      url = endpointPaths.BankPortal_Dashboard_PS + '/' + url;
    }
    
    return this.http.post(`${this.config.apiBaseUrl}${url}`, body);
  }
}

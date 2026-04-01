import { Injectable } from '@angular/core';
import { BehaviorSubject, filter, Observable } from 'rxjs';
import { Toast } from '../toaster/toast.interface';
import { ToastType } from '../toaster/toast.type';

@Injectable({
  providedIn: 'root'
})
export class ToasterService {

  subject: BehaviorSubject<Toast | null>;
  toast$: Observable<Toast>;


  constructor() {
    this.subject = new BehaviorSubject<Toast | null>(null);
    this.toast$ = this.subject.asObservable()
      .pipe(filter(toast => toast !== null));
  }


  show(type: ToastType, title?: string, body?: string, delay?: number) {
    this.subject.next({ type, title:  title ?? '', body :body??'' , delay:delay?? 0 });
  }

}

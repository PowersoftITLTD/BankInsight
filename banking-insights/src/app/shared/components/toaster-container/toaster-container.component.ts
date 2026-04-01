import { Component } from '@angular/core';
import { ToasterService } from '../../../services/toaster/toaster.service';
import { Toast } from '../../../services/toaster/toast.interface';
// import { ToasterComponent } from '../toaster-container/toaster/toaster.component';
import { AsyncPipe, CommonModule, JsonPipe, NgFor } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ToasterComponent } from './toaster/toaster.component';

@Component({
  selector: 'app-toaster-container',
  standalone: false,
  // imports: [
  //    ToasterComponent,
  //   CommonModule,
  //   NgFor,
  //   AsyncPipe,
  //   JsonPipe,
  // ],
  templateUrl: './toaster-container.component.html',
  styleUrls: ['./toaster-container.component.scss']
})
export class ToasterContainerComponent {

  toasts: Toast[] = [];

  constructor(public toaster: ToasterService) {}  // <-- MUST be public

 ngOnInit() {
    this.toaster.toast$
      .subscribe(toast => {
        this.toasts = [toast, ...this.toasts];
        setTimeout(() => this.toasts.pop(), toast.delay || 6000);
      });
  }

  remove(index: number) {
    this.toasts = this.toasts.filter((v, i) => i !== index);
    this.toasts.splice(index, 1);
  }
}

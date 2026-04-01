import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-view-icon-details',
  standalone: false,
  // imports: [],
  templateUrl: './view-icon-details.component.html',
  styleUrl: './view-icon-details.component.scss'
})
export class ViewIconDetailsComponent {
  @Input() visible: boolean = false;           // Parent controls visibility
  @Input() rowData: any;                        // Data from clicked row
  @Output() sidebarHide = new EventEmitter<void>(); // Event to notify parent when closed

  closeSidebar() {
    this.sidebarHide.emit();
  }

  onSubmitAdd() {
    setTimeout(() => {
      this.closeSidebar();
      alert('Task added!');
    }, 500);
  }

  onSubmitUpdate() {
    setTimeout(() => {
      this.closeSidebar();
      alert('Task updated!');
    }, 500);
  }

  onCancel() {
    this.closeSidebar();
  }

   negativeAmtfield(amt: any): string {

    if (amt == null) {
      return '';
    }

    // Convert to string and check if it starts with '-'
    if (amt.toString().startsWith('-')) {
      return 'count-overdue total';
    }else {
      return 'detail-row total';
    }

    // return '';
  }

}

import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-button',
  standalone: false,
  // imports: [],
  templateUrl: './button.component.html',
  styleUrls: ['./button.component.scss']
})
export class ButtonComponent {

    @Input() label: string = 'Button';
  @Input() iconStart: string = '';
  @Input() iconEnd: string = '';
  @Input() iconPosition: 'start' | 'end' | 'both' = 'start';
  @Input() isLoading: boolean = false;
  @Input() disabled: boolean = false;

  @Output() onClick = new EventEmitter<any>();

  handleClick() {
    // console.log('clicked')
    this.onClick.emit();
  }

}

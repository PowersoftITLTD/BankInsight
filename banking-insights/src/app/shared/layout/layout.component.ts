import { Component } from '@angular/core';

@Component({
  selector: 'app-layout',
  standalone: false,
  // imports: [],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.scss'
})
export class LayoutComponent {
  docked = false; 

}

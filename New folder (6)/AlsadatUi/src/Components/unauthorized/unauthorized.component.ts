import { Component } from '@angular/core';
import { Location } from '@angular/common';

@Component({
  selector: 'app-unauthorized',
  standalone: true,
  imports: [],
  templateUrl: './unauthorized.component.html',
  styleUrl: './unauthorized.component.css'
})
export class UnauthorizedComponent {
constructor(private location: Location) {}

goBack() {
  this.location.back();
}
}

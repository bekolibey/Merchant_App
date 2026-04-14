import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-layouts',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './layouts.html',
  styleUrl: './layouts.scss',
})
export class Layouts {}

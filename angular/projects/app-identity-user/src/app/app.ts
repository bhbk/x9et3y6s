import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthStore, ChatBubbleComponent } from 'lib-identity';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ChatBubbleComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  readonly authStore = inject(AuthStore);
}

import { Component } from '@angular/core';
import { ChatComponent } from 'lib-identity';

@Component({
  selector: 'app-assistant',
  standalone: true,
  imports: [ChatComponent],
  template: `
    <div class="p-6 h-full flex flex-col">
      <lib-chat class="flex-1 min-h-0"></lib-chat>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      height: 100%;
    }
  `]
})
export class AssistantComponent {}

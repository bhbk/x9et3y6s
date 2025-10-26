import {
  Component,
  inject,
  OnDestroy,
  OnInit,
  signal,
  ViewChild,
  ElementRef,
  AfterViewChecked,
  effect
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { HubConnectionState } from '@microsoft/signalr';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_INPUTS } from '@progress/kendo-angular-inputs';
import { KENDO_INDICATORS } from '@progress/kendo-angular-indicators';
import { KENDO_ICONS } from '@progress/kendo-angular-icons';
import {
  commentIcon,
  xIcon,
  paperPlaneIcon,
  chevronDownIcon,
  unlinkIcon,
  warningTriangleIcon
} from '@progress/kendo-svg-icons';
import { PublicChatService } from '../../services/public-chat.service';

interface BubbleMessage {
  id: number;
  role: 'user' | 'assistant';
  content: string;
  created: string;
  typing?: boolean;
}

@Component({
  selector: 'lib-chat-bubble',
  standalone: true,
  imports: [FormsModule, KENDO_BUTTONS, KENDO_INPUTS, KENDO_INDICATORS, KENDO_ICONS],
  template: `
    @if (!panelOpen() && llmAvailable() === true) {
      <button
        kendoButton
        [svgIcon]="commentIcon"
        rounded="full"
        themeColor="primary"
        size="large"
        class="chat-fab"
        (click)="togglePanel()"
        title="Chat with us">
      </button>
    }

    @if (panelOpen()) {
      <div class="chat-panel">

        <!-- Header -->
        <div class="chat-panel-header">
          <span>Hi, how can I help?</span>
          <button
            kendoButton
            [svgIcon]="xIcon"
            fillMode="flat"
            size="small"
            (click)="togglePanel()">
          </button>
        </div>

        <!-- Body -->
        <div class="flex flex-col flex-1 min-h-0 bg-white">

          @if (llmAvailable() === null && !connectionError()) {
            <!-- Connecting -->
            <div class="flex-1 flex flex-col items-center justify-center gap-3 text-center px-6">
              <kendo-loader size="medium"></kendo-loader>
              <p class="text-sm text-gray-400">Connecting to assistant...</p>
            </div>

          } @else if (connectionError()) {
            <!-- Connection error -->
            <div class="flex-1 flex flex-col items-center justify-center gap-3 text-center px-6">
              <div class="w-12 h-12 bg-gray-100 rounded-full flex items-center justify-center">
                <kendo-svg-icon [icon]="unlinkIcon" size="xlarge" class="text-gray-400"></kendo-svg-icon>
              </div>
              <p class="text-sm font-medium text-gray-600">Unable to connect</p>
              <button kendoButton themeColor="primary" size="small" (click)="retryConnect()">Retry</button>
            </div>

          } @else if (llmError()) {
            <!-- LLM service error -->
            <div class="flex-1 flex flex-col items-center justify-center gap-3 text-center px-6">
              <div class="w-12 h-12 bg-red-100 rounded-full flex items-center justify-center">
                <kendo-svg-icon [icon]="warningTriangleIcon" size="xlarge" class="text-red-500"></kendo-svg-icon>
              </div>
              <p class="text-sm font-medium text-gray-600">Service unavailable</p>
              <p class="text-xs text-gray-400">There is a problem with the assistant service.</p>
            </div>

          } @else if (llmAvailable() === false) {
            <!-- LLM not available -->
            <div class="flex-1 flex flex-col items-center justify-center gap-3 text-center px-6">
              <div class="w-12 h-12 bg-gray-100 rounded-full flex items-center justify-center">
                <kendo-svg-icon [icon]="unlinkIcon" size="xlarge" class="text-gray-400"></kendo-svg-icon>
              </div>
              <p class="text-sm font-medium text-gray-600">Assistant unavailable</p>
            </div>

          } @else {
            <!-- Connected and ready -->
            <div class="relative flex-1 min-h-0">

              <!-- Messages -->
              <div #messagesContainer class="absolute inset-0 overflow-y-auto hide-scrollbar" (scroll)="onMessagesScroll()">
                <div class="px-4 py-4 pb-36 space-y-3">

                  @if (messages().length === 0 && !waitingForResponse()) {
                    <div class="flex items-center justify-center h-full pt-12">
                      <p class="text-sm text-gray-400">Ask me anything...</p>
                    </div>
                  }

                  @for (msg of messages(); track msg.id) {
                    @if (msg.role === 'user') {
                      <div class="flex justify-end">
                        <div class="max-w-[80%] bg-blue-50 rounded-2xl px-3 py-2">
                          <p class="text-sm text-gray-800 whitespace-pre-wrap leading-relaxed">{{ msg.content }}</p>
                          <p class="text-[10px] text-gray-400 mt-1 text-right">{{ formatTime(msg.created) }}</p>
                        </div>
                      </div>
                    } @else {
                      <div class="max-w-[85%] bg-gray-100 rounded-2xl px-3 py-2">
                        <p class="text-sm text-gray-800 whitespace-pre-wrap leading-relaxed">{{ msg.content }}</p>
                        <p class="text-[10px] text-gray-400 mt-1">{{ formatTime(msg.created) }}</p>
                      </div>
                    }
                  }

                  @if (waitingForResponse() && !currentStreamContent()) {
                    <div class="flex items-center gap-1 pl-1">
                      <span class="w-2 h-2 rounded-full bg-gray-300 animate-bounce" style="animation-delay:0ms"></span>
                      <span class="w-2 h-2 rounded-full bg-gray-300 animate-bounce" style="animation-delay:150ms"></span>
                      <span class="w-2 h-2 rounded-full bg-gray-300 animate-bounce" style="animation-delay:300ms"></span>
                    </div>
                  }

                </div>
              </div>

              <!-- Floating input -->
              <div class="absolute bottom-0 left-0 right-0 px-3 pb-3 pt-10 bg-gradient-to-t from-white via-white to-transparent pointer-events-none">
                <div class="pointer-events-auto">

                  @if (showScrollButton) {
                    <div class="flex justify-center mb-2">
                      <button
                        kendoButton fillMode="outline" size="small" [svgIcon]="chevronDownIcon"
                        class="rounded-full bg-white shadow-md"
                        title="Scroll to bottom"
                        (click)="scrollToBottomManual()"
                      ></button>
                    </div>
                  }

                  <div class="rounded-xl border border-gray-200 bg-white shadow-sm focus-within:border-gray-300 focus-within:shadow-md transition-all overflow-hidden">
                    <kendo-textarea
                      [(ngModel)]="messageText"
                      placeholder="Type a message..."
                      [style.width.%]="100"
                      [rows]="2"
                      (keydown.enter)="onEnterKey($event)"
                      (keydown.arrowUp)="onArrowUp($event)"
                      (keydown.arrowDown)="onArrowDown($event)"
                      [disabled]="waitingForResponse()"
                    ></kendo-textarea>
                    <div class="flex items-center justify-between px-2 py-1.5 border-t border-gray-100">
                      <span class="text-[10px] text-gray-400">Shift+Enter new line &middot; Up/Down history</span>
                      <button
                        kendoButton themeColor="primary" [svgIcon]="paperPlaneIcon" size="small"
                        [disabled]="!messageText.trim() || waitingForResponse()"
                        (click)="sendMessage()"
                        style="border-radius:50%"
                      ></button>
                    </div>
                  </div>

                </div>
              </div>

            </div>
          }

        </div>

      </div>
    }
  `,
  styles: [`
    :host {
      position: fixed;
      bottom: 24px;
      right: 24px;
      z-index: 10000;
    }

    .chat-fab {
      width: 56px;
      height: 56px;
      border-radius: 50% !important;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.25);
    }

    .chat-panel {
      display: flex;
      flex-direction: column;
      width: 380px;
      height: 500px;
      border-radius: 12px;
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);
      overflow: hidden;
      background: #fff;
    }

    .chat-panel-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 12px 16px;
      background: #1274ac;
      color: #fff;
      font-weight: 600;
      font-size: 14px;
      flex-shrink: 0;
    }

    .chat-panel-header button {
      color: #fff;
    }

    .hide-scrollbar {
      scrollbar-width: none;
      -ms-overflow-style: none;
    }
    .hide-scrollbar::-webkit-scrollbar {
      display: none;
    }
  `]
})
export class ChatBubbleComponent implements OnInit, OnDestroy, AfterViewChecked {
  private readonly chatService = inject(PublicChatService);
  private subs: Subscription[] = [];
  private messageIdCounter = 0;

  @ViewChild('messagesContainer') messagesContainer?: ElementRef<HTMLDivElement>;

  readonly commentIcon = commentIcon;
  readonly xIcon = xIcon;
  readonly paperPlaneIcon = paperPlaneIcon;
  readonly chevronDownIcon = chevronDownIcon;
  readonly unlinkIcon = unlinkIcon;
  readonly warningTriangleIcon = warningTriangleIcon;

  readonly panelOpen = signal(false);
  readonly messages = signal<BubbleMessage[]>([]);
  readonly waitingForResponse = signal(false);
  readonly connectionError = signal(false);
  readonly llmAvailable = signal<boolean | null>(null);
  readonly llmError = signal(false);

  messageText = '';
  showScrollButton = false;

  private forceScrollToBottom = false;

  /* streaming content tracker */
  readonly currentStreamContent = signal('');

  /* prompt history (up/down arrow, backed by localStorage) */
  private promptHistory: string[] = [];
  private historyIndex = -1;
  private currentDraft = '';
  private readonly HISTORY_KEY = 'public-chat-history';
  private readonly MAX_HISTORY = 50;

  constructor() {
    // Auto-scroll when messages change
    effect(() => {
      this.messages();
      setTimeout(() => this.scrollToBottom());
    });
  }

  ngOnInit(): void {
    this.loadPromptHistory();
    this.connectAndSubscribe();
  }

  ngAfterViewChecked(): void {
    // No-op — scroll is handled via effect + setTimeout
  }

  togglePanel(): void {
    this.panelOpen.set(!this.panelOpen());
  }

  async retryConnect(): Promise<void> {
    this.connectionError.set(false);
    this.llmAvailable.set(null);
    this.llmError.set(false);
    await this.connectAndSubscribe();
  }

  sendMessage(): void {
    const text = this.messageText.trim();
    if (!text) return;

    // Save to prompt history
    this.promptHistory.push(text);
    if (this.promptHistory.length > this.MAX_HISTORY) {
      this.promptHistory = this.promptHistory.slice(-this.MAX_HISTORY);
    }
    this.savePromptHistory();
    this.historyIndex = -1;
    this.currentDraft = '';

    const userMessage: BubbleMessage = {
      id: this.nextId(),
      role: 'user',
      content: text,
      created: new Date().toISOString()
    };

    this.messages.update(msgs => [...msgs, userMessage]);
    this.messageText = '';
    this.waitingForResponse.set(true);
    this.currentStreamContent.set('');
    this.forceScrollToBottom = true;

    this.chatService.sendMessage(text).catch(() => {
      this.waitingForResponse.set(false);
    });
  }

  onEnterKey(event: Event): void {
    const keyEvent = event as KeyboardEvent;
    if (!keyEvent.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  onArrowUp(event: Event): void {
    if (this.promptHistory.length === 0) return;
    const textarea = this.getTextareaElement(event);
    if (textarea && textarea.selectionStart !== 0) return;

    event.preventDefault();
    if (this.historyIndex === -1) {
      this.currentDraft = this.messageText;
    }
    if (this.historyIndex < this.promptHistory.length - 1) {
      this.historyIndex++;
      this.messageText = this.promptHistory[this.promptHistory.length - 1 - this.historyIndex];
    }
  }

  onArrowDown(event: Event): void {
    if (this.historyIndex === -1) return;
    const textarea = this.getTextareaElement(event);
    if (textarea && textarea.selectionStart !== textarea.value.length) return;

    event.preventDefault();
    this.historyIndex--;
    if (this.historyIndex === -1) {
      this.messageText = this.currentDraft;
    } else {
      this.messageText = this.promptHistory[this.promptHistory.length - 1 - this.historyIndex];
    }
  }

  onMessagesScroll(): void {
    if (this.messagesContainer) {
      const el = this.messagesContainer.nativeElement;
      this.showScrollButton = el.scrollHeight - el.scrollTop - el.clientHeight > 80;
    }
  }

  scrollToBottomManual(): void {
    if (this.messagesContainer) {
      const el = this.messagesContainer.nativeElement;
      el.scrollTop = el.scrollHeight;
      this.showScrollButton = false;
    }
  }

  formatTime(dateStr: string): string {
    return new Date(dateStr).toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' });
  }

  ngOnDestroy(): void {
    this.subs.forEach(s => s.unsubscribe());
    this.chatService.disconnect();
  }

  // ── Private ──────────────────────────────────────────────────────────────────

  private async connectAndSubscribe(): Promise<void> {
    try {
      await this.chatService.connect();
      this.connectionError.set(false);
      this.subscribeToEvents();
      this.chatService.checkStatus().catch(() => {});
    } catch {
      this.connectionError.set(true);
    }
  }

  private subscribeToEvents(): void {
    const chunkSub = this.chatService.chunks$.subscribe(chunk => {
      if (chunk.type === 'content') {
        const updated = this.currentStreamContent() + chunk.content;
        this.currentStreamContent.set(updated);
        this.updateOrAddBotMessage(updated, true);
      }

      if (chunk.isComplete || chunk.type === 'complete') {
        this.finalizeBotMessage();
        this.waitingForResponse.set(false);
        this.currentStreamContent.set('');
      }
    });

    const errorSub = this.chatService.errors$.subscribe(() => {
      this.waitingForResponse.set(false);
    });

    const serviceErrorSub = this.chatService.serviceErrors$.subscribe(() => {
      this.llmError.set(true);
      this.waitingForResponse.set(false);
      this.currentStreamContent.set('');
    });

    const statusSub = this.chatService.status$.subscribe(status => {
      this.llmAvailable.set(status.llmAvailable);
      if (!status.llmAvailable) {
        this.llmError.set(false);
      }
    });

    const connSub = this.chatService.connectionState$.subscribe(state => {
      if (state === HubConnectionState.Disconnected) {
        this.connectionError.set(true);
      }
    });

    this.subs.push(chunkSub, errorSub, serviceErrorSub, statusSub, connSub);
  }

  private updateOrAddBotMessage(text: string, typing: boolean): void {
    this.messages.update(msgs => {
      const last = msgs[msgs.length - 1];
      if (last && last.role === 'assistant' && last.typing) {
        return [...msgs.slice(0, -1), { ...last, content: text, typing }];
      }
      return [...msgs, {
        id: this.nextId(),
        role: 'assistant',
        content: text,
        created: new Date().toISOString(),
        typing
      }];
    });
  }

  private finalizeBotMessage(): void {
    this.messages.update(msgs => {
      const last = msgs[msgs.length - 1];
      if (last && last.role === 'assistant') {
        return [...msgs.slice(0, -1), { ...last, typing: false }];
      }
      return msgs;
    });
  }

  private scrollToBottom(): void {
    if (this.messagesContainer) {
      const el = this.messagesContainer.nativeElement;
      const nearBottom = el.scrollHeight - el.scrollTop - el.clientHeight < 80;
      if (this.forceScrollToBottom || nearBottom) {
        el.scrollTop = el.scrollHeight;
        if (el.scrollHeight > el.clientHeight && !this.waitingForResponse()) {
          this.forceScrollToBottom = false;
        }
        this.showScrollButton = false;
      }
    }
  }

  private getTextareaElement(event: Event): HTMLTextAreaElement | null {
    const target = event.target as HTMLElement;
    if (target.tagName === 'TEXTAREA') return target as HTMLTextAreaElement;
    return target.querySelector('textarea');
  }

  private nextId(): number {
    return ++this.messageIdCounter;
  }

  private loadPromptHistory(): void {
    try {
      const raw = localStorage.getItem(this.HISTORY_KEY);
      this.promptHistory = raw ? JSON.parse(raw) : [];
    } catch {
      this.promptHistory = [];
    }
  }

  private savePromptHistory(): void {
    try {
      localStorage.setItem(this.HISTORY_KEY, JSON.stringify(this.promptHistory));
    } catch { }
  }
}

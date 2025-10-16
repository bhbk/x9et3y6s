import {
  Component,
  inject,
  Input,
  OnInit,
  HostListener,
  ViewChild,
  ElementRef,
  AfterViewChecked,
  effect
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_INPUTS } from '@progress/kendo-angular-inputs';
import { KENDO_INDICATORS } from '@progress/kendo-angular-indicators';
import { KENDO_ICONS } from '@progress/kendo-angular-icons';
import { AuthStore } from '../../stores/auth.store';
import { ChatStore } from '../../stores/chat.store';
import { ChatConversation, ChatFavorite } from '../../models/chat.model';
import {
  paperPlaneIcon,
  plusIcon,
  trashIcon,
  pencilIcon,
  checkIcon,
  xIcon,
  chevronLeftIcon,
  chevronRightIcon,
  chevronDownIcon,
  moreHorizontalIcon,
  pinIcon,
  unlinkIcon,
  warningTriangleIcon,
  gearIcon,
  commentIcon,
  starOutlineIcon,
  downloadIcon
} from '@progress/kendo-svg-icons';

@Component({
  selector: 'lib-chat',
  standalone: true,
  imports: [
    FormsModule,
    KENDO_BUTTONS,
    KENDO_INPUTS,
    KENDO_INDICATORS,
    KENDO_ICONS
  ],
  template: `
    <div class="flex flex-1 min-h-0 overflow-hidden bg-white">

      <!-- ── Sidebar ── -->
      @if (sidebarOpen && isFullyConnected()) {
        <div class="flex flex-col w-64 shrink-0 bg-gray-50 border-r border-gray-200">

          <!-- Collapse button -->
          <div class="flex justify-end px-2 pt-2 shrink-0">
            <button kendoButton fillMode="flat" size="small" [svgIcon]="chevronLeftIcon" title="Collapse sidebar" (click)="closeSidebar()"></button>
          </div>

          <div class="flex-1 overflow-y-auto pb-2">

            <!-- ── Favorites ── -->
            <div class="pt-3 pb-1">
              <div class="flex items-center justify-between px-4 py-1">
                <p class="text-xs font-semibold text-gray-500">Favorites</p>
                <button
                  kendoButton fillMode="flat" size="small" [svgIcon]="plusIcon"
                  title="Add favorite"
                  (click)="startAddFav()"
                ></button>
              </div>

              @for (fav of sortedFavorites; track fav.id) {
                @if (deletingFavId === fav.id) {
                  <div class="mx-2 my-0.5 px-3 py-2 rounded-xl bg-red-50 border border-red-100 text-xs">
                    <p class="text-gray-600 mb-2 truncate">Delete "{{ fav.name }}"?</p>
                    <div class="flex gap-2">
                      <button kendoButton size="small" themeColor="error" (click)="confirmDeleteFav(fav.id)">Delete</button>
                      <button kendoButton size="small" fillMode="outline" (click)="cancelDeleteFav()">Cancel</button>
                    </div>
                  </div>
                } @else if (editingFavId === fav.id) {
                  <div class="mx-2 my-0.5 px-3 py-2 rounded-xl bg-white border border-blue-300 space-y-1.5">
                    <input
                      type="text" [(ngModel)]="editingFavName" placeholder="Name"
                      class="w-full px-2 py-1 border border-gray-200 rounded-lg text-xs focus:outline-none focus:border-blue-400"
                    />
                    <textarea
                      [(ngModel)]="editingFavPrompt" placeholder="Prompt" rows="2"
                      class="w-full px-2 py-1 border border-gray-200 rounded-lg text-xs resize-none focus:outline-none focus:border-blue-400"
                    ></textarea>
                    <div class="flex gap-1.5 justify-end">
                      <button kendoButton size="small" themeColor="primary" (click)="confirmEditFav()">Save</button>
                      <button kendoButton size="small" fillMode="outline" (click)="cancelEditFav()">Cancel</button>
                    </div>
                  </div>
                } @else {
                  <div
                    class="mx-2 my-0.5 px-3 py-1.5 rounded-full text-sm text-gray-700 group flex items-center hover:bg-gray-100 transition-colors cursor-pointer"
                    (click)="sendFavorite(fav)"
                  >
                    @if (fav.pinned) {
                      <kendo-svg-icon [icon]="pinIcon" size="xsmall" class="text-blue-400 mr-1.5 shrink-0"></kendo-svg-icon>
                    } @else {
                      <kendo-svg-icon [icon]="starOutlineIcon" size="xsmall" class="text-gray-400 mr-1.5 shrink-0"></kendo-svg-icon>
                    }
                    <span class="truncate flex-1 mr-1">{{ fav.name }}</span>
                    <button
                      kendoButton fillMode="flat" size="small" [svgIcon]="moreHorizontalIcon"
                      class="opacity-0 group-hover:opacity-100 shrink-0"
                      [class.opacity-100]="openMenuId === fav.id && openMenuType === 'fav'"
                      title="More options"
                      (click)="toggleMenu($event, fav.id, 'fav')"
                    ></button>
                  </div>
                }
              }

              @if (favorites.length === 0 && !addingFavorite) {
                <p class="text-xs text-gray-400 text-center py-2 px-4">No favorites yet</p>
              }

              @if (addingFavorite) {
                <div class="mx-2 my-0.5 px-3 py-2 rounded-xl bg-white border border-blue-300 space-y-1.5">
                  <input
                    type="text" [(ngModel)]="newFavName" placeholder="Favorite name"
                    class="w-full px-2 py-1 border border-gray-200 rounded-lg text-xs focus:outline-none focus:border-blue-400"
                  />
                  <textarea
                    [(ngModel)]="newFavPrompt" placeholder="Prompt text" rows="2"
                    class="w-full px-2 py-1 border border-gray-200 rounded-lg text-xs resize-none focus:outline-none focus:border-blue-400"
                  ></textarea>
                  <div class="flex gap-1.5 justify-end">
                    <button kendoButton size="small" themeColor="primary" (click)="confirmAddFav()">Add</button>
                    <button kendoButton size="small" fillMode="outline" (click)="cancelAddFav()">Cancel</button>
                  </div>
                </div>
              }
            </div>

            <hr class="border-gray-200 mx-3 my-1" />

            <!-- ── Recent Chats ── -->
            <div class="pt-1 pb-1">
              <div class="flex items-center justify-between px-4 py-1">
                <p class="text-xs font-semibold text-gray-500">Recent Chats</p>
                <button
                  kendoButton fillMode="flat" size="small" [svgIcon]="plusIcon"
                  title="New chat"
                  [disabled]="!chatStore.isConnected() || chatStore.llmAvailable() !== true || chatStore.llmError()"
                  (click)="startNewConversation()"
                ></button>
              </div>

              @if (chatStore.isLoading() && chatStore.conversations().length === 0) {
                <div class="flex justify-center py-4">
                  <kendo-loader size="small"></kendo-loader>
                </div>
              } @else if (chatStore.conversations().length === 0) {
                <p class="text-xs text-gray-400 text-center py-2 px-4">No conversations yet</p>
              } @else {
                @for (conv of sortedConversations; track conv.id) {
                  @if (deletingConvId === conv.id) {
                    <div class="mx-2 my-0.5 px-3 py-2 rounded-xl bg-red-50 border border-red-100 text-xs">
                      <p class="text-gray-600 mb-2 truncate">Delete "{{ conv.title || 'Untitled' }}"?</p>
                      <div class="flex gap-2">
                        <button kendoButton size="small" themeColor="error" (click)="confirmDeleteConv(conv.id)">Delete</button>
                        <button kendoButton size="small" fillMode="outline" (click)="cancelDeleteConv()">Cancel</button>
                      </div>
                    </div>
                  } @else if (renamingConvId === conv.id) {
                    <div class="mx-2 my-0.5 px-2 py-1.5 flex items-center gap-1">
                      <input
                        #convRenameInput type="text"
                        [(ngModel)]="renamingConvTitle"
                        class="flex-1 text-sm px-2 py-0.5 border border-blue-400 rounded-lg focus:outline-none bg-white"
                        (keydown.enter)="confirmRenameConv()"
                        (keydown.escape)="cancelRenameConv()"
                      />
                      <button kendoButton fillMode="flat" size="small" [svgIcon]="checkIcon" class="!text-green-600 shrink-0" (click)="confirmRenameConv()"></button>
                      <button kendoButton fillMode="flat" size="small" [svgIcon]="xIcon" class="!text-gray-400 shrink-0" (click)="cancelRenameConv()"></button>
                    </div>
                  } @else {
                    <div
                      class="mx-2 my-0.5 px-3 py-1.5 rounded-full text-sm text-gray-700 group flex items-center hover:bg-gray-100 transition-colors cursor-pointer"
                      [class.bg-blue-50]="chatStore.currentConversation()?.id === conv.id"
                      [class.text-blue-700]="chatStore.currentConversation()?.id === conv.id"
                      [class.font-medium]="chatStore.currentConversation()?.id === conv.id"
                      (click)="selectConversation(conv.id)"
                    >
                      @if (isConvPinned(conv.id)) {
                        <kendo-svg-icon [icon]="pinIcon" size="xsmall" class="text-blue-400 mr-1.5 shrink-0"></kendo-svg-icon>
                      } @else {
                        <kendo-svg-icon [icon]="commentIcon" size="xsmall" class="text-gray-400 mr-1.5 shrink-0"></kendo-svg-icon>
                      }
                      <span class="truncate flex-1 mr-1">{{ conv.title || 'Untitled' }}</span>
                      <button
                        kendoButton fillMode="flat" size="small" [svgIcon]="moreHorizontalIcon"
                        class="opacity-0 group-hover:opacity-100 shrink-0"
                        [class.opacity-100]="openMenuId === conv.id && openMenuType === 'conv'"
                        title="More options"
                        (click)="toggleMenu($event, conv.id, 'conv')"
                      ></button>
                    </div>
                  }
                }
              }
            </div>

          </div>

        </div>
      }

      <!-- ── Main content ── -->
      <div class="flex flex-col flex-1 min-w-0 min-h-0 bg-white">

        @if (chatStore.llmAvailable() === null && !chatStore.error()) {
          <div class="flex-1 flex flex-col items-center justify-center gap-4 text-center px-8">
            <kendo-loader size="medium"></kendo-loader>
            <p class="text-sm text-gray-400">Connecting to assistant...</p>
          </div>

        } @else if (!chatStore.isConnected()) {
          <div class="flex-1 flex flex-col items-center justify-center gap-3 text-center px-8">
            <div class="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center">
              <kendo-svg-icon [icon]="unlinkIcon" size="xlarge" class="text-gray-400"></kendo-svg-icon>
            </div>
            <p class="text-base font-medium text-gray-600">Assistant unavailable</p>
            <p class="text-sm text-gray-400">Unable to connect to the service.</p>
          </div>

        } @else if (chatStore.llmError()) {
          <div class="flex-1 flex flex-col items-center justify-center gap-3 text-center px-8">
            <div class="w-16 h-16 bg-red-100 rounded-full flex items-center justify-center">
              <kendo-svg-icon [icon]="warningTriangleIcon" size="xlarge" class="text-red-500"></kendo-svg-icon>
            </div>
            @if (isAdmin) {
              <p class="text-base font-medium text-gray-600">Service error</p>
              <p class="text-sm text-gray-400">The assistant service is mis-configured.</p>
            } @else {
              <p class="text-base font-medium text-gray-600">Service unavailable</p>
              <p class="text-sm text-gray-400">There is a problem with the assistant service.</p>
            }
          </div>

        } @else if (chatStore.llmAvailable() === false) {
          <div class="flex-1 flex flex-col items-center justify-center gap-3 text-center px-8">
            @if (isAdmin) {
              <div class="w-16 h-16 bg-orange-100 rounded-full flex items-center justify-center">
                <kendo-svg-icon [icon]="gearIcon" size="xlarge" class="text-orange-500"></kendo-svg-icon>
              </div>
              <p class="text-base font-medium text-gray-600">No LLM configured</p>
              <p class="text-sm text-gray-400">Configure an LLM provider to enable the assistant.</p>
            } @else {
              <div class="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center">
                <kendo-svg-icon [icon]="unlinkIcon" size="xlarge" class="text-gray-400"></kendo-svg-icon>
              </div>
              <p class="text-base font-medium text-gray-600">Assistant unavailable</p>
              <p class="text-sm text-gray-400">Unable to connect to the service.</p>
            }
          </div>

        } @else {
          <!-- Connected and ready -->
          <div class="flex flex-col flex-1 min-h-0">

            @if (!chatStore.currentConversation()) {
              <!-- Welcome state -->
              <div class="flex-1 flex flex-col min-h-0">
                @if (!sidebarOpen) {
                  <div class="flex items-start p-3 shrink-0">
                    <button kendoButton fillMode="flat" size="small" [svgIcon]="chevronRightIcon" (click)="openSidebar()" title="Open sidebar"></button>
                  </div>
                }
                <div class="flex-1 flex flex-col items-center justify-center px-8 pb-12">
                  <div class="w-full max-w-2xl">
                    <p class="text-2xl font-medium text-gray-700 mb-1 text-center">Hello{{ authStore.userDisplayName() ? ', ' + authStore.userDisplayName() : '' }}</p>
                    <p class="text-base text-gray-400 mb-5 text-center">How can I help you today?</p>
                    <div class="rounded-2xl border border-gray-200 bg-white shadow-sm focus-within:border-gray-300 focus-within:shadow-md transition-all overflow-hidden">
                      <kendo-textarea
                        #welcomeInput
                        [(ngModel)]="messageText"
                        placeholder="Ask me anything about the Identity system..."
                        [style.width.%]="100"
                        [rows]="3"
                        (keydown.enter)="onEnterKey($event)"
                        [disabled]="!isFullyConnected() || chatStore.isStreaming()"
                      ></kendo-textarea>
                      <div class="flex justify-end px-3 py-2 border-t border-gray-100">
                        <button
                          kendoButton themeColor="primary" [svgIcon]="paperPlaneIcon"
                          [disabled]="!messageText.trim() || !isFullyConnected() || chatStore.isStreaming()"
                          (click)="sendMessage()"
                          style="border-radius:50%"
                        ></button>
                      </div>
                    </div>
                    <p class="text-[11px] text-gray-400 mt-2">Shift+Enter for new line</p>
                  </div>
                </div>
              </div>

            } @else {
              <!-- Active conversation -->
              <div class="relative flex-1 min-h-0">

                @if (!sidebarOpen) {
                  <div class="absolute top-0 left-0 z-10 p-3">
                    <button kendoButton fillMode="flat" size="small" [svgIcon]="chevronRightIcon" (click)="openSidebar()" title="Open sidebar"></button>
                  </div>
                }

                <!-- Messages: fills entire area, scrollbar hidden -->
                <div #messagesContainer class="absolute inset-0 overflow-y-auto hide-scrollbar" (scroll)="onMessagesScroll()">
                  <div class="max-w-3xl mx-auto px-6 py-8 pb-52 space-y-6">

                    @for (msg of chatStore.displayMessages(); track msg.id) {
                      @if (msg.role === 'user') {
                        <div class="flex justify-end">
                          <div class="max-w-[75%] bg-blue-50 rounded-2xl px-4 py-3 text-sm text-gray-800 whitespace-pre-wrap leading-relaxed">
                            {{ msg.content }}
                          </div>
                        </div>
                      } @else {
                        <div>
                          <p class="text-sm text-gray-800 whitespace-pre-wrap leading-relaxed">{{ msg.content }}</p>
                          @if (msg.files?.length) {
                            <div class="mt-2 space-y-1.5">
                              @for (file of msg.files; track file.fileId) {
                                <div class="inline-flex items-center gap-2 px-3 py-1.5 bg-gray-50 border border-gray-200 rounded-lg text-sm">
                                  <kendo-svg-icon [icon]="downloadIcon" size="small" class="text-blue-500"></kendo-svg-icon>
                                  <button class="text-blue-600 hover:text-blue-800 hover:underline cursor-pointer"
                                          (click)="onDownloadFile(file.fileId, file.fileName)">
                                    {{ file.fileName }}
                                  </button>
                                  <span class="text-xs text-gray-400">({{ formatFileSize(file.fileSize) }})</span>
                                </div>
                              }
                            </div>
                          }
                          @if (isAdmin && (msg.inputTokens !== null || msg.outputTokens !== null)) {
                            <p class="text-[11px] text-gray-400 mt-2">
                              {{ msg.inputTokens || 0 }} in / {{ msg.outputTokens || 0 }} out tokens
                            </p>
                          }
                        </div>
                      }
                    }

                    @if (chatStore.isStreaming() && !chatStore.streamingContent()) {
                      <div class="flex items-center gap-1">
                        <span class="w-2 h-2 rounded-full bg-gray-300 animate-bounce" style="animation-delay:0ms"></span>
                        <span class="w-2 h-2 rounded-full bg-gray-300 animate-bounce" style="animation-delay:150ms"></span>
                        <span class="w-2 h-2 rounded-full bg-gray-300 animate-bounce" style="animation-delay:300ms"></span>
                      </div>
                    }

                  </div>
                </div>

                <!-- Floating input with gradient fade -->
                <div class="absolute bottom-0 left-0 right-0 px-6 pb-6 pt-16 bg-gradient-to-t from-white via-white to-transparent pointer-events-none">
                  <div class="pointer-events-auto">

                    <!-- Scroll-to-bottom button -->
                    @if (showScrollButton) {
                      <div class="flex justify-center mb-3">
                        <button
                          kendoButton fillMode="outline" size="small" [svgIcon]="chevronDownIcon"
                          class="rounded-full bg-white shadow-md"
                          title="Scroll to bottom"
                          (click)="scrollToBottomManual()"
                        ></button>
                      </div>
                    }

                    @if (chatStore.error()) {
                      <div class="max-w-3xl mx-auto mb-3 px-4 py-2 bg-red-50 border border-red-100 rounded-xl flex items-center justify-between text-xs text-red-600">
                        <span>{{ chatStore.error() }}</span>
                        <button kendoButton fillMode="flat" size="small" (click)="chatStore.clearError()">✕</button>
                      </div>
                    }
                    <div class="max-w-3xl mx-auto">
                      <div class="rounded-2xl border border-gray-200 bg-white shadow-sm focus-within:border-gray-300 focus-within:shadow-md transition-all overflow-hidden">
                        <kendo-textarea
                          #messageInput
                          [(ngModel)]="messageText"
                          placeholder="Ask me anything about the Identity system..."
                          [style.width.%]="100"
                          [rows]="3"
                          (keydown.enter)="onEnterKey($event)"
                          [disabled]="!isFullyConnected() || chatStore.isStreaming()"
                        ></kendo-textarea>
                        <div class="flex justify-end px-3 py-2 border-t border-gray-100">
                          <button
                            kendoButton themeColor="primary" [svgIcon]="paperPlaneIcon"
                            [disabled]="!messageText.trim() || !isFullyConnected() || chatStore.isStreaming()"
                            (click)="sendMessage()"
                            style="border-radius:50%"
                          ></button>
                        </div>
                      </div>
                      <p class="text-[11px] text-gray-400 mt-2">Shift+Enter for new line</p>
                    </div>
                  </div>
                </div>

              </div>
            }
          </div>
        }

      </div>

      <!-- ── Context menu (fixed, escapes sidebar overflow) ── -->
      @if (openMenuId) {
        <div
          class="fixed z-50 bg-white rounded-xl shadow-lg border border-gray-100 py-1.5 min-w-[152px]"
          [style.top.px]="menuTop"
          [style.left.px]="menuLeft"
          (click)="$event.stopPropagation()"
        >
          @if (openMenuType === 'fav') {
            <button class="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 flex items-center gap-2.5 rounded-lg" (click)="menuFavEdit()">
              <kendo-svg-icon [icon]="pencilIcon" size="small" class="text-gray-500"></kendo-svg-icon>
              Edit
            </button>
            <button class="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 flex items-center gap-2.5 rounded-lg" (click)="menuFavPin()">
              <kendo-svg-icon [icon]="pinIcon" size="small" class="text-gray-500"></kendo-svg-icon>
              {{ isFavPinnedForMenu() ? 'Unpin' : 'Pin' }}
            </button>
            <hr class="border-gray-100 my-1" />
            <button class="w-full text-left px-4 py-2 text-sm text-red-500 hover:bg-red-50 flex items-center gap-2.5 rounded-lg" (click)="menuFavDelete()">
              <kendo-svg-icon [icon]="trashIcon" size="small"></kendo-svg-icon>
              Delete
            </button>
          } @else if (openMenuType === 'conv') {
            <button class="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 flex items-center gap-2.5 rounded-lg" (click)="menuConvRename()">
              <kendo-svg-icon [icon]="pencilIcon" size="small" class="text-gray-500"></kendo-svg-icon>
              Rename
            </button>
            <button class="w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 flex items-center gap-2.5 rounded-lg" (click)="menuConvPin()">
              <kendo-svg-icon [icon]="pinIcon" size="small" class="text-gray-500"></kendo-svg-icon>
              {{ isConvPinnedForMenu() ? 'Unpin' : 'Pin' }}
            </button>
            <hr class="border-gray-100 my-1" />
            <button class="w-full text-left px-4 py-2 text-sm text-red-500 hover:bg-red-50 flex items-center gap-2.5 rounded-lg" (click)="menuConvDelete()">
              <kendo-svg-icon [icon]="trashIcon" size="small"></kendo-svg-icon>
              Delete
            </button>
          }
        </div>
      }

    </div>
  `,
  styles: [`
    :host {
      display: flex;
      flex-direction: column;
      flex: 1;
      min-height: 0;
      overflow: hidden;
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
export class ChatComponent implements OnInit, AfterViewChecked {
  @Input() isAdmin = false;

  readonly authStore = inject(AuthStore);
  readonly chatStore = inject(ChatStore);

  @ViewChild('messagesContainer') messagesContainer?: ElementRef<HTMLDivElement>;
  @ViewChild('convRenameInput') convRenameInputRef?: ElementRef<HTMLInputElement>;

  readonly paperPlaneIcon = paperPlaneIcon;
  readonly plusIcon = plusIcon;
  readonly trashIcon = trashIcon;
  readonly pencilIcon = pencilIcon;
  readonly checkIcon = checkIcon;
  readonly xIcon = xIcon;
  readonly chevronLeftIcon = chevronLeftIcon;
  readonly chevronRightIcon = chevronRightIcon;
  readonly chevronDownIcon = chevronDownIcon;
  readonly moreHorizontalIcon = moreHorizontalIcon;
  readonly pinIcon = pinIcon;
  readonly unlinkIcon = unlinkIcon;
  readonly warningTriangleIcon = warningTriangleIcon;
  readonly gearIcon = gearIcon;
  readonly commentIcon = commentIcon;
  readonly starOutlineIcon = starOutlineIcon;
  readonly downloadIcon = downloadIcon;

  messageText = '';
  sidebarOpen = true;
  showScrollButton = false;

  // Favorites state
  favorites: ChatFavorite[] = [];
  addingFavorite = false;
  newFavName = '';
  newFavPrompt = '';
  editingFavId: string | null = null;
  editingFavName = '';
  editingFavPrompt = '';
  deletingFavId: string | null = null;

  // Conversation rename/delete state
  renamingConvId: string | null = null;
  renamingConvTitle = '';
  deletingConvId: string | null = null;

  // Pinned conversations (localStorage-persisted set of ids)
  pinnedConvIds = new Set<string>();

  // Context menu state
  openMenuId: string | null = null;
  openMenuType: 'conv' | 'fav' | null = null;
  menuTop = 0;
  menuLeft = 0;

  private pendingMessage = '';
  private forceScrollToBottom = false;
  private shouldFocusRename = false;

  private readonly defaultAdminFavorites: ChatFavorite[] = [
    { id: 'default-1', name: 'User count', prompt: 'How many users are in the system?' },
    { id: 'default-2', name: 'Recent activity', prompt: 'Show recent authentication activity' },
    { id: 'default-3', name: 'System roles', prompt: 'What roles exist in the system?' },
    { id: 'default-4', name: 'List audiences', prompt: 'List all audiences' }
  ];

  private readonly defaultUserFavorites: ChatFavorite[] = [
    { id: 'default-1', name: 'My roles', prompt: 'What roles am I assigned to?' },
    { id: 'default-2', name: 'Login activity', prompt: 'Show my recent login activity' },
    { id: 'default-3', name: 'My profile', prompt: 'What are my profile details?' },
    { id: 'default-4', name: 'Active sessions', prompt: 'Do I have any active sessions?' }
  ];

  private get favStorageKey(): string {
    return this.isAdmin ? 'chat-favorites-admin' : 'chat-favorites-user';
  }

  private get pinStorageKey(): string {
    return this.isAdmin ? 'chat-pinned-convs-admin' : 'chat-pinned-convs-user';
  }

  constructor() {
    effect(() => {
      const conv = this.chatStore.currentConversation();
      if (conv && this.pendingMessage) {
        const msg = this.pendingMessage;
        this.pendingMessage = '';
        this.chatStore.sendMessage(msg);
      }
    });

    // When the selected conversation changes, force-scroll to the bottom
    // so returning to a conversation always shows the most recent messages.
    effect(() => {
      this.chatStore.currentConversation();
      this.forceScrollToBottom = true;
    });

    effect(() => {
      this.chatStore.displayMessages();
      // Use setTimeout(0) so the scroll runs after Angular finishes rendering
      // the updated view. Relying on ngAfterViewChecked is unreliable because
      // effects run as microtasks *after* lifecycle hooks in the same CD cycle.
      setTimeout(() => this.scrollToBottom());
    });
  }

  ngOnInit(): void {
    this.loadFavorites();
    this.loadPinnedConvs();
    this.chatStore.init();
    this.chatStore.loadConversations({});
  }

  ngAfterViewChecked(): void {
    if (this.shouldFocusRename && this.convRenameInputRef) {
      this.convRenameInputRef.nativeElement.focus();
      this.shouldFocusRename = false;
    }
  }

  private scrollToBottom(): void {
    if (this.messagesContainer) {
      const el = this.messagesContainer.nativeElement;
      const nearBottom = el.scrollHeight - el.scrollTop - el.clientHeight < 100;
      if (this.forceScrollToBottom || nearBottom) {
        el.scrollTop = el.scrollHeight;
        // Keep the flag active while: (a) the container has no scrollable
        // content yet (empty/loading state), or (b) a response is still
        // streaming in.  This ensures the entire send→response cycle stays
        // pinned to the bottom.
        if (el.scrollHeight > el.clientHeight && !this.chatStore.isStreaming()) {
          this.forceScrollToBottom = false;
        }
        this.showScrollButton = false;
      }
    }
  }

  onMessagesScroll(): void {
    if (this.messagesContainer) {
      const el = this.messagesContainer.nativeElement;
      this.showScrollButton = el.scrollHeight - el.scrollTop - el.clientHeight > 100;
    }
  }

  scrollToBottomManual(): void {
    if (this.messagesContainer) {
      const el = this.messagesContainer.nativeElement;
      el.scrollTop = el.scrollHeight;
      this.showScrollButton = false;
    }
  }

  isFullyConnected(): boolean {
    return this.chatStore.isConnected() && this.chatStore.llmAvailable() === true && !this.chatStore.llmError();
  }

  // ── Context menu ────────────────────────────────────────────────────────────

  @HostListener('document:click')
  onDocumentClick(): void {
    if (this.openMenuId) {
      this.openMenuId = null;
      this.openMenuType = null;
    }
  }

  toggleMenu(event: Event, id: string, type: 'conv' | 'fav'): void {
    event.stopPropagation();
    if (this.openMenuId === id && this.openMenuType === type) {
      this.openMenuId = null;
      this.openMenuType = null;
      return;
    }
    const rect = (event.currentTarget as HTMLElement).getBoundingClientRect();
    this.menuTop = rect.bottom + 4;
    this.menuLeft = rect.right - 152;
    this.openMenuId = id;
    this.openMenuType = type;
  }

  closeMenu(): void {
    this.openMenuId = null;
    this.openMenuType = null;
  }

  // Favorite menu actions
  menuFavEdit(): void {
    const fav = this.favorites.find(f => f.id === this.openMenuId);
    this.closeMenu();
    if (fav) this.startEditFav(fav);
  }

  menuFavPin(): void {
    const id = this.openMenuId;
    this.closeMenu();
    if (id) this.togglePinFav(id);
  }

  menuFavDelete(): void {
    const id = this.openMenuId;
    this.closeMenu();
    if (id) this.startDeleteFav(id);
  }

  isFavPinnedForMenu(): boolean {
    return this.favorites.find(f => f.id === this.openMenuId)?.pinned ?? false;
  }

  // Conversation menu actions
  menuConvRename(): void {
    const conv = this.chatStore.conversations().find(c => c.id === this.openMenuId);
    this.closeMenu();
    if (conv) this.startRenameConv(conv);
  }

  menuConvPin(): void {
    const id = this.openMenuId;
    this.closeMenu();
    if (id) this.togglePinConv(id);
  }

  menuConvDelete(): void {
    const id = this.openMenuId;
    this.closeMenu();
    if (id) this.startDeleteConv(id);
  }

  isConvPinnedForMenu(): boolean {
    return this.openMenuId ? this.pinnedConvIds.has(this.openMenuId) : false;
  }

  // ── Sorted lists ────────────────────────────────────────────────────────────

  get sortedFavorites(): ChatFavorite[] {
    const pinned = this.favorites.filter(f => f.pinned);
    const unpinned = this.favorites.filter(f => !f.pinned);
    return [...pinned, ...unpinned];
  }

  get sortedConversations(): ChatConversation[] {
    const convs = this.chatStore.conversations();
    const pinned = convs.filter(c => this.pinnedConvIds.has(c.id));
    const unpinned = convs.filter(c => !this.pinnedConvIds.has(c.id));
    return [...pinned, ...unpinned];
  }

  // ── Favorites ──────────────────────────────────────────────────────────────

  private loadFavorites(): void {
    try {
      const raw = localStorage.getItem(this.favStorageKey);
      if (raw) {
        this.favorites = JSON.parse(raw);
      } else {
        const defaults = this.isAdmin ? this.defaultAdminFavorites : this.defaultUserFavorites;
        this.favorites = defaults.map(f => ({ ...f }));
        this.saveFavorites();
      }
    } catch {
      this.favorites = [];
    }
  }

  private saveFavorites(): void {
    try {
      localStorage.setItem(this.favStorageKey, JSON.stringify(this.favorites));
    } catch { }
  }

  sendFavorite(fav: ChatFavorite): void {
    if (!this.isFullyConnected()) return;
    const prompt = fav.prompt.trim();
    if (!prompt) return;

    this.messageText = '';
    this.forceScrollToBottom = true;

    // If already viewing the conversation dedicated to this favorite, send directly
    const current = this.chatStore.currentConversation();
    if (current?.title === fav.name) {
      this.chatStore.sendMessage(prompt);
      return;
    }

    // Reuse an existing conversation that was named after this favorite
    const existing = this.chatStore.conversations().find(c => c.title === fav.name);
    this.pendingMessage = prompt;
    if (existing) {
      this.chatStore.selectConversation(existing.id);
    } else {
      this.chatStore.createConversation({ title: fav.name });
    }
  }

  startAddFav(): void {
    this.addingFavorite = true;
    this.newFavName = '';
    this.newFavPrompt = '';
    this.editingFavId = null;
    this.deletingFavId = null;
  }

  confirmAddFav(): void {
    const name = this.newFavName.trim();
    const prompt = this.newFavPrompt.trim();
    if (!name || !prompt) return;
    this.favorites = [...this.favorites, { id: crypto.randomUUID(), name, prompt }];
    this.saveFavorites();
    this.addingFavorite = false;
    this.newFavName = '';
    this.newFavPrompt = '';
  }

  cancelAddFav(): void {
    this.addingFavorite = false;
    this.newFavName = '';
    this.newFavPrompt = '';
  }

  startEditFav(fav: ChatFavorite): void {
    this.editingFavId = fav.id;
    this.editingFavName = fav.name;
    this.editingFavPrompt = fav.prompt;
    this.deletingFavId = null;
    this.addingFavorite = false;
  }

  confirmEditFav(): void {
    const name = this.editingFavName.trim();
    const prompt = this.editingFavPrompt.trim();
    if (!name || !prompt || !this.editingFavId) return;
    this.favorites = this.favorites.map(f =>
      f.id === this.editingFavId ? { ...f, name, prompt } : f
    );
    this.saveFavorites();
    this.editingFavId = null;
  }

  cancelEditFav(): void {
    this.editingFavId = null;
  }

  startDeleteFav(id: string): void {
    this.deletingFavId = id;
    this.editingFavId = null;
    this.addingFavorite = false;
  }

  confirmDeleteFav(id: string): void {
    this.favorites = this.favorites.filter(f => f.id !== id);
    this.saveFavorites();
    this.deletingFavId = null;
  }

  cancelDeleteFav(): void {
    this.deletingFavId = null;
  }

  togglePinFav(id: string): void {
    this.favorites = this.favorites.map(f =>
      f.id === id ? { ...f, pinned: !f.pinned } : f
    );
    this.saveFavorites();
  }

  isFavPinned(id: string): boolean {
    return this.favorites.find(f => f.id === id)?.pinned ?? false;
  }

  // ── Conversations ───────────────────────────────────────────────────────────

  startRenameConv(conv: ChatConversation): void {
    this.renamingConvId = conv.id;
    this.renamingConvTitle = conv.title || '';
    this.deletingConvId = null;
    this.shouldFocusRename = true;
  }

  confirmRenameConv(): void {
    const title = this.renamingConvTitle.trim();
    if (!title || !this.renamingConvId) {
      this.cancelRenameConv();
      return;
    }
    this.chatStore.renameConversation(this.renamingConvId, title);
    this.renamingConvId = null;
    this.renamingConvTitle = '';
  }

  cancelRenameConv(): void {
    this.renamingConvId = null;
    this.renamingConvTitle = '';
  }

  startDeleteConv(id: string): void {
    this.deletingConvId = id;
    this.renamingConvId = null;
  }

  confirmDeleteConv(id: string): void {
    this.deletingConvId = null;
    this.pinnedConvIds.delete(id);
    this.savePinnedConvs();
    this.chatStore.deleteConversation(id);
  }

  cancelDeleteConv(): void {
    this.deletingConvId = null;
  }

  togglePinConv(id: string): void {
    if (this.pinnedConvIds.has(id)) {
      this.pinnedConvIds.delete(id);
    } else {
      this.pinnedConvIds.add(id);
    }
    this.pinnedConvIds = new Set(this.pinnedConvIds); // trigger change detection
    this.savePinnedConvs();
  }

  isConvPinned(id: string): boolean {
    return this.pinnedConvIds.has(id);
  }

  private loadPinnedConvs(): void {
    try {
      const raw = localStorage.getItem(this.pinStorageKey);
      this.pinnedConvIds = new Set(raw ? JSON.parse(raw) : []);
    } catch {
      this.pinnedConvIds = new Set();
    }
  }

  private savePinnedConvs(): void {
    try {
      localStorage.setItem(this.pinStorageKey, JSON.stringify([...this.pinnedConvIds]));
    } catch { }
  }

  // ── Sidebar ─────────────────────────────────────────────────────────────────

  openSidebar(): void {
    this.sidebarOpen = true;
  }

  closeSidebar(): void {
    this.sidebarOpen = false;
  }

  startNewConversation(): void {
    this.chatStore.clearCurrentConversation();
  }

  selectConversation(id: string): void {
    this.forceScrollToBottom = true;
    this.chatStore.selectConversation(id);
  }

  // ── Messaging ───────────────────────────────────────────────────────────────

  sendMessage(): void {
    const text = this.messageText.trim();
    if (!text) return;
    this.messageText = '';
    this.forceScrollToBottom = true;

    if (!this.chatStore.currentConversation()) {
      this.pendingMessage = text;
      this.chatStore.createConversation({ title: text.slice(0, 50) });
      return;
    }

    this.chatStore.sendMessage(text);
  }

  onEnterKey(event: Event): void {
    const keyEvent = event as KeyboardEvent;
    if (!keyEvent.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  onDownloadFile(fileId: string, fileName: string): void {
    this.chatStore.downloadFile(fileId, fileName);
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }
}

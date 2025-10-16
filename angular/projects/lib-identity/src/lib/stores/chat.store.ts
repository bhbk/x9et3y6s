import { computed, inject } from '@angular/core';
import { signalStore, withState, withComputed, withMethods, patchState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { pipe, switchMap, tap, catchError, of, takeUntil, Subject } from 'rxjs';
import { HubConnectionState } from '@microsoft/signalr';
import { ChatService } from '../services/chat.service';
import { ChatState, ChatConversation, ChatMessage, ChatStreamChunk, ChatLLMStatus, ChatFileReference } from '../models/chat.model';

const initialState: ChatState = {
  conversations: [],
  currentConversation: null,
  messages: [],
  isConnected: false,
  llmAvailable: null,
  llmError: false,
  isStreaming: false,
  streamingContent: '',
  isLoading: false,
  error: null,
  pendingFiles: []
};

export const ChatStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed((store) => ({
    hasConversations: computed(() => store.conversations().length > 0),
    hasCurrentConversation: computed(() => store.currentConversation() !== null),
    sortedMessages: computed(() => {
      const messages = store.messages();
      return [...messages].sort((a, b) =>
        new Date(a.createdUtc).getTime() - new Date(b.createdUtc).getTime()
      );
    }),
    displayMessages: computed(() => {
      const msgs = store.messages();
      const sorted = [...msgs].sort((a, b) =>
        new Date(a.createdUtc).getTime() - new Date(b.createdUtc).getTime()
      );
      const streamingContent = store.streamingContent();
      const isStreaming = store.isStreaming();

      // If streaming, append a temporary assistant message with current content
      if (isStreaming && streamingContent) {
        return [
          ...sorted,
          {
            id: 'streaming',
            role: 'assistant' as const,
            content: streamingContent,
            inputTokens: null,
            outputTokens: null,
            createdUtc: new Date().toISOString()
          }
        ];
      }
      return sorted;
    })
  })),
  withMethods((store, chatService = inject(ChatService)) => {
    let destroySubject = new Subject<void>();
    let initialized = false;

    // Subscribe to connection state changes
    const setupConnectionTracking = () => {
      chatService.connectionState$.pipe(
        takeUntil(destroySubject)
      ).subscribe(state => {
        patchState(store, {
          isConnected: state === HubConnectionState.Connected
        });
      });
    };

    // Subscribe to chunk stream
    const setupChunkListener = () => {
      chatService.chunks$.pipe(
        takeUntil(destroySubject)
      ).subscribe((chunk: ChatStreamChunk) => {
        console.log('[ChatStore] chunk:', chunk.type, chunk.isComplete, JSON.stringify(chunk.content)?.slice(0, 80));
        handleChunk(chunk);
      });
    };

    // Subscribe to errors
    const setupErrorListener = () => {
      chatService.errors$.pipe(
        takeUntil(destroySubject)
      ).subscribe(error => {
        console.error('[ChatStore] Hub Error event:', error);
        patchState(store, { error, isStreaming: false, streamingContent: '' });
      });
    };

    // Subscribe to LLM status
    const setupStatusListener = () => {
      chatService.status$.pipe(
        takeUntil(destroySubject)
      ).subscribe((status: ChatLLMStatus) => {
        patchState(store, { llmAvailable: status.llmAvailable });
      });
    };

    // Subscribe to service errors (sanitized LLM failures)
    const setupServiceErrorListener = () => {
      chatService.serviceErrors$.pipe(
        takeUntil(destroySubject)
      ).subscribe((code: string) => {
        console.error('[ChatStore] Hub ServiceError event:', code);
        patchState(store, { llmError: true, isStreaming: false, streamingContent: '' });
      });
    };

    const setupTitleUpdateListener = () => {
      chatService.titleUpdates$.pipe(
        takeUntil(destroySubject)
      ).subscribe(({ conversationId, title }) => {
        patchState(store, {
          conversations: store.conversations().map(c =>
            c.id === conversationId ? { ...c, title } : c
          ),
          currentConversation: store.currentConversation()?.id === conversationId
            ? { ...store.currentConversation()!, title }
            : store.currentConversation()
        });
      });
    };

    // Handle incoming chunks
    const handleChunk = (chunk: ChatStreamChunk) => {
      switch (chunk.type) {
        case 'content':
          patchState(store, {
            streamingContent: store.streamingContent() + chunk.content,
            isStreaming: true
          });
          break;

        case 'tool_call':
        case 'tool_result':
          // Tool activity is internal — keep the typing indicator active but don't
          // expose raw tool call/result text in the message stream
          patchState(store, { isStreaming: true });
          break;

        case 'file':
          if (chunk.fileId && chunk.fileName) {
            const fileRef: ChatFileReference = {
              fileId: chunk.fileId,
              fileName: chunk.fileName,
              contentType: '',
              fileSize: chunk.fileSize ?? 0,
              summary: chunk.content ?? null
            };
            patchState(store, { pendingFiles: [...store.pendingFiles(), fileRef] });
          }
          break;

        case 'complete':
          console.log('[ChatStore] complete chunk, streamingContent length:', store.streamingContent().length);
          // Check if this is a ConversationCreated event
          if (chunk.content?.startsWith('{')) {
            try {
              const eventData = JSON.parse(chunk.content);
              if (eventData.event === 'ConversationCreated') {
                patchState(store, {
                  conversations: [eventData.data, ...store.conversations()],
                  currentConversation: eventData.data,
                  messages: [],
                  isStreaming: false,
                  streamingContent: ''
                });
                return;
              }
            } catch {
              // Not a JSON event, treat as normal completion
            }
          }

          // Normal message completion - add the complete message
          if (store.streamingContent()) {
            const files = store.pendingFiles().length > 0 ? [...store.pendingFiles()] : undefined;
            const newMessage: ChatMessage = {
              id: crypto.randomUUID(),
              role: 'assistant',
              content: store.streamingContent(),
              inputTokens: chunk.inputTokens,
              outputTokens: chunk.outputTokens,
              createdUtc: new Date().toISOString(),
              files
            };
            patchState(store, {
              messages: [...store.messages(), newMessage],
              isStreaming: false,
              streamingContent: '',
              pendingFiles: []
            });
          } else {
            patchState(store, {
              isStreaming: false,
              streamingContent: '',
              pendingFiles: []
            });
          }
          break;

        case 'error':
          patchState(store, {
            error: chunk.content,
            isStreaming: false,
            streamingContent: ''
          });
          break;
      }
    };

    return {
      // Initialize subscriptions (idempotent — guard against double init)
      init: () => {
        if (initialized) {
          console.warn('[ChatStore] init() called more than once — skipping duplicate setup');
          return;
        }
        initialized = true;
        setupConnectionTracking();
        setupChunkListener();
        setupErrorListener();
        setupStatusListener();
        setupServiceErrorListener();
        setupTitleUpdateListener();
      },

      // Cleanup subscriptions
      destroy: () => {
        destroySubject.next();
        destroySubject.complete();
        // Reset so init() works again after navigation back to this page
        initialized = false;
        destroySubject = new Subject<void>();
      },

      // Connect to SignalR hub
      connect: async (isAdmin: boolean) => {
        patchState(store, { isLoading: true, error: null });
        try {
          await chatService.connect(isAdmin);
          patchState(store, { isLoading: false, isConnected: true });
          // Check if LLM is available on the server
          await chatService.checkStatus();
        } catch (err) {
          const message = err instanceof Error ? err.message : 'Failed to connect';
          patchState(store, { isLoading: false, error: message });
        }
      },

      // Disconnect from SignalR hub
      disconnect: async () => {
        await chatService.disconnect();
        patchState(store, { isConnected: false });
      },

      // Load conversations
      loadConversations: rxMethod<{ skip?: number; take?: number }>(
        pipe(
          tap(() => patchState(store, { isLoading: true, error: null })),
          switchMap(({ skip, take }) =>
            chatService.getConversations(skip ?? 0, take ?? 20).pipe(
              tap(conversations => {
                patchState(store, { conversations, isLoading: false });
              }),
              catchError(error => {
                const message = error?.message ?? 'Failed to load conversations';
                patchState(store, { error: message, isLoading: false });
                return of(null);
              })
            )
          )
        )
      ),

      // Select a conversation and load its messages
      selectConversation: rxMethod<string>(
        pipe(
          tap(() => patchState(store, { isLoading: true, error: null })),
          switchMap(id =>
            chatService.getConversation(id).pipe(
              tap(conversation => {
                patchState(store, { currentConversation: conversation });
              }),
              switchMap(() => chatService.getMessages(id)),
              tap(messages => {
                patchState(store, { messages, isLoading: false });
              }),
              catchError(error => {
                const message = error?.message ?? 'Failed to load conversation';
                patchState(store, { error: message, isLoading: false });
                return of(null);
              })
            )
          )
        )
      ),

      // Create a new conversation
      createConversation: rxMethod<{ title?: string }>(
        pipe(
          tap(() => patchState(store, { isLoading: true, error: null })),
          switchMap(({ title }) =>
            chatService.createConversation({ title }).pipe(
              tap(conversation => {
                patchState(store, {
                  conversations: [conversation, ...store.conversations()],
                  currentConversation: conversation,
                  messages: [],
                  isLoading: false
                });
              }),
              catchError(error => {
                const message = error?.message ?? 'Failed to create conversation';
                patchState(store, { error: message, isLoading: false });
                return of(null);
              })
            )
          )
        )
      ),

      // Delete a conversation
      deleteConversation: rxMethod<string>(
        pipe(
          tap(() => patchState(store, { isLoading: true, error: null })),
          switchMap(id =>
            chatService.deleteConversation(id).pipe(
              tap(() => {
                const conversations = store.conversations().filter(c => c.id !== id);
                const currentConversation = store.currentConversation();
                patchState(store, {
                  conversations,
                  currentConversation: currentConversation?.id === id ? null : currentConversation,
                  messages: currentConversation?.id === id ? [] : store.messages(),
                  isLoading: false
                });
              }),
              catchError(error => {
                const message = error?.message ?? 'Failed to delete conversation';
                patchState(store, { error: message, isLoading: false });
                return of(null);
              })
            )
          )
        )
      ),

      // Send a message via SignalR
      sendMessage: async (message: string) => {
        const conversationId = store.currentConversation()?.id;
        if (!conversationId) {
          patchState(store, { error: 'No conversation selected' });
          return;
        }

        if (!chatService.isConnected) {
          patchState(store, { error: 'Not connected to chat' });
          return;
        }

        // Add user message immediately
        const userMessage: ChatMessage = {
          id: crypto.randomUUID(),
          role: 'user',
          content: message,
          inputTokens: null,
          outputTokens: null,
          createdUtc: new Date().toISOString()
        };

        patchState(store, {
          messages: [...store.messages(), userMessage],
          isStreaming: true,
          streamingContent: '',
          error: null
        });

        try {
          await chatService.sendMessage(conversationId, message);
        } catch (err) {
          const errorMessage = err instanceof Error ? err.message : 'Failed to send message';
          patchState(store, { error: errorMessage, isStreaming: false });
        }
      },

      // Clear current conversation
      clearCurrentConversation: () => {
        patchState(store, {
          currentConversation: null,
          messages: [],
          streamingContent: '',
          isStreaming: false
        });
      },

      // Rename a conversation (client-side optimistic; resets on reload)
      renameConversation: (id: string, title: string) => {
        patchState(store, {
          conversations: store.conversations().map(c =>
            c.id === id ? { ...c, title } : c
          ),
          currentConversation: store.currentConversation()?.id === id
            ? { ...store.currentConversation()!, title }
            : store.currentConversation()
        });
      },

      // Download a file generated by the assistant
      downloadFile: (fileId: string, fileName: string) => {
        chatService.downloadFile(fileId).subscribe({
          next: (blob) => {
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = fileName;
            a.click();
            URL.revokeObjectURL(url);
          },
          error: (err) => {
            const message = err?.message ?? 'Failed to download file';
            patchState(store, { error: message });
          }
        });
      },

      // Clear error
      clearError: () => {
        patchState(store, { error: null });
      }
    };
  })
);

import { Injectable, inject, NgZone } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject, BehaviorSubject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { ConfigService } from './config.service';
import { AuthStore } from '../stores/auth.store';
import { ChatConversation, ChatMessage, ChatStreamChunk, ChatLLMStatus, CreateConversationRequest } from '../models/chat.model';

/**
 * Chat service for REST API calls and SignalR streaming
 * Supports both Admin and User API endpoints based on configuration
 */
@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);
  private readonly authStore = inject(AuthStore);
  private readonly ngZone = inject(NgZone);

  private hubConnection: signalR.HubConnection | null = null;
  private _isAdmin = false;
  private readonly _chunks$ = new Subject<ChatStreamChunk>();
  private readonly _connectionState$ = new BehaviorSubject<signalR.HubConnectionState>(signalR.HubConnectionState.Disconnected);
  private readonly _errors$ = new Subject<string>();
  private readonly _serviceErrors$ = new Subject<string>();
  private readonly _status$ = new Subject<ChatLLMStatus>();
  private readonly _titleUpdates$ = new Subject<{ conversationId: string; title: string }>();

  /** Stream of incoming chat chunks */
  readonly chunks$ = this._chunks$.asObservable();

  /** Current SignalR connection state */
  readonly connectionState$ = this._connectionState$.asObservable();

  /** Stream of SignalR errors */
  readonly errors$ = this._errors$.asObservable();

  /** Stream of sanitized service errors (LLM misconfiguration, etc.) */
  readonly serviceErrors$ = this._serviceErrors$.asObservable();

  /** Stream of LLM status updates */
  readonly status$ = this._status$.asObservable();

  readonly titleUpdates$ = this._titleUpdates$.asObservable();

  /**
   * Get the base API URL based on the app context set at connect() time
   */
  private getBaseUrl(): string {
    return this._isAdmin ? this.config.adminApiUrl : this.config.userApiUrl;
  }

  /**
   * Get all conversations for the current user
   */
  getConversations(skip = 0, take = 20): Observable<ChatConversation[]> {
    return this.http.get<ChatConversation[]>(
      `${this.getBaseUrl()}${this.config.pathBase}/chat/conversations`,
      { params: { skip: skip.toString(), take: take.toString() } }
    );
  }

  /**
   * Get a specific conversation by ID
   */
  getConversation(id: string): Observable<ChatConversation> {
    return this.http.get<ChatConversation>(
      `${this.getBaseUrl()}${this.config.pathBase}/chat/conversations/${id}`
    );
  }

  /**
   * Create a new conversation
   */
  createConversation(request?: CreateConversationRequest): Observable<ChatConversation> {
    return this.http.post<ChatConversation>(
      `${this.getBaseUrl()}${this.config.pathBase}/chat/conversations`,
      request ?? {}
    );
  }

  /**
   * Delete a conversation
   */
  deleteConversation(id: string): Observable<void> {
    return this.http.delete<void>(
      `${this.getBaseUrl()}${this.config.pathBase}/chat/conversations/${id}`
    );
  }

  /**
   * Get messages for a conversation
   */
  getMessages(conversationId: string): Observable<ChatMessage[]> {
    return this.http.get<ChatMessage[]>(
      `${this.getBaseUrl()}${this.config.pathBase}/chat/conversations/${conversationId}/messages`
    );
  }

  /**
   * Connect to the SignalR chat hub
   * @param isAdmin true to use the admin API URL, false for the user API URL
   */
  async connect(isAdmin: boolean): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected ||
        this.hubConnection?.state === signalR.HubConnectionState.Connecting) {
      return;
    }

    this._isAdmin = isAdmin;
    const token = this.authStore.accessToken();
    if (!token) {
      throw new Error('Not authenticated');
    }

    const hubUrl = `${this.getBaseUrl()}${this.config.pathBase}/hubs/chat`;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => this.authStore.accessToken() ?? ''
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Handle incoming chunks
    this.hubConnection.on('ReceiveChunk', (chunk: ChatStreamChunk) => {
      this.ngZone.run(() => {
        this._chunks$.next(chunk);
      });
    });

    // Handle errors from the hub
    this.hubConnection.on('Error', (message: string) => {
      this.ngZone.run(() => {
        this._errors$.next(message);
      });
    });

    // Handle LLM status response
    this.hubConnection.on('Status', (status: ChatLLMStatus) => {
      this.ngZone.run(() => {
        this._status$.next(status);
      });
    });

    // Handle sanitized service errors (LLM misconfiguration, etc.)
    this.hubConnection.on('ServiceError', (code: string) => {
      this.ngZone.run(() => {
        this._serviceErrors$.next(code);
      });
    });

    this.hubConnection.on('TitleUpdated', (update: { conversationId: string; title: string }) => {
      this.ngZone.run(() => {
        this._titleUpdates$.next(update);
      });
    });

    // Handle conversation created (from hub method)
    this.hubConnection.on('ConversationCreated', (conversation: ChatConversation) => {
      this.ngZone.run(() => {
        // This event can be used by the store to add the conversation
        this._chunks$.next({
          type: 'complete',
          content: JSON.stringify({ event: 'ConversationCreated', data: conversation }),
          isComplete: true,
          inputTokens: null,
          outputTokens: null
        });
      });
    });

    // Track connection state
    this.hubConnection.onreconnecting(() => {
      this.ngZone.run(() => {
        this._connectionState$.next(signalR.HubConnectionState.Reconnecting);
      });
    });

    this.hubConnection.onreconnected(() => {
      this.ngZone.run(() => {
        this._connectionState$.next(signalR.HubConnectionState.Connected);
      });
    });

    this.hubConnection.onclose(() => {
      this.ngZone.run(() => {
        this._connectionState$.next(signalR.HubConnectionState.Disconnected);
      });
    });

    try {
      await this.hubConnection.start();
      this._connectionState$.next(signalR.HubConnectionState.Connected);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to connect to chat hub';
      this._errors$.next(message);
      throw err;
    }
  }

  /**
   * Disconnect from the SignalR chat hub
   */
  async disconnect(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.hubConnection = null;
      this._connectionState$.next(signalR.HubConnectionState.Disconnected);
    }
  }

  /**
   * Send a message to a conversation via SignalR
   * Response chunks will be emitted via chunks$
   */
  async sendMessage(conversationId: string, message: string): Promise<void> {
    if (this.hubConnection?.state !== signalR.HubConnectionState.Connected) {
      throw new Error('Not connected to chat hub');
    }

    await this.hubConnection.invoke('SendMessage', conversationId, message);
  }

  /**
   * Create a conversation via SignalR
   * Result will be emitted via chunks$ with ConversationCreated event
   */
  async createConversationViaHub(title?: string): Promise<void> {
    if (this.hubConnection?.state !== signalR.HubConnectionState.Connected) {
      throw new Error('Not connected to chat hub');
    }

    await this.hubConnection.invoke('CreateConversation', title ?? null);
  }

  /**
   * Check LLM availability via SignalR hub
   */
  async checkStatus(): Promise<void> {
    if (this.hubConnection?.state !== signalR.HubConnectionState.Connected) {
      throw new Error('Not connected to chat hub');
    }

    await this.hubConnection.invoke('CheckStatus');
  }

  /**
   * Download a chat file by ID
   */
  downloadFile(fileId: string): Observable<Blob> {
    return this.http.get(
      `${this.getBaseUrl()}${this.config.pathBase}/chat/files/${fileId}`,
      { responseType: 'blob' }
    );
  }

  /**
   * Check if currently connected
   */
  get isConnected(): boolean {
    return this.hubConnection?.state === signalR.HubConnectionState.Connected;
  }
}

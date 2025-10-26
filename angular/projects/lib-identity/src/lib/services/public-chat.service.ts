import { Injectable, inject, NgZone } from '@angular/core';
import { Subject, BehaviorSubject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { ConfigService } from './config.service';

export interface PublicChatChunk {
  type: string;
  content: string;
  isComplete: boolean;
  inputTokens: number | null;
  outputTokens: number | null;
}

export interface PublicChatStatus {
  llmAvailable: boolean;
  providerName: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class PublicChatService {
  private readonly config = inject(ConfigService);
  private readonly ngZone = inject(NgZone);

  private hubConnection: signalR.HubConnection | null = null;
  private readonly _chunks$ = new Subject<PublicChatChunk>();
  private readonly _connectionState$ = new BehaviorSubject<signalR.HubConnectionState>(signalR.HubConnectionState.Disconnected);
  private readonly _errors$ = new Subject<string>();
  private readonly _serviceErrors$ = new Subject<string>();
  private readonly _status$ = new Subject<PublicChatStatus>();

  readonly chunks$ = this._chunks$.asObservable();
  readonly connectionState$ = this._connectionState$.asObservable();
  readonly errors$ = this._errors$.asObservable();
  readonly serviceErrors$ = this._serviceErrors$.asObservable();
  readonly status$ = this._status$.asObservable();

  async connect(): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected ||
        this.hubConnection?.state === signalR.HubConnectionState.Connecting) {
      return;
    }

    const hubUrl = `${this.config.userApiUrl}${this.config.pathBase}/hubs/public-chat`;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.hubConnection.on('ReceiveChunk', (chunk: PublicChatChunk) => {
      this.ngZone.run(() => this._chunks$.next(chunk));
    });

    this.hubConnection.on('Error', (message: string) => {
      this.ngZone.run(() => this._errors$.next(message));
    });

    this.hubConnection.on('Status', (status: PublicChatStatus) => {
      this.ngZone.run(() => this._status$.next(status));
    });

    this.hubConnection.on('ServiceError', (code: string) => {
      this.ngZone.run(() => this._serviceErrors$.next(code));
    });

    this.hubConnection.onreconnecting(() => {
      this.ngZone.run(() => this._connectionState$.next(signalR.HubConnectionState.Reconnecting));
    });

    this.hubConnection.onreconnected(() => {
      this.ngZone.run(() => this._connectionState$.next(signalR.HubConnectionState.Connected));
    });

    this.hubConnection.onclose(() => {
      this.ngZone.run(() => this._connectionState$.next(signalR.HubConnectionState.Disconnected));
    });

    try {
      await this.hubConnection.start();
      this._connectionState$.next(signalR.HubConnectionState.Connected);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to connect to public chat';
      this._errors$.next(message);
      throw err;
    }
  }

  async disconnect(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.hubConnection = null;
      this._connectionState$.next(signalR.HubConnectionState.Disconnected);
    }
  }

  async sendMessage(message: string): Promise<void> {
    if (this.hubConnection?.state !== signalR.HubConnectionState.Connected) {
      throw new Error('Not connected to public chat hub');
    }
    await this.hubConnection.invoke('SendMessage', message);
  }

  async checkStatus(): Promise<void> {
    if (this.hubConnection?.state !== signalR.HubConnectionState.Connected) {
      throw new Error('Not connected to public chat hub');
    }
    await this.hubConnection.invoke('CheckStatus');
  }

  get isConnected(): boolean {
    return this.hubConnection?.state === signalR.HubConnectionState.Connected;
  }
}

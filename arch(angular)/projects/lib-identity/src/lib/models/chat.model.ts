/**
 * Chat conversation summary
 */
export interface ChatConversation {
  id: string;
  title: string | null;
  startedUtc: string;
  endedUtc: string | null;
  createdUtc: string;
}

/**
 * Chat message with role and content
 */
export interface ChatMessage {
  id: string;
  role: 'user' | 'assistant' | 'tool';
  content: string;
  inputTokens: number | null;
  outputTokens: number | null;
  createdUtc: string;
}

/**
 * Streaming chunk received from SignalR
 */
export interface ChatStreamChunk {
  type: 'content' | 'tool_call' | 'tool_result' | 'complete' | 'error';
  content: string;
  isComplete: boolean;
  inputTokens: number | null;
  outputTokens: number | null;
}

/**
 * Request to create a new conversation
 */
export interface CreateConversationRequest {
  title?: string;
}

/**
 * LLM status received from SignalR CheckStatus
 */
export interface ChatLLMStatus {
  llmAvailable: boolean;
  providerName: string | null;
}

/**
 * Saved favorite prompt for the chat sidebar
 */
export interface ChatFavorite {
  id: string;
  name: string;
  prompt: string;
  pinned?: boolean;
}

/**
 * Chat state for NgRx Signals store
 */
export interface ChatState {
  conversations: ChatConversation[];
  currentConversation: ChatConversation | null;
  messages: ChatMessage[];
  isConnected: boolean;
  llmAvailable: boolean | null;
  llmError: boolean;
  isStreaming: boolean;
  streamingContent: string;
  isLoading: boolean;
  error: string | null;
}

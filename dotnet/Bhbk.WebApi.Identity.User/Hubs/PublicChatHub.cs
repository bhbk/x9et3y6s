using Bhbk.Lib.Identity.LLM.Abstractions;
using Bhbk.Lib.Identity.LLM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Bhbk.WebApi.Identity.User.Hubs
{
    [AllowAnonymous]
    public class PublicChatHub : Hub
    {
        private readonly ILLMProvider _llmProvider;
        private readonly ILogger<PublicChatHub> _logger;

        private static readonly ConcurrentDictionary<string, List<LLMMessage>> _connectionHistory
            = new ConcurrentDictionary<string, List<LLMMessage>>();

        private const int MaxHistoryMessages = 20;

        private const string SystemPrompt =
            "You are a helpful assistant for an identity management platform. " +
            "You can answer general questions about authentication, OAuth2, user management, " +
            "and how identity systems work. You do not have access to any user data, " +
            "system data, or administrative functions. If someone asks you to look up, " +
            "modify, or access specific user accounts or system configuration, politely " +
            "explain that you can only answer general questions and they should log in " +
            "to access their account. Keep responses concise and helpful.";

        public PublicChatHub([FromKeyedServices("Public")] ILLMProvider llmProvider, ILogger<PublicChatHub> logger)
        {
            _llmProvider = llmProvider;
            _logger = logger;
        }

        public async Task CheckStatus()
        {
            await Clients.Caller.SendAsync("Status", new
            {
                llmAvailable = _llmProvider != null,
                providerName = _llmProvider?.ProviderName
            });
        }

        public async Task SendMessage(string message)
        {
            if (_llmProvider == null)
            {
                await Clients.Caller.SendAsync("Error", "No LLM provider configured");
                return;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                await Clients.Caller.SendAsync("Error", "Message cannot be empty");
                return;
            }

            var connectionId = Context.ConnectionId;
            var history = _connectionHistory.GetOrAdd(connectionId, _ => new List<LLMMessage>());

            // Add user message to history
            history.Add(LLMMessage.User(message));

            // Trim history if too long (keep system prompt slot + recent messages)
            while (history.Count > MaxHistoryMessages)
            {
                history.RemoveAt(0);
            }

            // Build messages list with system prompt
            var messages = new List<LLMMessage> { LLMMessage.System(SystemPrompt) };
            messages.AddRange(history);

            try
            {
                var fullContent = new System.Text.StringBuilder();

                await foreach (var chunk in _llmProvider.StreamMessageAsync(messages))
                {
                    var normalizedType = NormalizeChunkType(chunk.Type);

                    if (normalizedType == "content")
                    {
                        fullContent.Append(chunk.Content);
                    }

                    await Clients.Caller.SendAsync("ReceiveChunk", new
                    {
                        type = normalizedType,
                        content = chunk.Content,
                        isComplete = chunk.IsComplete,
                        inputTokens = chunk.InputTokens,
                        outputTokens = chunk.OutputTokens
                    });
                }

                // Add assistant response to history
                var responseText = fullContent.ToString();
                if (!string.IsNullOrEmpty(responseText))
                {
                    history.Add(LLMMessage.Assistant(responseText));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LLM provider error during public chat SendMessage");
                await Clients.Caller.SendAsync("ServiceError", "LLM_SERVICE_ERROR");
            }
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _connectionHistory.TryRemove(Context.ConnectionId, out _);
            return base.OnDisconnectedAsync(exception);
        }

        private static string NormalizeChunkType(string providerType)
        {
            return providerType switch
            {
                "content_block_delta" => "content",
                "message_stop" => "complete",
                _ => providerType
            };
        }
    }
}

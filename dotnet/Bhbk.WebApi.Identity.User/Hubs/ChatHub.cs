using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.LLM.Abstractions;
using Bhbk.Lib.Identity.LLM.Models;
using Bhbk.Lib.Identity.LLM.Services;
using Bhbk.Lib.Identity.MCP.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.User.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ILLMProvider _llmProvider;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IEnumerable<ILLMProvider> llmProviders, IUnitOfWork uow, ILogger<ChatHub> logger)
        {
            _llmProvider = llmProviders.FirstOrDefault();
            _uow = uow;
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

        public async Task SendMessage(Guid conversationId, string message)
        {
            if (_llmProvider == null)
            {
                await Clients.Caller.SendAsync("Error", "No LLM provider configured");
                return;
            }

            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Verify conversation belongs to user
            var conversation = _uow.ChatConversations.Get(x => x.Id == conversationId && x.UserId == userId.Value).FirstOrDefault();
            if (conversation == null)
            {
                await Clients.Caller.SendAsync("Error", "Conversation not found");
                return;
            }

            /* Must check before the orchestrator saves the user message. */
            var isFirstMessage = !_uow.ChatMessages
                .Get(x => x.ConversationId == conversationId && x.Role == "user")
                .Any();

            try
            {
                var mcpContext = new UserMCPContext(_uow, userId.Value);
                var conversationService = new ConversationService(_uow);
                var orchestrator = new LLMOrchestrator(_llmProvider, mcpContext, conversationService);

                // Stream response
                await foreach (var chunk in orchestrator.ProcessMessageStreamAsync(conversationId, message))
                {
                    await Clients.Caller.SendAsync("ReceiveChunk", new
                    {
                        type = NormalizeChunkType(chunk.Type),
                        content = chunk.Content,
                        isComplete = chunk.IsComplete,
                        inputTokens = chunk.InputTokens,
                        outputTokens = chunk.OutputTokens,
                        fileId = chunk.FileId,
                        fileName = chunk.FileName,
                        fileSize = chunk.FileSize
                    });
                }

                if (isFirstMessage)
                {
                    try
                    {
                        var titleResponse = await _llmProvider.SendMessageAsync(new[]
                        {
                            LLMMessage.User($"Generate a concise title (6 words or fewer) for a conversation that starts with this message. Respond with only the title, no quotes, no punctuation at the end:\n\n{message}")
                        });
                        if (!string.IsNullOrWhiteSpace(titleResponse?.Content))
                        {
                            var title = titleResponse.Content.Trim().Trim('"').Trim('\'').Trim('.');
                            if (title.Length > 100) title = title[..100];
                            conversationService.UpdateConversationTitle(conversationId, title);
                            await Clients.Caller.SendAsync("TitleUpdated", new { conversationId, title });
                        }
                    }
                    catch (Exception titleEx)
                    {
                        _logger.LogWarning(titleEx, "Title generation failed for conversation {ConversationId} — non-fatal", conversationId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LLM provider error during SendMessage for conversation {ConversationId}", conversationId);
                await Clients.Caller.SendAsync("ServiceError", "LLM_SERVICE_ERROR");
            }
        }

        public async Task CreateConversation(string title)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            try
            {
                var conversationService = new ConversationService(_uow);
                var conversation = conversationService.CreateConversation(userId.Value, title);

                await Clients.Caller.SendAsync("ConversationCreated", new
                {
                    id = conversation.Id,
                    title = conversation.Title,
                    started = conversation.Started
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during CreateConversation");
                await Clients.Caller.SendAsync("ServiceError", "LLM_SERVICE_ERROR");
            }
        }

        private static string NormalizeChunkType(string providerType)
        {
            return providerType switch
            {
                "content_block_delta" => "content",
                "message_stop" => "complete",
                "tool_use" => "tool_call",
                "tool_execution" => "tool_result",
                "file" => "file",
                _ => providerType
            };
        }

        private Guid? GetUserId()
        {
            var claims = Context.User?.Identity as ClaimsIdentity;
            var userIdClaim = claims?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                return userId;

            return null;
        }
    }
}

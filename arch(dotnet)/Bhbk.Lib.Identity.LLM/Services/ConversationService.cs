using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.LLM.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Lib.Identity.LLM.Services
{
    public class ConversationService
    {
        private readonly IUnitOfWork _uow;

        public ConversationService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public tbl_ChatConversation CreateConversation(Guid userId, string title = null)
        {
            var conversation = new tbl_ChatConversation
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title ?? "New Conversation",
                StartedUtc = DateTimeOffset.UtcNow,
                IsDeleted = false,
                CreatedUtc = DateTimeOffset.UtcNow
            };

            _uow.ChatConversations.Create(conversation);
            _uow.Commit();

            return conversation;
        }

        public tbl_ChatConversation GetConversation(Guid conversationId)
        {
            return _uow.ChatConversations.Get(x => x.Id == conversationId).FirstOrDefault();
        }

        public IEnumerable<tbl_ChatConversation> GetUserConversations(Guid userId, int skip = 0, int take = 20)
        {
            return _uow.ChatConversations.Get(x => x.UserId == userId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedUtc)
                .Skip(skip)
                .Take(take)
                .ToList();
        }

        public IEnumerable<tbl_ChatMessage> GetConversationMessages(Guid conversationId)
        {
            return _uow.ChatMessages.Get(x => x.ConversationId == conversationId)
                .OrderBy(x => x.CreatedUtc)
                .ToList();
        }

        public tbl_ChatMessage AddMessage(
            Guid conversationId,
            string role,
            string content,
            string toolCalls = null,
            string toolResults = null,
            int? inputTokens = null,
            int? outputTokens = null)
        {
            var message = new tbl_ChatMessage
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                Role = role,
                Content = content,
                ToolCalls = toolCalls,
                ToolResults = toolResults,
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                CreatedUtc = DateTimeOffset.UtcNow
            };

            _uow.ChatMessages.Create(message);
            _uow.Commit();

            return message;
        }

        public void UpdateConversationTitle(Guid conversationId, string title)
        {
            var conversation = GetConversation(conversationId);
            if (conversation != null)
            {
                conversation.Title = title;
                conversation.ModifiedUtc = DateTimeOffset.UtcNow;
                _uow.ChatConversations.Update(conversation);
                _uow.Commit();
            }
        }

        public void EndConversation(Guid conversationId)
        {
            var conversation = GetConversation(conversationId);
            if (conversation != null)
            {
                conversation.EndedUtc = DateTimeOffset.UtcNow;
                conversation.ModifiedUtc = DateTimeOffset.UtcNow;
                _uow.ChatConversations.Update(conversation);
                _uow.Commit();
            }
        }

        public void DeleteConversation(Guid conversationId)
        {
            var conversation = GetConversation(conversationId);
            if (conversation != null)
            {
                conversation.IsDeleted = true;
                conversation.ModifiedUtc = DateTimeOffset.UtcNow;
                _uow.ChatConversations.Update(conversation);
                _uow.Commit();
            }
        }

        public IEnumerable<LLMMessage> BuildMessageHistory(Guid conversationId)
        {
            var messages = GetConversationMessages(conversationId);
            var llmMessages = new List<LLMMessage>();

            foreach (var msg in messages)
            {
                llmMessages.Add(new LLMMessage
                {
                    Role = msg.Role,
                    Content = msg.Content,
                    ToolCalls = !string.IsNullOrEmpty(msg.ToolCalls)
                        ? Newtonsoft.Json.Linq.JArray.Parse(msg.ToolCalls)
                        : null,
                    ToolResults = !string.IsNullOrEmpty(msg.ToolResults)
                        ? Newtonsoft.Json.Linq.JArray.Parse(msg.ToolResults)
                        : null
                });
            }

            return llmMessages;
        }

        public string GetSystemPrompt(string promptType)
        {
            var prompt = _uow.ChatPrompts.Get(x => x.PromptType == promptType && x.IsEnabled)
                .OrderBy(x => x.SortOrder)
                .FirstOrDefault();

            return prompt?.Content;
        }

        public IEnumerable<tbl_ChatPrompt> GetActivePrompts(string promptType)
        {
            return _uow.ChatPrompts.Get(x => x.PromptType == promptType && x.IsEnabled)
                .OrderBy(x => x.SortOrder)
                .ToList();
        }
    }
}

using Bhbk.Lib.Identity.LLM.Services;
using Bhbk.Lib.Identity.Primitives.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Bhbk.WebApi.Identity.Admin.Controllers
{
    [Route("chat")]
    [Authorize(Policy = PolicyConstants.IdentityAdminPolicy)]
    public class ChatController : BaseController
    {
        [Route("conversations"), HttpGet]
        public IActionResult GetConversations([FromQuery] int skip = 0, [FromQuery] int take = 20)
        {
            var userId = GetIdentityGUID();
            var conversationService = new ConversationService(uow);
            var conversations = conversationService.GetUserConversations(userId, skip, take);

            return Ok(conversations.Select(c => new
            {
                c.Id,
                c.Title,
                c.StartedUtc,
                c.EndedUtc,
                c.CreatedUtc
            }));
        }

        [Route("conversations/{id}"), HttpGet]
        public IActionResult GetConversation(Guid id)
        {
            var userId = GetIdentityGUID();
            var conversation = uow.ChatConversations.Get(x => x.Id == id && x.UserId == userId).FirstOrDefault();

            if (conversation == null)
                return NotFound();

            return Ok(new
            {
                conversation.Id,
                conversation.Title,
                conversation.StartedUtc,
                conversation.EndedUtc,
                conversation.CreatedUtc
            });
        }

        [Route("conversations"), HttpPost]
        public IActionResult CreateConversation([FromBody] CreateConversationRequest request)
        {
            var userId = GetIdentityGUID();
            var conversationService = new ConversationService(uow);
            var conversation = conversationService.CreateConversation(userId, request?.Title);

            return Created($"/chat/conversations/{conversation.Id}", new
            {
                conversation.Id,
                conversation.Title,
                conversation.StartedUtc,
                conversation.CreatedUtc
            });
        }

        [Route("conversations/{id}"), HttpDelete]
        public IActionResult DeleteConversation(Guid id)
        {
            var userId = GetIdentityGUID();
            var conversation = uow.ChatConversations.Get(x => x.Id == id && x.UserId == userId).FirstOrDefault();

            if (conversation == null)
                return NotFound();

            var conversationService = new ConversationService(uow);
            conversationService.DeleteConversation(id);

            return NoContent();
        }

        [Route("conversations/{id}/messages"), HttpGet]
        public IActionResult GetMessages(Guid id)
        {
            var userId = GetIdentityGUID();
            var conversation = uow.ChatConversations.Get(x => x.Id == id && x.UserId == userId).FirstOrDefault();

            if (conversation == null)
                return NotFound();

            var conversationService = new ConversationService(uow);
            var messages = conversationService.GetConversationMessages(id);

            return Ok(messages.Select(m => new
            {
                m.Id,
                m.Role,
                m.Content,
                m.InputTokens,
                m.OutputTokens,
                m.CreatedUtc
            }));
        }
    }

    public class CreateConversationRequest
    {
        public string Title { get; set; }
    }
}

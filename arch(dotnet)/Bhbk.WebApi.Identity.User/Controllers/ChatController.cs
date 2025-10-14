using Bhbk.Lib.Identity.LLM.Services;
using Bhbk.Lib.Identity.Primitives.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Bhbk.WebApi.Identity.User.Controllers
{
    [Route("chat")]
    [Authorize(Policy = PolicyConstants.OAuth2ROPGrants)]
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

        [Route("conversations/{conversationId}/files"), HttpGet]
        public IActionResult GetConversationFiles(Guid conversationId)
        {
            var userId = GetIdentityGUID();
            var conversation = uow.ChatConversations.Get(x => x.Id == conversationId && x.UserId == userId).FirstOrDefault();

            if (conversation == null)
                return NotFound();

            var files = uow.ChatFiles.Get(x => x.ConversationId == conversationId);

            return Ok(files.Select(f => new
            {
                f.Id,
                f.FileName,
                f.ContentType,
                f.FileSize,
                f.Summary,
                f.ExpiresUtc,
                f.CreatedUtc
            }));
        }

        [Route("files/{id}"), HttpGet]
        public IActionResult DownloadFile(Guid id)
        {
            var userId = GetIdentityGUID();
            var file = uow.ChatFiles.Get(x => x.Id == id).FirstOrDefault();

            if (file == null)
                return NotFound();

            /* Verify the file's conversation belongs to this user. */
            var conversation = uow.ChatConversations.Get(x => x.Id == file.ConversationId && x.UserId == userId).FirstOrDefault();
            if (conversation == null)
                return NotFound();

            if (file.ExpiresUtc < DateTimeOffset.UtcNow)
                return NotFound();

            return File(file.FileContent, file.ContentType, file.FileName);
        }
    }

    public class CreateConversationRequest
    {
        public string Title { get; set; }
    }
}
